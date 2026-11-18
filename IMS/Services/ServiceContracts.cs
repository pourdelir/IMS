using IMS.Domain;

namespace IMS.Services;

public interface IArchiveProfileService
{
    Task<IReadOnlyList<ArchiveProfile>> GetProfilesAsync(bool activeOnly, CancellationToken cancellationToken = default);
    Task<int> AddProfileAsync(string profileName, string rootPath, CancellationToken cancellationToken = default);
    Task EditProfileAsync(int profileId, string profileName, string rootPath, CancellationToken cancellationToken = default);
    Task DeactivateProfileAsync(int profileId, CancellationToken cancellationToken = default);
    Task ActivateProfileAsync(int profileId, CancellationToken cancellationToken = default);
}

public interface IScanRepository
{
    Task<long> EnqueueScanAsync(int profileId, string rootPath, DateTime scheduledAtUtc, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<QueuedScan>> GetQueuedScansAsync(CancellationToken cancellationToken = default);
    Task<QueuedScan?> GetNextQueuedScanAsync(CancellationToken cancellationToken = default);
    Task<bool> TryStartQueuedScanAsync(long scanId, CancellationToken cancellationToken = default);
    Task<bool> AbortQueuedScanAsync(long scanId, CancellationToken cancellationToken = default);
    Task RecoverInterruptedScansAsync(CancellationToken cancellationToken = default);
    Task CompleteScanAsync(long scanId, ScanStatus status, long totalFiles, long totalDirectories,
        long totalSizeBytes, long totalHashedBytes, long durationSeconds, int errorCount,
        CancellationToken cancellationToken = default);
}

public sealed record ScanProgress(long FilesProcessed, long DirectoriesProcessed, long BytesHashed,
    long TotalSizeDiscovered, string CurrentDirectory, string? CurrentFile, int ErrorCount, long ScanId = 0,
    string Operation = "OperationStatus: Scanning...", int CommitCount = 0);

public interface IScanDataWriter
{
    Task WriteFilesAsync(long scanId, IReadOnlyCollection<IMS.Domain.FileSnapshotRecord> files, CancellationToken cancellationToken = default);
    Task WriteDirectoriesAsync(long scanId, IReadOnlyCollection<IMS.Domain.DirectorySnapshotRecord> directories, CancellationToken cancellationToken = default);
    Task WriteErrorsAsync(long scanId, IReadOnlyCollection<IMS.Domain.ScanErrorRecord> errors, CancellationToken cancellationToken = default);
}

public interface IArchiveScanService
{
    Task<long> ScanAsync(long scanId, IMS.Domain.ArchiveProfile profile, IProgress<ScanProgress>? progress = null,
        CancellationToken cancellationToken = default);
}

public interface ISchemaService
{
    Task<int> GetSchemaVersionAsync(CancellationToken cancellationToken = default);
    Task EnsureCompatibleAsync(CancellationToken cancellationToken = default);
}
