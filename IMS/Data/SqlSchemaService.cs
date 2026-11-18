using IMS.Services;
using Microsoft.Data.SqlClient;

namespace IMS.Data;

public sealed class SqlSchemaService(SqlServerConnectionFactory connectionFactory) : ISchemaService
{
    public const int SupportedVersion = 2;
    private readonly SqlServerConnectionFactory _connectionFactory = connectionFactory;

    public async Task<int> GetSchemaVersionAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT MAX(SchemaVersionId) FROM dbo.SchemaVersion;";
        var result = await command.ExecuteScalarAsync(cancellationToken);
        if (result is null or DBNull)
            throw new InvalidOperationException("The database does not contain a schema version.");

        return Convert.ToInt32(result, System.Globalization.CultureInfo.InvariantCulture);
    }

    public async Task EnsureCompatibleAsync(CancellationToken cancellationToken = default)
    {
        int version;
        try
        {
            version = await GetSchemaVersionAsync(cancellationToken);
        }
        catch (SqlException exception) when (exception.Number == 208)
        {
            throw new InvalidOperationException("The selected database is missing the Archive Auditor schema. Initialize it using the approved SQL script.", exception);
        }

        if (version != SupportedVersion)
            throw new InvalidOperationException($"Database schema version {version} is incompatible. This application requires version {SupportedVersion}.");
    }
}
