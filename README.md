# Archive Auditor (IMS)

Archive Auditor is a Windows desktop application for building and maintaining SHA-256 inventories of file archives. It stores scan snapshots in SQL Server, compares snapshots over time, verifies a live archive against a saved snapshot, and produces CSV or HTML reports.

**Release status:** first release candidate (RC1). The application assembly version is currently `1.0.0`.

## Features

- Manage archive profiles stored in SQL Server. Profiles can be added, edited, deactivated, and reactivated; deactivation preserves history.
- Recursively scan files, calculate SHA-256 hashes, and store metadata and directory summaries in SQL Server.
- Queue scans in SQL Server using the existing `dbo.Scans` table, optionally choose a local scheduled start time, pause/resume the queue, and abort a queued job while retaining its `Aborted` history row.
- Apply a configurable delay before each scan starts. `ScanStartDelaySeconds` defaults to 5 in `settings.json` and currently accepts values from 0 to 3600; the UI for changing this setting is planned for a later release.
- Save file records in batches of 500 and scan errors in batches of 100. Commits are awaited by the scan pipeline; files are not scanned in parallel with SQL commits.
- Scanning and Full Verification skip selected Windows-managed folders at a drive root: `System Volume Information`, `$RECYCLE.BIN`, and `$WINDOWS.~BT`. Reparse points are skipped and reported as scan errors.
- Compare scans and filter results by added, deleted, modified, and unchanged records. A comparison involving a `CompletedWithErrors` scan requires confirmation because inaccessible items can look added or deleted.
- Run full verification against a fully completed baseline without creating a new scan.
- View large-file totals, duplicate summaries, directory statistics, and archive growth.
- Export comparison results to CSV or self-contained HTML.
- View SHA-256 values abbreviated as `aaaa...bbbb` in the comparison grid. Full hashes remain in SQL Server and are retained in report data.
- View scan errors in the Scan tab. Partial scans are still available for analysis and comparison, with their partial status indicated.
- Inspect SQL database row counts and database-file size, and run `DBCC CHECKDB` from the maintenance tools.
- Minimize the form using **Background**; cancel a scan with a confirmation prompt.

## Technology and database support

- C# and Windows Forms, targeting `net10.0-windows`.
- SQL Server access through `Microsoft.Data.SqlClient` 7.1.1 and SQL Server-specific T-SQL.
- SQL Server is the **only database engine supported by this release**. The data-access code is organized in services, but the SQL implementation is not yet database-provider agnostic. Other engines can be added in a future release by introducing and testing provider-specific connection, schema/migration, bulk-write, and query implementations behind suitable interfaces.

## Requirements

- Windows 10 or later.
- .NET 10 SDK to build, or the .NET 10 Windows Desktop Runtime to run a framework-dependent build.
- SQL Server 2019 or later, SQL Server Express, or LocalDB. The configured login needs the required schema access; creating a new database also requires permission to create databases.
- Visual Studio with the .NET 10 Windows Desktop workload, or the .NET SDK command-line tools.

## Build and run

From the repository root:

```powershell
dotnet restore IMS/IMS.csproj
dotnet build IMS/IMS.csproj
dotnet run --project IMS/IMS.csproj
```

Or open `IMS.slnx` in Visual Studio. The project does not explicitly set `PlatformTarget` or a runtime identifier; choose the target architecture when publishing if you need to guarantee a particular process architecture.

On first launch, the setup form collects the SQL Server connection and initial archive profile. It tests the connection and initializes a missing database/schema using the embedded copy of `Database/001_CreateArchiveAuditor.sql`. The same SQL script is kept in the repository for review or manual execution. New database files are sized to 64 MB by the script. Startup checks schema version 2; the script upgrades version 1 databases in place and preserves their scan rows.

The app directory must be writable because it stores `settings.json` and the application log there. Keep it outside all monitored archive roots.

## Configuration and credentials

For this release, `settings.json` is stored beside the executable to keep the application folder portable. It contains connection settings and, when SQL authentication is used, the SQL username and password as **plain text**. Configuration backups may also contain those credentials. Protect the application directory and backups; do not commit them to source control. The repository `.gitignore` excludes settings files and common credential-bearing backups.

