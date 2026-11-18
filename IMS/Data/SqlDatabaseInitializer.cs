using System.Reflection;
using System.Text.RegularExpressions;
using IMS.Configuration;
using Microsoft.Data.SqlClient;
using AppConfig = IMS.Configuration.ApplicationConfiguration;

namespace IMS.Data;

public sealed class SqlDatabaseInitializer(AppConfig configuration, SqlServerConnectionFactory connectionFactory)
{
    private readonly AppConfig _configuration = configuration;
    private readonly SqlServerConnectionFactory _connectionFactory = connectionFactory;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var databaseName = _configuration.Database.DatabaseName;
        if (!System.Text.RegularExpressions.Regex.IsMatch(databaseName, "^[A-Za-z][A-Za-z0-9_]{0,127}$"))
            throw new InvalidOperationException("Use a database name beginning with a letter and containing only letters, numbers, or underscores.");
        if (new[] { "master", "model", "msdb", "tempdb" }.Contains(databaseName, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("Select an application database name, not a SQL Server system database.");

        await using var connection = _connectionFactory.CreateConnection(connectToMaster: true);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandTimeout = 0;

        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith("001_CreateArchiveAuditor.sql", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("The approved database creation script is missing from the application.");
        await using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        var script = await reader.ReadToEndAsync(cancellationToken);
        script = Regex.Replace(script, "ArchiveAuditor", databaseName, RegexOptions.IgnoreCase);

        // SQL Server Management Studio's GO separator is a client directive, so execute each batch individually.
        foreach (var batch in Regex.Split(script, @"^\s*GO\s*(?:--.*)?$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
                     .Where(batch => !string.IsNullOrWhiteSpace(batch)))
        {
            command.CommandText = batch;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
