using IMS.Domain;
using IMS.Services;
using Microsoft.Data.SqlClient;

namespace IMS.Data;

public sealed class SqlScanRepository(SqlServerConnectionFactory connectionFactory) : IScanRepository
{
    private readonly SqlServerConnectionFactory _connectionFactory = connectionFactory;

    /// <summary>Creates a durable queued scan row and snapshots the profile root path.</summary>
    public async Task<long> EnqueueScanAsync(int profileId, string rootPath, DateTime scheduledAtUtc, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);
        var fullPath = Path.GetFullPath(rootPath);
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT dbo.Scans (ProfileId, RootPath, ScheduledAtUtc, Status) OUTPUT INSERTED.ScanId SELECT ProfileId, @RootPath, @ScheduledAtUtc, 'Queued' FROM dbo.ArchiveProfiles WHERE ProfileId = @ProfileId AND IsActive = 1;";
        command.Parameters.Add(new SqlParameter("@ProfileId", System.Data.SqlDbType.Int) { Value = profileId });
        command.Parameters.Add(new SqlParameter("@RootPath", System.Data.SqlDbType.NVarChar, 1000) { Value = fullPath });
        command.Parameters.Add(new SqlParameter("@ScheduledAtUtc", System.Data.SqlDbType.DateTime2) { Value = scheduledAtUtc });
        var result = await command.ExecuteScalarAsync(cancellationToken);
        if (result is null or DBNull)
            throw new InvalidOperationException($"Active archive profile {profileId} was not found.");

        return Convert.ToInt64(result, System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>Loads pending jobs for the queue grid in execution order.</summary>
    public async Task<IReadOnlyList<QueuedScan>> GetQueuedScansAsync(CancellationToken cancellationToken = default)
    {
        var scans = new List<QueuedScan>();
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT s.ScanId, s.ProfileId, p.ProfileName, s.RootPath, s.QueuedAtUtc, s.ScheduledAtUtc FROM dbo.Scans s INNER JOIN dbo.ArchiveProfiles p ON p.ProfileId = s.ProfileId WHERE s.Status = 'Queued' ORDER BY s.ScheduledAtUtc, s.QueuedAtUtc, s.ScanId;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            scans.Add(new QueuedScan(reader.GetInt64(0), reader.GetInt32(1), reader.GetString(2), reader.GetString(3),
                reader.GetDateTime(4), reader.GetDateTime(5)));
        return scans;
    }

    /// <summary>Returns the next queued job, including one scheduled for later.</summary>
    public async Task<QueuedScan?> GetNextQueuedScanAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT TOP (1) s.ScanId, s.ProfileId, p.ProfileName, s.RootPath, s.QueuedAtUtc, s.ScheduledAtUtc FROM dbo.Scans s INNER JOIN dbo.ArchiveProfiles p ON p.ProfileId = s.ProfileId WHERE s.Status = 'Queued' ORDER BY s.ScheduledAtUtc, s.QueuedAtUtc, s.ScanId;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        return new QueuedScan(reader.GetInt64(0), reader.GetInt32(1), reader.GetString(2), reader.GetString(3),
            reader.GetDateTime(4), reader.GetDateTime(5));
    }

    /// <summary>Claims a due queued row. The status condition prevents duplicate starts.</summary>
    public async Task<bool> TryStartQueuedScanAsync(long scanId, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE dbo.Scans SET Status = 'Running', StartedAtUtc = SYSUTCDATETIME() WHERE ScanId = @ScanId AND Status = 'Queued' AND ScheduledAtUtc <= SYSUTCDATETIME();";
        command.Parameters.Add(new SqlParameter("@ScanId", System.Data.SqlDbType.BigInt) { Value = scanId });
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    /// <summary>Marks a pending queue item Aborted while preserving its database row.</summary>
    public async Task<bool> AbortQueuedScanAsync(long scanId, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE dbo.Scans SET Status = 'Aborted', CompletedAtUtc = SYSUTCDATETIME() WHERE ScanId = @ScanId AND Status = 'Queued';";
        command.Parameters.Add(new SqlParameter("@ScanId", System.Data.SqlDbType.BigInt) { Value = scanId });
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    /// <summary>Keeps only one app instance responsible for queue execution.</summary>
    public async Task<SqlConnection?> TryAcquireQueueWorkerLockAsync(CancellationToken cancellationToken = default)
    {
        var connection = _connectionFactory.CreateConnection();
        try
        {
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = "DECLARE @LockResult int; EXEC @LockResult = sys.sp_getapplock @Resource = N'ArchiveAuditor.ScanQueueWorker', @LockMode = 'Exclusive', @LockOwner = 'Session', @LockTimeout = 0; SELECT @LockResult;";
            var result = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken), System.Globalization.CultureInfo.InvariantCulture);
            if (result >= 0) return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }

        await connection.DisposeAsync();
        return null;
    }

