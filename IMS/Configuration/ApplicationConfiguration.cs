namespace IMS.Configuration;

/// <summary>Settings persisted outside the monitored archives.</summary>
public sealed class ApplicationConfiguration
{
    public const int CurrentFormatVersion = 1;

    public int FormatVersion { get; set; } = CurrentFormatVersion;
    public DatabaseConfiguration Database { get; set; } = new();
    public List<ArchiveProfileConfiguration> Profiles { get; set; } = [];
    public string ExportDirectory { get; set; } = string.Empty;
    public bool DisplayTimestampsInUtc { get; set; } = true;

    /// <summary>Delay in seconds before each queued scan starts; configurable in settings.json.</summary>
    public int ScanStartDelaySeconds { get; set; } = 5;
}

public sealed class DatabaseConfiguration
{
    public string Server { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "ArchiveAuditor";
    public bool UseIntegratedSecurity { get; set; } = true;
    public string UserName { get; set; } = string.Empty;
    /// <summary>SQL password stored in the local JSON configuration for this release.</summary>
    public string Password { get; set; } = string.Empty;
}

public sealed class ArchiveProfileConfiguration
{
    public string ProfileName { get; set; } = string.Empty;
    public string RootPath { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
