using IMS.Domain;
using IMS.Services;
using Microsoft.Data.SqlClient;

namespace IMS.Data;

public sealed class SqlArchiveProfileService(SqlServerConnectionFactory connectionFactory) : IArchiveProfileService
{
    private readonly SqlServerConnectionFactory _connectionFactory = connectionFactory;

    public async Task<IReadOnlyList<ArchiveProfile>> GetProfilesAsync(bool activeOnly, CancellationToken cancellationToken = default)
    {
        var profiles = new List<ArchiveProfile>();
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT ProfileId, ProfileName, RootPath, IsActive FROM dbo.ArchiveProfiles WHERE (@ActiveOnly = 0 OR IsActive = 1) ORDER BY ProfileName;";
        command.Parameters.Add(new SqlParameter("@ActiveOnly", System.Data.SqlDbType.Bit) { Value = activeOnly });
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            profiles.Add(new ArchiveProfile(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetBoolean(3)));

        return profiles;
    }

    public async Task<int> AddProfileAsync(string profileName, string rootPath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);
        var normalizedPath = Path.GetFullPath(rootPath);
        if (!Directory.Exists(normalizedPath))
            throw new DirectoryNotFoundException($"The archive root directory does not exist: {normalizedPath}");

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT dbo.ArchiveProfiles (ProfileName, RootPath) OUTPUT INSERTED.ProfileId VALUES (@ProfileName, @RootPath);";
        command.Parameters.Add(new SqlParameter("@ProfileName", System.Data.SqlDbType.NVarChar, 100) { Value = profileName.Trim() });
        command.Parameters.Add(new SqlParameter("@RootPath", System.Data.SqlDbType.NVarChar, 1000) { Value = normalizedPath });
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken), System.Globalization.CultureInfo.InvariantCulture);
    }

    public async Task DeactivateProfileAsync(int profileId, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE dbo.ArchiveProfiles SET IsActive = 0 WHERE ProfileId = @ProfileId;";
        command.Parameters.Add(new SqlParameter("@ProfileId", System.Data.SqlDbType.Int) { Value = profileId });
        if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
            throw new KeyNotFoundException($"Archive profile {profileId} was not found.");
    }

    public async Task ActivateProfileAsync(int profileId, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE dbo.ArchiveProfiles SET IsActive = 1 WHERE ProfileId = @ProfileId AND IsActive = 0;";
        command.Parameters.Add(new SqlParameter("@ProfileId", System.Data.SqlDbType.Int) { Value = profileId });
        if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
            throw new KeyNotFoundException($"Deactivated archive profile {profileId} was not found.");
    }

    public async Task EditProfileAsync(int profileId, string profileName, string rootPath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);
        var normalizedPath = Path.GetFullPath(rootPath);
        if (!Directory.Exists(normalizedPath)) throw new DirectoryNotFoundException($"The archive root directory does not exist: {normalizedPath}");
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE dbo.ArchiveProfiles SET ProfileName = @Name, RootPath = @Path WHERE ProfileId = @Id AND IsActive = 1;";
        command.Parameters.Add("@Name", System.Data.SqlDbType.NVarChar, 100).Value = profileName.Trim();
        command.Parameters.Add("@Path", System.Data.SqlDbType.NVarChar, 1000).Value = normalizedPath;
        command.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = profileId;
        if (await command.ExecuteNonQueryAsync(cancellationToken) != 1) throw new KeyNotFoundException($"Active archive profile {profileId} was not found.");
    }
}
