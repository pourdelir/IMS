namespace IMS.Services;

/// <summary>Windows-managed folders intentionally excluded when an archive root is a volume root.</summary>
internal static class ArchiveSystemFolderExclusions
{
    private static readonly HashSet<string> DriveRootFolderNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "System Volume Information",
        "$RECYCLE.BIN",
        "$WINDOWS.~BT"
    };

    public static bool IsExcludedDriveRootEntry(string rootPath, string entryPath)
    {
        var fullRoot = Path.GetFullPath(rootPath);
        var driveRoot = Path.GetPathRoot(fullRoot);
        if (string.IsNullOrEmpty(driveRoot) ||
            !string.Equals(fullRoot, driveRoot, StringComparison.OrdinalIgnoreCase))
            return false;

        var fullEntry = Path.GetFullPath(entryPath);
        var entryParent = Path.GetDirectoryName(fullEntry);
        if (!string.Equals(entryParent, fullRoot, StringComparison.OrdinalIgnoreCase))
            return false;

        return DriveRootFolderNames.Contains(Path.GetFileName(fullEntry));
    }

    public static bool IsExcludedRelativeFilePath(string rootPath, string relativePath)
    {
        var fullRoot = Path.GetFullPath(rootPath);
        var driveRoot = Path.GetPathRoot(fullRoot);
        if (string.IsNullOrEmpty(driveRoot) ||
            !string.Equals(fullRoot, driveRoot, StringComparison.OrdinalIgnoreCase))
            return false;

        var normalizedRelativePath = relativePath.Replace('\\', '/');
        var separatorIndex = normalizedRelativePath.IndexOf('/');
        if (separatorIndex <= 0)
            return false;

        return DriveRootFolderNames.Contains(normalizedRelativePath[..separatorIndex]);
    }
}