DPAPI credential protection is a planned future improvement. Until then, do not treat the JSON file as encrypted or safe to share. Profile information is also mirrored in the JSON configuration for compatibility, while SQL Server is the operational source of profile records.

The connection factory enables `TrustServerCertificate` only for local endpoints and the `192.168.*` private address range to support the current local/LAN setup. For other servers, use a certificate trusted by the client rather than broadening certificate bypass behavior.

## Scan statuses and comparison cautions

- `Running`: scan has started and is still in progress.
- `Queued`: a scan is waiting for its scheduled time or its pre-start delay.
- `Completed`: scan finished without recorded errors.
- `CompletedWithErrors`: scan finished, but one or more paths or files could not be read. The stored snapshot is partial.
- `Failed`: scan could not be finalized successfully.
- `Cancelled`: the user requested cancellation.
- `Aborted`: a queued scan was removed after confirmation; the row remains in SQL as history.

The Dashboard queue is stored in `dbo.Scans` alongside completed scan history. The worker processes queued scans in scheduled/enqueued order. One SQL application lock prevents two application instances from processing the same queue concurrently. Pausing the queue lets an already running scan finish and holds later queued scans. `Scan` adds an immediate job to the queue; `Add to Queue` can use the optional local scheduled time.

The comparison selector and summary analyses accept both `Completed` and `CompletedWithErrors` scans. Partial analysis describes only the records that were captured. Before comparing a partial scan, the app warns that unreadable files can appear to have been added or deleted. Full Verification uses only fully completed scans as its baseline.

Scanning and Full Verification do not intentionally modify archive files. Scans hash every discovered regular file, even when metadata has not changed. Drive-root exclusions are intentionally limited to the named Windows-managed folders; hidden attributes alone do not exclude a file or directory. Other scan access failures are recorded in the scan error log.

## Data and integrity notes

The SQL schema is defined in `Database/001_CreateArchiveAuditor.sql`. It creates `SchemaVersion`, `ArchiveProfiles`, `Scans`, `Files`, `Directories`, `Comparisons`, and `ScanErrors`.

File and directory relative paths are stored in full, with a SHA-256 path key used for indexing. The approved uniqueness key for file path records is `(ScanId, RelativePathHash)`; the database keeps the complete relative path and content hash. Do not shorten or replace stored hashes to change how the UI formats them. Scan/profile IDs are SQL Server identity values and can contain gaps; they are keys, not row counts.

The script creates the initial data and log files at 64 MB when it creates a new database. Existing databases are not resized by rerunning the script. Back up the SQL Server database separately using SQL Server backup tools; copying `settings.json` alone does not back up scan history.

## Project layout

```text
IMS.slnx
Database/001_CreateArchiveAuditor.sql
IMS/
  Form1.cs                         Main form and event orchestration
  Form1.Designer.cs                WinForms controls and layout
  FirstRunSetupForm.cs             Initial SQL/profile setup
  Configuration/                   JSON settings and configuration models
  Data/                             SQL Server repositories and services
  Domain/                           Scan, file, profile, and comparison models
  Services/                         Scanning, verification, logging, exports
```

## Deferred development discussed

These are future directions, not capabilities promised by RC1:

1. **Protect credentials with DPAPI.** Keep a portable app-directory configuration approach where practical, but encrypt/decrypt SQL credentials using an appropriate Windows protection scope and define how backups and migration behave.
2. **Make SQL the single source of truth for profiles.** Remove or simplify the duplicate profile metadata stored in `settings.json` after adding a safe migration path for existing installations.
3. **Add SQL Server health telemetry.** Extend database statistics with server CPU, memory, disk activity, and available disk/database-file space. Use documented SQL Server interfaces, explain required permissions, and handle unsupported metrics clearly.
4. **Prepare for other database engines.** Keep this release SQL Server-only. If another engine is selected, introduce a provider boundary for connections, schema versioning/migrations, bulk writes, and engine-specific queries; do not spread generic SQL assumptions through the UI.
5. **Revisit scan/commit concurrency only with measurements.** Current scans await each 500-file SQL batch; the user observed short LAN commit pauses and chose the simpler sequential pipeline. Profile real workloads before introducing parallel producers/writers, and preserve cancellation, ordering, partial-scan status, and `(ScanId, RelativePathHash)` integrity.

## License

No public license has been selected yet. See `LICENSE`; choose and update the license before public redistribution.
