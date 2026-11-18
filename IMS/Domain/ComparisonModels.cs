namespace IMS.Domain;

public sealed record ComparisonChange(string RecordType, string RelativePath, string ChangeType,
    long? PreviousSize, long? CurrentSize, string? PreviousHash, string? CurrentHash);

public sealed record ComparisonResult(long ComparisonId, int ProfileId, ScanSnapshot Previous,
    ScanSnapshot Current, DateTime ComparedAtUtc, long UnchangedFiles, long SizeDifferenceBytes,
    IReadOnlyList<ComparisonChange> Changes);
