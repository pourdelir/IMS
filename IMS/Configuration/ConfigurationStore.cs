using System.Text.Json;

namespace IMS.Configuration;

public sealed class ConfigurationStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public string ConfigurationPath { get; }

    public ConfigurationStore(string? configurationPath = null)
    {
        ConfigurationPath = configurationPath ?? Path.Combine(AppContext.BaseDirectory, "settings.json");
    }

    public async Task<ApplicationConfiguration?> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(ConfigurationPath))
            return null;

        await using var stream = new FileStream(ConfigurationPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var configuration = await JsonSerializer.DeserializeAsync<ApplicationConfiguration>(stream, JsonOptions, cancellationToken);
        if (configuration is null || configuration.FormatVersion != ApplicationConfiguration.CurrentFormatVersion)
            throw new InvalidDataException("The configuration file is invalid or uses an unsupported format version.");

        configuration.Profiles ??= [];
        configuration.Database ??= new DatabaseConfiguration();
        if (configuration.ScanStartDelaySeconds is < 0 or > 3600)
            throw new InvalidDataException("ScanStartDelaySeconds in settings.json must be between 0 and 3600.");
        return configuration;
    }

    public async Task SaveAsync(ApplicationConfiguration configuration, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        configuration.FormatVersion = ApplicationConfiguration.CurrentFormatVersion;
        Directory.CreateDirectory(Path.GetDirectoryName(ConfigurationPath)!);

        var temporaryPath = ConfigurationPath + ".tmp";
        await using (var stream = new FileStream(temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await JsonSerializer.SerializeAsync(stream, configuration, JsonOptions, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }

        File.Move(temporaryPath, ConfigurationPath, overwrite: true);
    }
}
