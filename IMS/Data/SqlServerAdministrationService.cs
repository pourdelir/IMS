using IMS.Configuration;
using Microsoft.Data.SqlClient;
using AppConfig = IMS.Configuration.ApplicationConfiguration;
using System.Text.RegularExpressions;

namespace IMS.Data;

/// <summary>Connection checks and database discovery used by first-run setup.</summary>
public sealed class SqlServerAdministrationService(AppConfig configuration)
{
    private readonly SqlServerConnectionFactory _connectionFactory = new(configuration);

    public async Task TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection(connectToMaster: true);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1;";
        await command.ExecuteScalarAsync(cancellationToken);
    }

    public async Task TestTargetDatabaseAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT 1;";
        await command.ExecuteScalarAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetAvailableDatabasesAsync(CancellationToken cancellationToken = default)
    {
        var names = new List<string>();
        await using var connection = _connectionFactory.CreateConnection(connectToMaster: true);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sys.databases WHERE state_desc = N'ONLINE' AND HAS_DBACCESS(name) = 1 ORDER BY name;";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            names.Add(reader.GetString(0));
        return names;
    }
}
