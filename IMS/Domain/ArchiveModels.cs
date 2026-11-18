namespace IMS.Domain;

public enum ScanStatus
{
    Queued,
    Running,
    Completed,
    CompletedWithErrors,
    Failed,
    Cancelled,
    Aborted
}

public sealed record ArchiveProfile(int ProfileId, string ProfileName, string RootPath, bool IsActive);

public sealed record ScanSnapshot(
    long ScanId,
    int ProfileId,
    string RootPath,
    DateTime StartedAtUtc,
    DateTime? CompletedAtUtc,
    ScanStatus Status,
    long TotalFiles,
    long TotalDirectories,
    long TotalSizeBytes,
    long TotalHashedBytes,
    long? ScanDurationSeconds,
    int ErrorCount);

/// <summary>A scan request waiting to start, read from dbo.Scans.</summary>
public sealed record QueuedScan(long ScanId, int ProfileId, string ProfileName, string RootPath,
    DateTime QueuedAtUtc, DateTime ScheduledAtUtc);

public sealed record FileSnapshotRecord(
    string RelativePath,
    string FileName,
    string Extension,
    long FileSizeBytes,
    DateTime? CreatedAtUtc,
    DateTime? ModifiedAtUtc,
    byte[] Sha256,
    int FileAttributes);

public sealed record DirectorySnapshotRecord(
    string RelativePath,
    long FileCount,
    long DirectoryCount,
    long TotalSizeBytes);

public sealed record ScanErrorRecord(string? RelativePath, string ErrorType, string ErrorMessage, DateTime OccurredAtUtc);
