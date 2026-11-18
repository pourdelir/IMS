using System.Data;
using IMS.Domain;
using Microsoft.Data.SqlClient;

namespace IMS.Data;

public sealed class SqlComparisonService(SqlServerConnectionFactory connectionFactory)
{
    private readonly SqlServerConnectionFactory _connectionFactory = connectionFactory;

    public async Task<ComparisonResult> CompareAsync(int profileId, long previousScanId, long currentScanId,
        CancellationToken cancellationToken = default)
    {
        if (previousScanId == currentScanId) throw new ArgumentException("Choose two different scans.");
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        var scans = new Dictionary<long, ScanSnapshot>();
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT ScanId, ProfileId, RootPath, StartedAtUtc, CompletedAtUtc, Status, TotalFiles, TotalDirectories, TotalSizeBytes, TotalHashedBytes, ScanDurationSeconds, ErrorCount FROM dbo.Scans WHERE ProfileId = @ProfileId AND ScanId IN (@PreviousId, @CurrentId) AND Status IN ('Completed', 'CompletedWithErrors');";
            command.Parameters.Add("@ProfileId", SqlDbType.Int).Value = profileId;
            command.Parameters.Add("@PreviousId", SqlDbType.BigInt).Value = previousScanId;
            command.Parameters.Add("@CurrentId", SqlDbType.BigInt).Value = currentScanId;
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var scan = new ScanSnapshot(reader.GetInt64(0), reader.GetInt32(1), reader.GetString(2), reader.GetDateTime(3),
                    reader.IsDBNull(4) ? null : reader.GetDateTime(4), Enum.Parse<ScanStatus>(reader.GetString(5)), reader.GetInt64(6),
                    reader.GetInt64(7), reader.GetInt64(8), reader.GetInt64(9), reader.IsDBNull(10) ? null : reader.GetInt64(10), reader.GetInt32(11));
                scans.Add(scan.ScanId, scan);
            }
        }
        if (scans.Count != 2) throw new InvalidOperationException("Both selected scans must be completed snapshots (with or without scan errors) from this profile.");
        var previous = scans[previousScanId];
        var current = scans[currentScanId];
        if (!string.Equals(previous.RootPath, current.RootPath, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("These snapshots used different archive roots and cannot be compared as the same profile.");

        var changes = new List<ComparisonChange>();
        const string fileSql = @"
SELECT 'File', COALESCE(c.RelativePath, p.RelativePath),
       CASE WHEN p.FileRecordId IS NULL THEN 'Added' WHEN c.FileRecordId IS NULL THEN 'Deleted' ELSE 'Modified' END,
       p.FileSizeBytes, c.FileSizeBytes, p.SHA256, c.SHA256, p.RelativePath, c.RelativePath
FROM dbo.Files p FULL OUTER JOIN dbo.Files c
  ON c.ScanId = @CurrentId AND p.ScanId = @PreviousId AND p.RelativePathHash = c.RelativePathHash
WHERE (p.ScanId = @PreviousId OR p.ScanId IS NULL) AND (c.ScanId = @CurrentId OR c.ScanId IS NULL)
  AND (p.FileRecordId IS NULL OR c.FileRecordId IS NULL OR p.SHA256 <> c.SHA256 OR p.RelativePath COLLATE Latin1_General_100_BIN2 <> c.RelativePath COLLATE Latin1_General_100_BIN2)
ORDER BY COALESCE(c.RelativePath, p.RelativePath);";
        await ReadChangesAsync(connection, fileSql, previousScanId, currentScanId, "File", changes, cancellationToken);
        const string directorySql = @"
SELECT 'Directory', COALESCE(c.RelativePath, p.RelativePath),
       CASE WHEN p.DirectoryRecordId IS NULL THEN 'Added' ELSE 'Deleted' END,
       NULL, NULL, NULL, NULL, p.RelativePath, c.RelativePath
FROM dbo.Directories p FULL OUTER JOIN dbo.Directories c
  ON c.ScanId = @CurrentId AND p.ScanId = @PreviousId AND p.RelativePathHash = c.RelativePathHash
WHERE (p.ScanId = @PreviousId OR p.ScanId IS NULL) AND (c.ScanId = @CurrentId OR c.ScanId IS NULL)
  AND (p.DirectoryRecordId IS NULL OR c.DirectoryRecordId IS NULL OR p.RelativePath COLLATE Latin1_General_100_BIN2 <> c.RelativePath COLLATE Latin1_General_100_BIN2)
ORDER BY COALESCE(c.RelativePath, p.RelativePath);";
        await ReadChangesAsync(connection, directorySql, previousScanId, currentScanId, "Directory", changes, cancellationToken);

        long unchanged;
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT COUNT_BIG(*) FROM dbo.Files p INNER JOIN dbo.Files c ON c.ScanId = @CurrentId AND p.ScanId = @PreviousId AND p.RelativePathHash = c.RelativePathHash AND p.RelativePath COLLATE Latin1_General_100_BIN2 = c.RelativePath COLLATE Latin1_General_100_BIN2 AND p.SHA256 = c.SHA256;";
            command.Parameters.Add("@PreviousId", SqlDbType.BigInt).Value = previousScanId;
            command.Parameters.Add("@CurrentId", SqlDbType.BigInt).Value = currentScanId;
            unchanged = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
        }
        long comparisonId;
        var addedFiles = changes.LongCount(c => c.RecordType == "File" && c.ChangeType == "Added");
        var deletedFiles = changes.LongCount(c => c.RecordType == "File" && c.ChangeType == "Deleted");
        var modifiedFiles = changes.LongCount(c => c.RecordType == "File" && c.ChangeType == "Modified");
        var addedDirectories = changes.LongCount(c => c.RecordType == "Directory" && c.ChangeType == "Added");
        var deletedDirectories = changes.LongCount(c => c.RecordType == "Directory" && c.ChangeType == "Deleted");
        var comparedAt = DateTime.UtcNow;
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = "INSERT dbo.Comparisons (ProfileId, PreviousScanId, CurrentScanId, ComparedAtUtc, AddedFiles, DeletedFiles, ModifiedFiles, UnchangedFiles, AddedDirectories, DeletedDirectories, TotalSizeDifferenceBytes) OUTPUT INSERTED.ComparisonId VALUES (@ProfileId,@PreviousId,@CurrentId,@At,@Added,@Deleted,@Modified,@Unchanged,@AddedDirs,@DeletedDirs,@SizeDiff);";
            command.Parameters.Add("@ProfileId", SqlDbType.Int).Value = profileId;
            command.Parameters.Add("@PreviousId", SqlDbType.BigInt).Value = previousScanId;
            command.Parameters.Add("@CurrentId", SqlDbType.BigInt).Value = currentScanId;
            command.Parameters.Add("@At", SqlDbType.DateTime2).Value = comparedAt;
            command.Parameters.Add("@Added", SqlDbType.BigInt).Value = addedFiles;
            command.Parameters.Add("@Deleted", SqlDbType.BigInt).Value = deletedFiles;
            command.Parameters.Add("@Modified", SqlDbType.BigInt).Value = modifiedFiles;
            command.Parameters.Add("@Unchanged", SqlDbType.BigInt).Value = unchanged;
            command.Parameters.Add("@AddedDirs", SqlDbType.BigInt).Value = addedDirectories;
            command.Parameters.Add("@DeletedDirs", SqlDbType.BigInt).Value = deletedDirectories;
            command.Parameters.Add("@SizeDiff", SqlDbType.BigInt).Value = current.TotalSizeBytes - previous.TotalSizeBytes;
            comparisonId = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
        }
        return new ComparisonResult(comparisonId, profileId, previous, current, comparedAt, unchanged,
            current.TotalSizeBytes - previous.TotalSizeBytes, changes);
    }

    private static async Task ReadChangesAsync(SqlConnection connection, string sql, long previousId, long currentId,
        string type, ICollection<ComparisonChange> changes, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("@PreviousId", SqlDbType.BigInt).Value = previousId;
        command.Parameters.Add("@CurrentId", SqlDbType.BigInt).Value = currentId;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var oldPath = reader.IsDBNull(7) ? null : reader.GetString(7);
            var newPath = reader.IsDBNull(8) ? null : reader.GetString(8);
            if (oldPath is not null && newPath is not null && !string.Equals(oldPath, newPath, StringComparison.Ordinal))
                throw new InvalidOperationException("A RelativePathHash collision was detected; comparison stopped safely.");
            changes.Add(new ComparisonChange(type, reader.GetString(1), reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetInt64(3), reader.IsDBNull(4) ? null : reader.GetInt64(4),
                reader.IsDBNull(5) ? null : Convert.ToHexString((byte[])reader[5]),
                reader.IsDBNull(6) ? null : Convert.ToHexString((byte[])reader[6])));
        }
    }
}
