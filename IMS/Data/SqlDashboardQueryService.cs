using IMS.Domain;
using Microsoft.Data.SqlClient;
using System.Data;

namespace IMS.Data;

public sealed class SqlDashboardQueryService(SqlServerConnectionFactory connectionFactory)
{
    private readonly SqlServerConnectionFactory _connectionFactory = connectionFactory;

    public async Task<IReadOnlyList<ScanSnapshot>> GetScansAsync(int profileId, bool completedOnly = false, CancellationToken cancellationToken = default)
    {
        var scans = new List<ScanSnapshot>();
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT ScanId, ProfileId, RootPath, StartedAtUtc, CompletedAtUtc, Status, TotalFiles, TotalDirectories, TotalSizeBytes, TotalHashedBytes, ScanDurationSeconds, ErrorCount FROM dbo.Scans WHERE ProfileId = @ProfileId AND ((@CompletedOnly = 0 AND Status NOT IN ('Queued','Aborted')) OR (@CompletedOnly = 1 AND Status IN ('Completed','CompletedWithErrors'))) ORDER BY StartedAtUtc DESC;";
        command.Parameters.Add("@ProfileId", SqlDbType.Int).Value = profileId;
        command.Parameters.Add("@CompletedOnly", SqlDbType.Bit).Value = completedOnly;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            scans.Add(new ScanSnapshot(reader.GetInt64(0), reader.GetInt32(1), reader.GetString(2), reader.GetDateTime(3),
                reader.IsDBNull(4) ? null : reader.GetDateTime(4), Enum.Parse<ScanStatus>(reader.GetString(5)),
                reader.GetInt64(6), reader.GetInt64(7), reader.GetInt64(8), reader.GetInt64(9),
                reader.IsDBNull(10) ? null : reader.GetInt64(10), reader.GetInt32(11)));
        return scans;
    }

    public async Task<IReadOnlyList<ScanErrorRecord>> GetErrorsAsync(long scanId, CancellationToken cancellationToken = default)
    {
        var errors = new List<ScanErrorRecord>();
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT RelativePath, ErrorType, ErrorMessage, OccurredAtUtc FROM dbo.ScanErrors WHERE ScanId = @ScanId ORDER BY ScanErrorId;";
        command.Parameters.Add("@ScanId", SqlDbType.BigInt).Value = scanId;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            errors.Add(new ScanErrorRecord(reader.IsDBNull(0) ? null : reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetDateTime(3)));
        return errors;
    }
}
