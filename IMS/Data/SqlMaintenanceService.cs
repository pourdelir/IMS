using Microsoft.Data.SqlClient;

namespace IMS.Data;

public sealed class SqlMaintenanceService(SqlServerConnectionFactory connectionFactory)
{
    private readonly SqlServerConnectionFactory _connectionFactory = connectionFactory;

    public async Task<string> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        var counts = new Dictionary<string, long>();
        foreach (var table in new[] { "ArchiveProfiles", "Scans", "Files", "Directories", "Comparisons", "ScanErrors" })
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT_BIG(*) FROM dbo." + table + ";";
            counts[table] = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
        }
        await using var sizeCommand = connection.CreateCommand();
        sizeCommand.CommandText = "SELECT SUM(size) * 8 * 1024 FROM sys.database_files;";
        var size = Convert.ToInt64(await sizeCommand.ExecuteScalarAsync(cancellationToken));
        return $"Profiles {counts["ArchiveProfiles"]:N0} · scans {counts["Scans"]:N0} · files {counts["Files"]:N0} · directories {counts["Directories"]:N0} · comparisons {counts["Comparisons"]:N0} · errors {counts["ScanErrors"]:N0} · database files {size / 1024d / 1024:0.##} MB";
    }

    public async Task CheckDatabaseAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        var databaseName = connection.Database.Replace("]", "]]", StringComparison.Ordinal);
        await using var command = connection.CreateCommand();
        command.CommandText = $"DBCC CHECKDB ([{databaseName}]) WITH NO_INFOMSGS;";
        command.CommandTimeout = 0;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
