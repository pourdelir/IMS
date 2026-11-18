using System.Security.Cryptography;
using System.Text;
using IMS.Data;
using IMS.Domain;

namespace IMS.Services;

public sealed record VerificationResult(long Missing, long Modified, long Additional, long Unreadable,
    long Checked, IReadOnlyList<string> SampleMessages);

/// <summary>Hashes the live archive against a completed snapshot without creating a new scan.</summary>
public sealed class ArchiveVerificationService(SqlServerConnectionFactory connectionFactory)
{
    private readonly SqlServerConnectionFactory _connectionFactory = connectionFactory;

    public async Task<VerificationResult> VerifyAsync(long baselineScanId, string rootPath,
        CancellationToken cancellationToken = default)
    {
        var fullRoot = Path.GetFullPath(rootPath);
        var baseline = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        await using (var connection = _connectionFactory.CreateConnection())
        {
            await connection.OpenAsync(cancellationToken);
            await using (var metadata = connection.CreateCommand())
            {
                metadata.CommandText = "SELECT RootPath FROM dbo.Scans WHERE ScanId = @ScanId AND Status = 'Completed';";
                metadata.Parameters.Add("@ScanId", System.Data.SqlDbType.BigInt).Value = baselineScanId;
                var savedRoot = await metadata.ExecuteScalarAsync(cancellationToken) as string;
                if (savedRoot is null) throw new InvalidOperationException("The selected baseline is not a completed snapshot.");
                if (!string.Equals(Path.GetFullPath(savedRoot), fullRoot, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("The current archive root differs from the selected snapshot root.");
            }
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT f.RelativePath, f.SHA256 FROM dbo.Files f INNER JOIN dbo.Scans s ON s.ScanId = f.ScanId WHERE f.ScanId = @ScanId AND s.Status = 'Completed';";
            command.Parameters.Add("@ScanId", System.Data.SqlDbType.BigInt).Value = baselineScanId;
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var relativePath = reader.GetString(0);
                if (ArchiveSystemFolderExclusions.IsExcludedRelativeFilePath(fullRoot, relativePath))
                    continue;
                baseline.Add(relativePath, (byte[])reader[1]);
            }
        }
        long modified = 0, additional = 0, unreadable = 0, checkedFiles = 0;
        var messages = new List<string>();
        void MarkUnverifiable(string relativePath, bool isDirectory = false)
        {
            if (!isDirectory) { baseline.Remove(relativePath); return; }
            var prefix = relativePath.Length == 0 ? string.Empty : relativePath.TrimEnd('/') + "/";
            foreach (var savedPath in baseline.Keys.Where(key => prefix.Length == 0 || key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToArray())
                baseline.Remove(savedPath);
        }
        var pending = new Stack<string>();
        pending.Push(fullRoot);
        while (pending.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var directory = pending.Pop();
            string[] entries;
            try { entries = Directory.GetFileSystemEntries(directory); }
            catch (Exception ex) when (IsFileSystemError(ex))
            {
                unreadable++;
                AddSample(messages, $"Unreadable directory: {directory} ({ex.Message})");
                var relativeDirectory = Path.GetRelativePath(fullRoot, directory).Replace(Path.DirectorySeparatorChar, '/');
                MarkUnverifiable(relativeDirectory == "." ? string.Empty : relativeDirectory, isDirectory: true);
                continue;
            }
            foreach (var entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                FileAttributes attributes;
                try { attributes = File.GetAttributes(entry); }
                catch (Exception ex) when (IsFileSystemError(ex))
                {
                    if (ArchiveSystemFolderExclusions.IsExcludedDriveRootEntry(fullRoot, entry))
                        continue;
                    unreadable++;
                    AddSample(messages, $"Unreadable item: {entry} ({ex.Message})");
                    var relativeItem = Path.GetRelativePath(fullRoot, entry).Replace(Path.DirectorySeparatorChar, '/');
                    MarkUnverifiable(relativeItem, isDirectory: true);
                    continue;
                }
                if ((attributes & FileAttributes.Directory) != 0 &&
                    ArchiveSystemFolderExclusions.IsExcludedDriveRootEntry(fullRoot, entry))
                    continue;
                if ((attributes & FileAttributes.ReparsePoint) != 0)
                {
                    unreadable++;
                    AddSample(messages, $"Skipped reparse point: {entry}");
                    MarkUnverifiable(Path.GetRelativePath(fullRoot, entry).Replace(Path.DirectorySeparatorChar, '/'),
                        (attributes & FileAttributes.Directory) != 0);
                    continue;
                }
                if ((attributes & FileAttributes.Directory) != 0) { pending.Push(entry); continue; }
                var relative = Path.GetRelativePath(fullRoot, entry).Replace(Path.DirectorySeparatorChar, '/');
                try
                {
                    await using var stream = new FileStream(entry, FileMode.Open, FileAccess.Read,
                        FileShare.ReadWrite | FileShare.Delete, 1024 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
                    var hash = await SHA256.HashDataAsync(stream, cancellationToken);
                    checkedFiles++;
                    if (baseline.Remove(relative, out var expected))
                    {
                        if (!CryptographicOperations.FixedTimeEquals(hash, expected))
                        {
                            modified++;
                            AddSample(messages, $"Modified: {relative}");
                        }
                    }
                    else
                    {
                        additional++;
                        AddSample(messages, $"Additional: {relative}");
                    }
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex) when (IsFileSystemError(ex))
                {
                    unreadable++;
                    AddSample(messages, $"Unreadable file: {relative} ({ex.Message})");
                    MarkUnverifiable(relative);
                }
            }
        }
        foreach (var missing in baseline.Keys)
        {
            cancellationToken.ThrowIfCancellationRequested();
            AddSample(messages, $"Missing: {missing}");
        }
        return new VerificationResult(baseline.Count, modified, additional, unreadable, checkedFiles, messages);
    }

    private static bool IsFileSystemError(Exception e) => e is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException or System.Security.SecurityException;
    private static void AddSample(List<string> list, string message) { if (list.Count < 100) list.Add(message); }
}