    /// <summary>Finalizes rows left Running after an unclean app or computer shutdown.</summary>
    public async Task RecoverInterruptedScansAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = @"
DECLARE @Interrupted TABLE (ScanId bigint NOT NULL PRIMARY KEY);
UPDATE dbo.Scans
SET Status = 'Failed',
    CompletedAtUtc = SYSUTCDATETIME(),
    ScanDurationSeconds = COALESCE(ScanDurationSeconds, DATEDIFF_BIG(second, StartedAtUtc, SYSUTCDATETIME())),
    ErrorCount = ErrorCount + 1
OUTPUT INSERTED.ScanId INTO @Interrupted (ScanId)
WHERE Status = 'Running';

INSERT dbo.ScanErrors (ScanId, RelativePath, ErrorType, ErrorMessage, OccurredAtUtc)
SELECT ScanId, NULL, N'ApplicationInterrupted',
       N'The application or computer stopped before this scan could be finalized.', SYSUTCDATETIME()
FROM @Interrupted;";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task CompleteScanAsync(long scanId, ScanStatus status, long totalFiles, long totalDirectories,
        long totalSizeBytes, long totalHashedBytes, long durationSeconds, int errorCount,
        CancellationToken cancellationToken = default)
    {
        if (status is ScanStatus.Queued or ScanStatus.Running or ScanStatus.Aborted)
            throw new ArgumentException("A final scan status is required.", nameof(status));
        if (status == ScanStatus.Completed && errorCount != 0)
            throw new InvalidOperationException("A scan with errors cannot be marked Completed.");

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE dbo.Scans SET Status = @Status, CompletedAtUtc = SYSUTCDATETIME(), TotalFiles = @TotalFiles, TotalDirectories = @TotalDirectories, TotalSizeBytes = @TotalSizeBytes, TotalHashedBytes = @TotalHashedBytes, ScanDurationSeconds = @DurationSeconds, ErrorCount = @ErrorCount WHERE ScanId = @ScanId AND Status = 'Running';";
        command.Parameters.Add(new SqlParameter("@Status", System.Data.SqlDbType.VarChar, 20) { Value = status.ToString() });
        command.Parameters.Add(new SqlParameter("@TotalFiles", System.Data.SqlDbType.BigInt) { Value = totalFiles });
        command.Parameters.Add(new SqlParameter("@TotalDirectories", System.Data.SqlDbType.BigInt) { Value = totalDirectories });
        command.Parameters.Add(new SqlParameter("@TotalSizeBytes", System.Data.SqlDbType.BigInt) { Value = totalSizeBytes });
        command.Parameters.Add(new SqlParameter("@TotalHashedBytes", System.Data.SqlDbType.BigInt) { Value = totalHashedBytes });
        command.Parameters.Add(new SqlParameter("@DurationSeconds", System.Data.SqlDbType.BigInt) { Value = durationSeconds });
        command.Parameters.Add(new SqlParameter("@ErrorCount", System.Data.SqlDbType.Int) { Value = errorCount });
        command.Parameters.Add(new SqlParameter("@ScanId", System.Data.SqlDbType.BigInt) { Value = scanId });
        if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
            throw new InvalidOperationException($"Scan {scanId} was not running or does not exist; its historical record was not changed.");
    }
}
