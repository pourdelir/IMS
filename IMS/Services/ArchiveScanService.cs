using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using IMS.Data;
using IMS.Domain;

namespace IMS.Services;

/// <summary>Performs a complete, read-only inventory and hashes every discovered regular file.</summary>
public sealed class ArchiveScanService(IScanRepository scans, IScanDataWriter dataWriter, ApplicationLog log)
    : IArchiveScanService
{
    private const int FileBatchSize = 500;
    private const int ErrorBatchSize = 100;
    private readonly IScanRepository _scans = scans;
    private readonly IScanDataWriter _dataWriter = dataWriter;
    private readonly ApplicationLog _log = log;

    /// <summary>Executes an existing queued scan row; the queue worker already assigned its ScanId.</summary>
    public async Task<long> ScanAsync(long scanId, ArchiveProfile profile, IProgress<ScanProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);
        if (scanId <= 0) throw new ArgumentOutOfRangeException(nameof(scanId));

        var root = profile.RootPath;
        var timer = Stopwatch.StartNew();
        var files = new List<FileSnapshotRecord>(FileBatchSize);
        var errors = new List<ScanErrorRecord>(ErrorBatchSize);
        var directories = new Dictionary<string, DirectoryStats>(StringComparer.OrdinalIgnoreCase)
        {
            [string.Empty] = new()
        };
        var pendingDirectories = new Stack<string>();
        pendingDirectories.Push(root);
        var excludedSystemDirectories = new List<string>();
        long filesSeen = 0, directoriesSeen = 1, bytesHashed = 0, totalSize = 0;
        var errorCount = 0;
        var commitCount = 0;
        var lastProgress = Stopwatch.StartNew();

        void ReportOperation(string operation) => progress?.Report(new ScanProgress(filesSeen, directoriesSeen,
            bytesHashed, totalSize, string.Empty, null, errorCount, scanId, operation, commitCount));

        async Task FlushErrorsAsync(CancellationToken token)
        {
            if (errors.Count == 0) return;
            ReportOperation("Saving to SQL");
            await _dataWriter.WriteErrorsAsync(scanId, errors, token);
            commitCount++;
            errors.Clear();
            ReportOperation("Operation Status: Scanning...");
        }

        async Task FlushFilesAsync(CancellationToken token)
        {
            if (files.Count == 0) return;
            ReportOperation("Saving to SQL");
            await _dataWriter.WriteFilesAsync(scanId, files, token);
            commitCount++;
            files.Clear();
            ReportOperation("Operation Status: Scanning...");
        }

        async Task RecordErrorAsync(string? path, Exception exception)
        {
            errorCount++;
            errors.Add(new ScanErrorRecord(path, exception.GetType().Name, exception.Message, DateTime.UtcNow));
            if (errors.Count >= ErrorBatchSize)
                await FlushErrorsAsync(CancellationToken.None);
        }

        async Task PublishProgressAsync(string directory, string? file, bool force = false)
        {
            if (!force && lastProgress.ElapsedMilliseconds < 250) return;
            progress?.Report(new ScanProgress(filesSeen, directoriesSeen, bytesHashed, totalSize,
                directory, file, errorCount, scanId, "Operation Status: Scanning...", commitCount));
            lastProgress.Restart();
            await Task.CompletedTask;
        }

        try
        {
            // Validate after claiming the queued row so failures are finalized in SQL.
            root = Path.GetFullPath(root);
            if (!Directory.Exists(root))
                throw new DirectoryNotFoundException($"Archive root does not exist: {root}");

            await _log.WriteAsync("Information", "ScanStarted", $"Scan {scanId} started for profile {profile.ProfileId}.", cancellationToken: cancellationToken);
            while (pendingDirectories.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var currentDirectory = pendingDirectories.Pop();
                var relativeDirectory = RelativePath(root, currentDirectory);
                await PublishProgressAsync(relativeDirectory, null);

                string[] entries;
                try
                {
                    entries = Directory.GetFileSystemEntries(currentDirectory);
                }
                catch (Exception exception) when (IsFileSystemError(exception))
                {
                    await RecordErrorAsync(relativeDirectory, exception);
                    continue;
                }

                foreach (var entry in entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var relativePath = RelativePath(root, entry);
                    FileAttributes attributes;
                    try { attributes = File.GetAttributes(entry); }
                    catch (Exception exception) when (IsFileSystemError(exception))
                    {
                        if (ArchiveSystemFolderExclusions.IsExcludedDriveRootEntry(root, entry))
                        {
                            excludedSystemDirectories.Add(relativePath);
                            continue;
                        }
                        await RecordErrorAsync(relativePath, exception);
                        continue;
                    }

                    if ((attributes & FileAttributes.Directory) != 0 &&
                        ArchiveSystemFolderExclusions.IsExcludedDriveRootEntry(root, entry))
                    {
                        excludedSystemDirectories.Add(relativePath);
                        continue;
                    }

                    if ((attributes & FileAttributes.ReparsePoint) != 0)
                    {
                        await RecordErrorAsync(relativePath, new IOException("Reparse points are skipped to avoid following links outside the archive root."));
                        continue;
                    }

                    if ((attributes & FileAttributes.Directory) != 0)
                    {
                        directoriesSeen++;
                        directories.TryAdd(relativePath, new DirectoryStats());
                        AddDirectoryToAncestors(relativeDirectory, directories);
                        pendingDirectories.Push(entry);
                        continue;
                    }

                    filesSeen++;
                    try
                    {
                        var info = new FileInfo(entry);
                        info.Refresh();
                        var size = info.Length;
                        totalSize = checked(totalSize + size);
                        AddFileToAncestors(relativeDirectory, size, directories);
                        await PublishProgressAsync(relativeDirectory, relativePath);

                        await using var stream = new FileStream(entry, FileMode.Open, FileAccess.Read,
                            FileShare.ReadWrite | FileShare.Delete, 1024 * 1024,
                            FileOptions.Asynchronous | FileOptions.SequentialScan);
                        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
                        bytesHashed = checked(bytesHashed + stream.Length);
                        files.Add(new FileSnapshotRecord(relativePath, info.Name, info.Extension,
                            size, info.CreationTimeUtc, info.LastWriteTimeUtc, hash, (int)attributes));

                        if (files.Count >= FileBatchSize)
                        {
                            await FlushFilesAsync(cancellationToken);
                        }
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception exception) when (IsFileSystemError(exception))
                    {
                        await RecordErrorAsync(relativePath, exception);
                    }
                }
            }

            await FlushFilesAsync(cancellationToken);
            await FlushErrorsAsync(CancellationToken.None);
            var directoryRecords = directories.Select(pair => new DirectorySnapshotRecord(pair.Key,
                pair.Value.FileCount, pair.Value.DirectoryCount, pair.Value.TotalSizeBytes)).ToArray();
            ReportOperation("Saving to SQL");
            await _dataWriter.WriteDirectoriesAsync(scanId, directoryRecords, cancellationToken);
            commitCount++;
            ReportOperation("Operation Status: Scanning...");
            timer.Stop();
            var finalStatus = errorCount == 0 ? ScanStatus.Completed : ScanStatus.CompletedWithErrors;
            await _scans.CompleteScanAsync(scanId, finalStatus, filesSeen, directoriesSeen, totalSize,
                bytesHashed, (long)timer.Elapsed.TotalSeconds, errorCount, cancellationToken);
            progress?.Report(new ScanProgress(filesSeen, directoriesSeen, bytesHashed, totalSize,
                string.Empty, null, errorCount, scanId, "Operation Status: Ready", commitCount));
            await _log.WriteAsync("Information", "ScanCompleted",
                $"Scan {scanId} ended with status {finalStatus}; errors: {errorCount}; excluded drive-root system folders: " +
                (excludedSystemDirectories.Count == 0 ? "none" : string.Join(", ", excludedSystemDirectories)) + ".");
            return scanId;
        }
        catch (OperationCanceledException)
        {
            timer.Stop();
            // A bulk copy may have committed part of its current batch before cancellation.
            // Leave that batch untouched to avoid reinserting duplicate path keys.
            await FlushErrorsAsync(CancellationToken.None);
            await _scans.CompleteScanAsync(scanId, ScanStatus.Cancelled, filesSeen, directoriesSeen, totalSize,
                bytesHashed, (long)timer.Elapsed.TotalSeconds, errorCount, CancellationToken.None);
            await _log.WriteAsync("Information", "ScanCancelled", $"Scan {scanId} was cancelled.");
            throw;
        }
        catch (Exception exception)
        {
            timer.Stop();
            errorCount++;
            errors.Add(new ScanErrorRecord(null, exception.GetType().Name, exception.Message, DateTime.UtcNow));
            try
            {
                await FlushErrorsAsync(CancellationToken.None);
                await _scans.CompleteScanAsync(scanId, ScanStatus.Failed, filesSeen, directoriesSeen, totalSize,
                    bytesHashed, (long)timer.Elapsed.TotalSeconds, errorCount, CancellationToken.None);
            }
            catch (Exception finalizeException)
            {
                await _log.WriteAsync("Error", "ScanFinalizeFailed", $"Could not finalize scan {scanId}.", finalizeException);
            }
            await _log.WriteAsync("Error", "ScanFailed", $"Scan {scanId} failed.", exception);
            throw;
        }
    }

    private static bool IsFileSystemError(Exception exception) => exception is IOException or UnauthorizedAccessException
        or ArgumentException or NotSupportedException or System.Security.SecurityException;

    private static string RelativePath(string root, string path)
    {
        var relative = Path.GetRelativePath(root, path);
        return relative == "." ? string.Empty : relative.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');
    }

    private static void AddFileToAncestors(string path, long size, Dictionary<string, DirectoryStats> directories)
    {
        var current = path;
        while (true)
        {
            directories[current].FileCount++;
            directories[current].TotalSizeBytes = checked(directories[current].TotalSizeBytes + size);
            if (current.Length == 0) break;
            current = ParentRelativePath(current);
        }
    }

    private static void AddDirectoryToAncestors(string path, Dictionary<string, DirectoryStats> directories)
    {
        var current = path;
        while (true)
        {
            directories[current].DirectoryCount++;
            if (current.Length == 0) break;
            current = ParentRelativePath(current);
        }
    }

    private static string ParentRelativePath(string path)
    {
        var index = path.LastIndexOf('/');
        return index < 0 ? string.Empty : path[..index];
    }

    private sealed class DirectoryStats
    {
        public long FileCount { get; set; }
        public long DirectoryCount { get; set; }
        public long TotalSizeBytes { get; set; }
    }
}
