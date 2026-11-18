/*
    Archive Auditor / IMS initial SQL Server schema.

    Review notes:
    - Run this script in SQL Server Management Studio or sqlcmd with permission
      to create a database. It creates and selects [ArchiveAuditor].
    - No historical data is deleted or replaced by this script.
    - Queue entries and scan history share dbo.Scans.
    - Only scans whose final status is Completed are eligible for full verification.
    - File/directory relative paths are indexed by SHA-256 path key because
      NVARCHAR(2000) exceeds SQL Server's index key byte limit. Application
      inserts must detect and reject a same-scan path-key collision by also
      comparing the full path.
*/

USE [master];
GO
IF DB_ID(N'ArchiveAuditor') IS NULL
BEGIN
    -- CREATE DATABASE must be the only statement in its batch, so execute it
    -- separately before sizing its initial data and log files.
    EXEC(N'CREATE DATABASE [ArchiveAuditor]');
    EXEC(N'ALTER DATABASE [ArchiveAuditor] MODIFY FILE (NAME = N''ArchiveAuditor'', SIZE = 64MB)');
    EXEC(N'ALTER DATABASE [ArchiveAuditor] MODIFY FILE (NAME = N''ArchiveAuditor_log'', SIZE = 64MB)');
END;
GO
USE [ArchiveAuditor];
GO

-- Disable identity caching on SQL Server 2017–2022, and only when it is still enabled.
-- The setting is unavailable in SQL Server 2025, so keep the command dynamic and version-guarded.
DECLARE @SqlMajorVersion int = TRY_CONVERT(int, SERVERPROPERTY('ProductMajorVersion'));
IF @SqlMajorVersion BETWEEN 14 AND 16
   AND EXISTS
   (
       SELECT 1
       FROM sys.database_scoped_configurations
       WHERE name = N'IDENTITY_CACHE' AND value <> 0
   )
BEGIN
    EXEC sys.sp_executesql N'ALTER DATABASE SCOPED CONFIGURATION SET IDENTITY_CACHE = OFF;';
END;
GO

