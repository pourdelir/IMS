using System.Data;
using IMS.Domain;
using Microsoft.Data.SqlClient;

namespace IMS.Data;

public sealed record DuplicateSummary(long DuplicateGroups, long ExtraCopies, long WastedBytes);
public sealed record LargeFilesSummary(long Count, long TotalBytes);

public sealed class SqlArchiveAnalysisService(SqlServerConnectionFactory connectionFactory)
{
    private readonly SqlServerConnectionFactory _connectionFactory = connectionFactory;

    public async Task<LargeFilesSummary> GetLargeFilesSummaryAsync(long scanId, long minimumSizeBytes, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT_BIG(*), COALESCE(SUM(FileSizeBytes), CONVERT(bigint, 0)) FROM dbo.Files WHERE ScanId = @ScanId AND FileSizeBytes >= @MinimumSizeBytes;";
        command.Parameters.Add("@ScanId", SqlDbType.BigInt).Value = scanId;
        command.Parameters.Add("@MinimumSizeBytes", SqlDbType.BigInt).Value = minimumSizeBytes;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return new LargeFilesSummary(reader.GetInt64(0), reader.GetInt64(1));
    }

    public async Task<DuplicateSummary> GetDuplicatesAsync(long scanId, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "WITH Groups AS (SELECT FileSizeBytes, SHA256, COUNT_BIG(*) AS Copies FROM dbo.Files WHERE ScanId = @ScanId GROUP BY FileSizeBytes, SHA256 HAVING COUNT_BIG(*) > 1) SELECT COUNT_BIG(*), COALESCE(SUM(Copies - 1),0), COALESCE(SUM((Copies - 1) * FileSizeBytes),0) FROM Groups;";
        command.Parameters.Add("@ScanId", SqlDbType.BigInt).Value = scanId;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return new DuplicateSummary(reader.GetInt64(0), reader.GetInt64(1), reader.GetInt64(2));
    }

    public async Task<(long EmptyDirectories, long Directories, long Files, long Bytes)> GetDirectoryStatisticsAsync(long scanId, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT SUM(CASE WHEN FileCount = 0 AND DirectoryCount = 0 THEN CONVERT(bigint,1) ELSE CONVERT(bigint,0) END), COUNT_BIG(*), COALESCE(MAX(CASE WHEN RelativePath = N'' THEN FileCount END),0), COALESCE(MAX(CASE WHEN RelativePath = N'' THEN TotalSizeBytes END),0) FROM dbo.Directories WHERE ScanId = @ScanId;";
        command.Parameters.Add("@ScanId", SqlDbType.BigInt).Value = scanId;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return (reader.IsDBNull(0) ? 0 : reader.GetInt64(0), reader.GetInt64(1), reader.GetInt64(2), reader.GetInt64(3));
    }
}