IF OBJECT_ID(N'dbo.SchemaVersion', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SchemaVersion
(
    SchemaVersionId int NOT NULL,

    AppliedAtUtc datetime2(3) NOT NULL
        CONSTRAINT DF_SchemaVersion_AppliedAtUtc
        DEFAULT SYSUTCDATETIME(),

    AppliedBy nvarchar(128) NOT NULL
        CONSTRAINT DF_SchemaVersion_AppliedBy
        DEFAULT ORIGINAL_LOGIN(),

    Notes nvarchar(500) NULL,

    CONSTRAINT PK_SchemaVersion
        PRIMARY KEY (SchemaVersionId)
);

END;
GO

IF OBJECT_ID(N'dbo.ArchiveProfiles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ArchiveProfiles
    (
        ProfileId int IDENTITY(1,1) NOT NULL,
        ProfileName nvarchar(100) NOT NULL,
        RootPath nvarchar(1000) NOT NULL,
        IsActive bit NOT NULL CONSTRAINT DF_ArchiveProfiles_IsActive DEFAULT (1),
        CreatedAtUtc datetime2(3) NOT NULL CONSTRAINT DF_ArchiveProfiles_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_ArchiveProfiles PRIMARY KEY (ProfileId),
        CONSTRAINT UQ_ArchiveProfiles_ProfileName UNIQUE (ProfileName),
        CONSTRAINT UQ_ArchiveProfiles_ProfileId_RootPath UNIQUE (ProfileId, RootPath),
        CONSTRAINT CK_ArchiveProfiles_ProfileName_NotBlank CHECK (LEN(LTRIM(RTRIM(ProfileName))) > 0),
        CONSTRAINT CK_ArchiveProfiles_RootPath_NotBlank CHECK (LEN(LTRIM(RTRIM(RootPath))) > 0)
    );
    CREATE INDEX IX_ArchiveProfiles_IsActive ON dbo.ArchiveProfiles (IsActive, ProfileName);
END;
GO

IF OBJECT_ID(N'dbo.Scans', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Scans
    (
        ScanId bigint IDENTITY(1,1) NOT NULL,
        ProfileId int NOT NULL,
        RootPath nvarchar(1000) NOT NULL,
        QueuedAtUtc datetime2(3) NOT NULL CONSTRAINT DF_Scans_QueuedAtUtc DEFAULT SYSUTCDATETIME(),
        ScheduledAtUtc datetime2(3) NOT NULL CONSTRAINT DF_Scans_ScheduledAtUtc DEFAULT SYSUTCDATETIME(),
        StartedAtUtc datetime2(3) NULL,
        CompletedAtUtc datetime2(3) NULL,
        Status varchar(20) NOT NULL CONSTRAINT DF_Scans_Status DEFAULT ('Queued'),
        TotalFiles bigint NOT NULL CONSTRAINT DF_Scans_TotalFiles DEFAULT (0),
        TotalDirectories bigint NOT NULL CONSTRAINT DF_Scans_TotalDirectories DEFAULT (0),
        TotalSizeBytes bigint NOT NULL CONSTRAINT DF_Scans_TotalSizeBytes DEFAULT (0),
        TotalHashedBytes bigint NOT NULL CONSTRAINT DF_Scans_TotalHashedBytes DEFAULT (0),
        ScanDurationSeconds bigint NULL,
        ErrorCount int NOT NULL CONSTRAINT DF_Scans_ErrorCount DEFAULT (0),
        CONSTRAINT PK_Scans PRIMARY KEY (ScanId),
        CONSTRAINT UQ_Scans_ScanId_ProfileId UNIQUE (ScanId, ProfileId),
        CONSTRAINT FK_Scans_ArchiveProfiles FOREIGN KEY (ProfileId) REFERENCES dbo.ArchiveProfiles(ProfileId),
        CONSTRAINT CK_Scans_Status CHECK (Status IN ('Queued','Running','Completed','CompletedWithErrors','Failed','Cancelled','Aborted')),
        CONSTRAINT CK_Scans_Counts CHECK (TotalFiles >= 0 AND TotalDirectories >= 0 AND TotalSizeBytes >= 0 AND TotalHashedBytes >= 0 AND ErrorCount >= 0),
        CONSTRAINT CK_Scans_CompletedHasNoErrors CHECK (Status <> 'Completed' OR ErrorCount = 0),
        CONSTRAINT CK_Scans_Lifecycle CHECK
        (
            (Status = 'Queued' AND StartedAtUtc IS NULL AND CompletedAtUtc IS NULL)
            OR (Status = 'Running' AND StartedAtUtc IS NOT NULL AND CompletedAtUtc IS NULL)
            OR (Status IN ('Completed','CompletedWithErrors','Failed','Cancelled') AND StartedAtUtc IS NOT NULL AND CompletedAtUtc IS NOT NULL)
            OR (Status = 'Aborted' AND StartedAtUtc IS NULL AND CompletedAtUtc IS NOT NULL)
        )
    );
    CREATE INDEX IX_Scans_Profile_Status_Started ON dbo.Scans (ProfileId, Status, StartedAtUtc DESC) INCLUDE (CompletedAtUtc, TotalFiles, TotalDirectories, TotalSizeBytes, ScanDurationSeconds, ErrorCount);
    CREATE INDEX IX_Scans_Queued_Scheduled ON dbo.Scans (ScheduledAtUtc, QueuedAtUtc, ScanId) WHERE Status = 'Queued';
END;
GO

IF OBJECT_ID(N'dbo.Files', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Files
    (
        FileRecordId bigint IDENTITY(1,1) NOT NULL,
        ScanId bigint NOT NULL,
        RelativePath nvarchar(2000) NOT NULL,
        RelativePathHash binary(32) NOT NULL,
        FileName nvarchar(500) NOT NULL,
        Extension nvarchar(30) NOT NULL,
        FileSizeBytes bigint NOT NULL,
        CreatedAtUtc datetime2(3) NULL,
        ModifiedAtUtc datetime2(3) NULL,
        SHA256 binary(32) NOT NULL,
        FileAttributes int NOT NULL,
        CONSTRAINT PK_Files PRIMARY KEY (FileRecordId),
        CONSTRAINT FK_Files_Scans FOREIGN KEY (ScanId) REFERENCES dbo.Scans(ScanId),
        CONSTRAINT CK_Files_FileSizeBytes CHECK (FileSizeBytes >= 0)
    );
    CREATE UNIQUE INDEX UX_Files_ScanId_RelativePathHash ON dbo.Files (ScanId, RelativePathHash);
    CREATE INDEX IX_Files_ScanId_Size ON dbo.Files (ScanId, FileSizeBytes DESC) INCLUDE (RelativePath, SHA256);
    CREATE INDEX IX_Files_ScanId_Hash ON dbo.Files (ScanId, SHA256) INCLUDE (FileSizeBytes, RelativePath);
END;
GO

IF OBJECT_ID(N'dbo.Directories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Directories
    (
        DirectoryRecordId bigint IDENTITY(1,1) NOT NULL,
        ScanId bigint NOT NULL,
        RelativePath nvarchar(2000) NOT NULL,
        RelativePathHash binary(32) NOT NULL,
        FileCount bigint NOT NULL,
        DirectoryCount bigint NOT NULL,
        TotalSizeBytes bigint NOT NULL,
        CONSTRAINT PK_Directories PRIMARY KEY (DirectoryRecordId),
        CONSTRAINT FK_Directories_Scans FOREIGN KEY (ScanId) REFERENCES dbo.Scans(ScanId),
        CONSTRAINT CK_Directories_Counts CHECK (FileCount >= 0 AND DirectoryCount >= 0 AND TotalSizeBytes >= 0)
    );
    CREATE UNIQUE INDEX UX_Directories_ScanId_RelativePathHash ON dbo.Directories (ScanId, RelativePathHash);
END;
GO

IF OBJECT_ID(N'dbo.Comparisons', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Comparisons
    (
        ComparisonId bigint IDENTITY(1,1) NOT NULL,
        ProfileId int NOT NULL,
        PreviousScanId bigint NOT NULL,
        CurrentScanId bigint NOT NULL,
        ComparedAtUtc datetime2(3) NOT NULL CONSTRAINT DF_Comparisons_ComparedAtUtc DEFAULT SYSUTCDATETIME(),
        AddedFiles bigint NOT NULL,
        DeletedFiles bigint NOT NULL,
        ModifiedFiles bigint NOT NULL,
        UnchangedFiles bigint NOT NULL,
        AddedDirectories bigint NOT NULL,
        DeletedDirectories bigint NOT NULL,
        TotalSizeDifferenceBytes bigint NOT NULL,
        CONSTRAINT PK_Comparisons PRIMARY KEY (ComparisonId),
        CONSTRAINT FK_Comparisons_PreviousScan FOREIGN KEY (PreviousScanId, ProfileId) REFERENCES dbo.Scans(ScanId, ProfileId),
        CONSTRAINT FK_Comparisons_CurrentScan FOREIGN KEY (CurrentScanId, ProfileId) REFERENCES dbo.Scans(ScanId, ProfileId),
        CONSTRAINT CK_Comparisons_DifferentScans CHECK (PreviousScanId <> CurrentScanId),
        CONSTRAINT CK_Comparisons_Counts CHECK (AddedFiles >= 0 AND DeletedFiles >= 0 AND ModifiedFiles >= 0 AND UnchangedFiles >= 0 AND AddedDirectories >= 0 AND DeletedDirectories >= 0)
    );
    CREATE INDEX IX_Comparisons_Profile_ComparedAt ON dbo.Comparisons (ProfileId, ComparedAtUtc DESC);
END;
GO

IF OBJECT_ID(N'dbo.ScanErrors', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ScanErrors
    (
        ScanErrorId bigint IDENTITY(1,1) NOT NULL,
        ScanId bigint NOT NULL,
        RelativePath nvarchar(2000) NULL,
        ErrorType nvarchar(100) NOT NULL,
        ErrorMessage nvarchar(max) NOT NULL,
        OccurredAtUtc datetime2(3) NOT NULL CONSTRAINT DF_ScanErrors_OccurredAtUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_ScanErrors PRIMARY KEY (ScanErrorId),
        CONSTRAINT FK_ScanErrors_Scans FOREIGN KEY (ScanId) REFERENCES dbo.Scans(ScanId)
    );
    CREATE INDEX IX_ScanErrors_ScanId_OccurredAt ON dbo.ScanErrors (ScanId, OccurredAtUtc);
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE SchemaVersionId = 1)
    INSERT dbo.SchemaVersion (SchemaVersionId, Notes) VALUES (1, N'Initial Archive Auditor schema');
GO

/* Upgrade existing installations to schema version 2 without deleting scans. */
IF NOT EXISTS (SELECT 1 FROM dbo.SchemaVersion WHERE SchemaVersionId = 2)
BEGIN
    -- Add queue timestamps and backfill old scan rows from their existing timestamps.
    IF COL_LENGTH(N'dbo.Scans', N'QueuedAtUtc') IS NULL
        ALTER TABLE dbo.Scans ADD QueuedAtUtc datetime2(3) NULL;
    IF COL_LENGTH(N'dbo.Scans', N'ScheduledAtUtc') IS NULL
        ALTER TABLE dbo.Scans ADD ScheduledAtUtc datetime2(3) NULL;

    UPDATE dbo.Scans
    SET QueuedAtUtc = COALESCE(QueuedAtUtc, StartedAtUtc, CompletedAtUtc, SYSUTCDATETIME()),
        ScheduledAtUtc = COALESCE(ScheduledAtUtc, StartedAtUtc, QueuedAtUtc, SYSUTCDATETIME())
    WHERE QueuedAtUtc IS NULL OR ScheduledAtUtc IS NULL;

    ALTER TABLE dbo.Scans ALTER COLUMN QueuedAtUtc datetime2(3) NOT NULL;
    ALTER TABLE dbo.Scans ALTER COLUMN ScheduledAtUtc datetime2(3) NOT NULL;

    -- These objects reference StartedAtUtc, so drop them before making it nullable.
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Scans') AND name = N'IX_Scans_Profile_Status_Started')
        DROP INDEX IX_Scans_Profile_Status_Started ON dbo.Scans;
    IF OBJECT_ID(N'dbo.CK_Scans_Status', N'C') IS NOT NULL
        ALTER TABLE dbo.Scans DROP CONSTRAINT CK_Scans_Status;
    IF OBJECT_ID(N'dbo.CK_Scans_CompletedState', N'C') IS NOT NULL
        ALTER TABLE dbo.Scans DROP CONSTRAINT CK_Scans_CompletedState;
    IF OBJECT_ID(N'dbo.CK_Scans_Lifecycle', N'C') IS NOT NULL
        ALTER TABLE dbo.Scans DROP CONSTRAINT CK_Scans_Lifecycle;

    -- StartedAtUtc is populated only when a queued scan actually begins.
    DECLARE @StartedAtDefault sysname;
    DECLARE @DropConstraintSql nvarchar(4000);
    SELECT @StartedAtDefault = dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.Scans') AND c.name = N'StartedAtUtc';
    IF @StartedAtDefault IS NOT NULL
    BEGIN
        SET @DropConstraintSql = N'ALTER TABLE dbo.Scans DROP CONSTRAINT ' + QUOTENAME(@StartedAtDefault);
        EXEC sys.sp_executesql @DropConstraintSql;
    END;
    ALTER TABLE dbo.Scans ALTER COLUMN StartedAtUtc datetime2(3) NULL;

    -- The status default now represents a newly queued request.
    DECLARE @StatusDefault sysname;
    SELECT @StatusDefault = dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.Scans') AND c.name = N'Status';
    IF @StatusDefault IS NOT NULL
    BEGIN
        SET @DropConstraintSql = N'ALTER TABLE dbo.Scans DROP CONSTRAINT ' + QUOTENAME(@StatusDefault);
        EXEC sys.sp_executesql @DropConstraintSql;
    END;
    ALTER TABLE dbo.Scans ADD CONSTRAINT DF_Scans_Status DEFAULT ('Queued') FOR Status;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.default_constraints dc
        INNER JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
        WHERE dc.parent_object_id = OBJECT_ID(N'dbo.Scans') AND c.name = N'QueuedAtUtc'
    )
        ALTER TABLE dbo.Scans ADD CONSTRAINT DF_Scans_QueuedAtUtc DEFAULT SYSUTCDATETIME() FOR QueuedAtUtc;
    IF NOT EXISTS
    (
        SELECT 1 FROM sys.default_constraints dc
        INNER JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
        WHERE dc.parent_object_id = OBJECT_ID(N'dbo.Scans') AND c.name = N'ScheduledAtUtc'
    )
        ALTER TABLE dbo.Scans ADD CONSTRAINT DF_Scans_ScheduledAtUtc DEFAULT SYSUTCDATETIME() FOR ScheduledAtUtc;

    -- Replace the version 1 status/timestamp checks with the queue lifecycle rules.
    ALTER TABLE dbo.Scans ADD CONSTRAINT CK_Scans_Status
        CHECK (Status IN ('Queued','Running','Completed','CompletedWithErrors','Failed','Cancelled','Aborted'));
    ALTER TABLE dbo.Scans ADD CONSTRAINT CK_Scans_Lifecycle CHECK
    (
        (Status = 'Queued' AND StartedAtUtc IS NULL AND CompletedAtUtc IS NULL)
        OR (Status = 'Running' AND StartedAtUtc IS NOT NULL AND CompletedAtUtc IS NULL)
        OR (Status IN ('Completed','CompletedWithErrors','Failed','Cancelled') AND StartedAtUtc IS NOT NULL AND CompletedAtUtc IS NOT NULL)
        OR (Status = 'Aborted' AND StartedAtUtc IS NULL AND CompletedAtUtc IS NOT NULL)
    );

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Scans') AND name = N'IX_Scans_Queued_Scheduled')
        CREATE INDEX IX_Scans_Queued_Scheduled ON dbo.Scans (ScheduledAtUtc, QueuedAtUtc, ScanId) WHERE Status = 'Queued';
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Scans') AND name = N'IX_Scans_Profile_Status_Started')
        CREATE INDEX IX_Scans_Profile_Status_Started ON dbo.Scans (ProfileId, Status, StartedAtUtc DESC) INCLUDE (CompletedAtUtc, TotalFiles, TotalDirectories, TotalSizeBytes, ScanDurationSeconds, ErrorCount);

    INSERT dbo.SchemaVersion (SchemaVersionId, Notes) VALUES (2, N'Queue scans in dbo.Scans');
END;
GO
