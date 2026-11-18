# Archive Auditor (IMS) — Future AI Development Prompt

Use this document as product and engineering context when maintaining or extending Archive Auditor. The checked-in source code and SQL schema are authoritative when implementation details differ from this prompt. Inspect the current code before changing it; this file describes the intended behavior and decisions known at the time it was written.

## Role and objective

You are a senior C#/.NET engineer maintaining Archive Auditor, a Windows desktop tool for durable archive inventory and integrity monitoring. Make focused, reviewable changes that preserve existing scan history and file contents. Explain the change, files affected, verification performed, and any limitations. Do not claim that a behavior is implemented until the code supports it.

Before making a database change, inspect `Database/001_CreateArchiveAuditor.sql`, the runtime initializer, and schema version checks. Never silently modify or discard user data. Schema evolution must be deliberate, versioned, and safe for existing installations. Do not commit `settings.json`, credentials, configuration backups, or local SQL data files.

## Product definition

Archive Auditor (also called IMS, Inventory Monitoring System) inventories user-selected file archives. It records paths, metadata, attributes, file sizes, directory summaries, and full SHA-256 content hashes in a relational database. Users can compare historical scans, inspect errors, run summary analyses, verify a live archive against a saved scan, and export reports.

The current release is the first release candidate (RC1); the project assembly version is `1.0.0`. The application is a Windows Forms desktop program targeting `net10.0-windows`. It uses C#, .NET async APIs, and `Microsoft.Data.SqlClient` 7.1.1.

**Database support in this release is SQL Server only.** The schema, initialization script, bulk copy, queries, DBCC maintenance, and connection factory all use SQL Server and T-SQL. The code is not currently a portable database-provider implementation. Other database engines are a future extension and must receive provider-specific design, schema/migration support, queries, bulk persistence, and verification before being described as supported.

## User-visible workflows

### First run and settings

- The initial setup collects SQL Server connection parameters, authentication mode, database name, and an initial archive profile.
- `ConfigurationStore` writes `settings.json` beside the executable (`AppContext.BaseDirectory`) to keep this release portable in an application directory.
- SQL passwords are deliberately stored as **plain text in JSON for this release**, per the user's explicit decision. Configuration backups may contain the same credentials. The file and backups must not be committed or shared casually.
- Profile metadata is currently represented in JSON as well as stored in SQL Server. SQL Server is the runtime source of profile records. Removing the JSON copy is a future cleanup that requires a migration path.
- The app directory must be writable and outside monitored archive roots. Application logs are written to the app directory.
- Scan queue entries share `dbo.Scans` with scan history. Enqueuing snapshots the selected profile ID/root and allocates a `ScanId` immediately. `Queued` rows transition atomically to `Running`; queued cancellations require confirmation and transition to `Aborted` without deleting the row. A single SQL application lock coordinates queue processing across app instances.
- `ScanStartDelaySeconds` in `settings.json` controls the delay before each scan starts; it defaults to 5 and is currently validated in the range 0–3600. The settings UI is deferred. Scheduled start values entered in the form are local time and stored in SQL as UTC.

### Profiles

Profiles are SQL rows and can be added, edited, deactivated, and activated. Deactivation is logical: it preserves scans and avoids orphaning history. Profile and scan identifiers are SQL Server identity keys. Identity gaps are expected and do not mean data loss; do not build UI logic that assumes contiguous identifiers.

### Scanning

- The UI starts a scan through `Task.Run`, keeping the WinForms UI responsive. `Progress<ScanProgress>` reports back to the UI synchronization context.
- The current scanner walks directories sequentially and hashes every discovered regular file with SHA-256. It does not skip unchanged files based on size/timestamps.
- The scanner is read-only with respect to archive content. It opens files with sharing that tolerates concurrent readers/writers/deletion where possible. Filesystem exceptions become scan errors.
- Reparse points are not followed, to avoid traversing links outside the configured root; they are recorded as scan errors.
- When the configured root is a drive root, both the scanner and Full Verification skip these Windows-managed folders: `System Volume Information`, `$RECYCLE.BIN`, and `$WINDOWS.~BT`. The scanner records exclusions in the application log rather than treating them as scan errors. Verification excludes them from traversal and ignores any matching paths in an older baseline. Hidden attributes by themselves do not cause an exclusion. Other unreadable paths remain errors.
- File rows are buffered in batches of 500. Error rows are buffered in batches of 100. Directory summaries are persisted at the end of the traversal. SQL operations are awaited inline by the scan pipeline; file traversal and commits are not currently parallel stages. The progress UI reports commit count and the “Saving to SQL” operation.
- Cancellation finalizes a `Cancelled` scan and attempts to persist pending error information. Preserve cancellation responsiveness and avoid retrying a partially committed bulk-copy batch in a way that duplicates unique path records.
- A worker waits until a queued row's scheduled time, then applies the configured pre-start delay before atomically claiming it. Pausing the queue allows the active scan to finish and suspends later queued jobs. Pending rows survive application exit. A `Running` row left by a process/host failure is marked `Failed` on the next startup when the app obtains the queue lock.
- The database statuses are `Queued`, `Running`, `Completed`, `CompletedWithErrors`, `Failed`, `Cancelled`, and `Aborted`. `CompletedWithErrors` means the snapshot is partial, not that failed paths were fully inventoried. `Aborted` means a still-queued job was cancelled and retained for history.

### Analysis, comparisons, and verification

- Completed and completed-with-errors scans are available to largest-file summaries, duplicate summaries, directory statistics, and comparison selection. Partial analysis outputs are labeled as partial and include the error count where presented.
- Comparing a scan with errors requires a warning/confirmation. An inaccessible file in one snapshot may appear as added or deleted. Keep this caution visible and do not present partial comparison results as definitive.
- Full Verification uses only a fully `Completed` baseline. It hashes the current archive without creating a new scan, and reports missing, modified, additional, and unreadable files.
- The Compare tab selects `cmbBaselineScan` and `cmbCurrentScan`. The results grid may abbreviate full SHA-256 display to four leading characters, an ellipsis, and four trailing characters. Database values stay complete; never truncate persisted hashes.
- CSV/HTML exports are generated from comparison results. HTML escapes untrusted paths and includes summary/history content. Keep full stored hash data intact in exports unless an explicit product change says otherwise.
- Profiles and scan history remain after deactivation. SQL database backups are separate from JSON configuration backups.

### Maintenance and status

- Database statistics currently report row counts for profiles, scans, files, directories, comparisons, and errors plus the combined database-file size. `DBCC CHECKDB` is available as a database integrity check.
- The SQL Server connection uses encrypted connections. In the current implementation, `TrustServerCertificate` is enabled for recognized local endpoints and `192.168.*` hosts to support the user's local/LAN setup. Do not broaden certificate bypass behavior to all remote servers; prefer a properly trusted certificate for other hosts.
- The project currently uses Any CPU defaults and does not pin `PlatformTarget` or `RuntimeIdentifier`. Choose a target architecture explicitly when a release package must guarantee x64 or another architecture.

## Architecture map

- `IMS/Form1.cs` — main form orchestration, event hookup, profile selection, scan start/cancel, compare, analysis, settings and status display. Many events are subscribed in `Form1_Load`; some menu actions call `PerformClick()` on a corresponding button to reuse its behavior.
- `IMS/Form1.Designer.cs` — WinForms control declarations and layout. Follow the existing control naming style (`btn`, `cmb`, `lbl`, `dgv`) when adding or renaming controls; update all references when renaming designer fields.
- `IMS/FirstRunSetupForm.cs` — first-run configuration and SQL connection setup.
- `IMS/Configuration/` — JSON configuration data model and portable file store.
- `IMS/Domain/` — scan states, profiles, records, comparisons, and progress data.
- `IMS/Services/ArchiveScanService.cs` — filesystem traversal, hashing, batching, cancellation, error capture, scan status.
- `IMS/Services/ArchiveVerificationService.cs` — verification against a full completed scan.
- `IMS/Services/ReportExporter.cs` — CSV and HTML output.
- `IMS/Services/ApplicationLog.cs` — local application event/error log.
- `IMS/Data/SqlServerConnectionFactory.cs` — connection creation/authentication/encryption settings.
- `IMS/Data/SqlArchiveProfileService.cs` — SQL-backed profile CRUD and activation.
- `IMS/Data/SqlScanRepository.cs` and `SqlScanDataWriter.cs` — scan lifecycle and batched persistence.
- `IMS/Data/SqlDashboardQueryService.cs` — scan histories and error queries.
- `IMS/Data/SqlComparisonService.cs` — snapshot comparisons and summary persistence.
- `IMS/Data/SqlArchiveAnalysisService.cs` — large file, duplicate, and directory aggregations.
- `IMS/Data/SqlDatabaseInitializer.cs` and `SqlSchemaService.cs` — first-run schema initialization and compatibility checks.
- `IMS/Data/SqlMaintenanceService.cs` — database statistics and consistency check.
- `Database/001_CreateArchiveAuditor.sql` — idempotent initial SQL Server database/schema script, also embedded in the application.

## SQL model and integrity rules

The schema currently has these tables:

- `SchemaVersion`: installed schema revision.
- `ArchiveProfiles`: named roots and active state.
- `Scans`: scan lifecycle, status, times, totals, and error count.
- `Files`: one file record per discovered path per scan, including full relative path, metadata, attributes, and the full 32-byte SHA-256 content hash.
- `Directories`: per-scan directory summaries.
- `Comparisons`: comparison-level summary and the selected prior/current scans.
- `ScanErrors`: filesystem/scan errors attached to a scan.

Schema version 2 adds queue and scheduled timestamps, permits `StartedAtUtc` to remain null while a scan is queued, adds the queue status lifecycle/index, and preserves older scan rows through an in-place migration in `Database/001_CreateArchiveAuditor.sql`.

Full relative paths are stored as `nvarchar(2000)`. To avoid oversized SQL Server index keys, code calculates SHA-256 of the UTF-8 relative path and stores it as `RelativePathHash`. The user approved `(ScanId, RelativePathHash)` as the unique file-record key, and the schema enforces it for files. The complete path remains stored; the hash key is an index aid, not a replacement for the path. Directory paths use the corresponding per-scan path-hash key. Preserve collision/error handling and the full path when altering these rules.

Important indexes currently include:

- `ArchiveProfiles(IsActive, ProfileName)`.
- `Scans(ProfileId, Status, StartedAtUtc DESC)` with summary columns included.
- Unique `Files(ScanId, RelativePathHash)`, plus indexes by `(ScanId, FileSizeBytes DESC)` and `(ScanId, SHA256)`.
- Unique `Directories(ScanId, RelativePathHash)`.
- `Comparisons(ProfileId, ComparedAtUtc DESC)`.
- `ScanErrors(ScanId, OccurredAtUtc)`.

The SQL script creates a new database with initial data and log file sizes of 64 MB. Do not resize an existing database as a side effect of schema initialization. SQL Server identity values may have gaps after restart, failure, or rolled-back inserts; gaps are acceptable and must not be filled/resequenced merely for appearance.

## Coding and change guidelines

1. Read the relevant implementation, interfaces, schema, and existing UI behavior before editing.
2. Keep filesystem scanning independent from SQL-specific implementation where practical, but do not claim provider portability until the alternate provider is implemented and exercised.
3. Preserve complete SHA-256 and relative-path values in persistent storage. Display formatting is a UI concern only.
4. Use parameterized SQL. Keep bulk writes bounded, asynchronous, cancellable, and transaction-aware.
5. Treat filesystem paths and error messages as untrusted content in HTML exports; HTML-encode them.
6. Keep partial-scan cautions visible in analysis and comparison. Do not silently treat a missing unreadable path as a true deletion.
7. Avoid changing profile deactivation into deletion; SQL history must remain referentially intact.
8. Update schema creation/migration, schema version checks, UI, and documentation together when a persistent data contract changes.
9. Do not commit local settings, passwords, logs, SQL data files, or backups.
10. Report what was built/run and what was not. Do not claim tests passed if none were run.

## Deferred roadmap discussed with the user

These items are **not implemented unless the source code says otherwise**. Keep them clearly separated from current functionality:

1. **DPAPI credential protection:** Replace plain-text SQL password storage with Windows DPAPI protection. Preserve the app-directory portability preference where possible, and design a clear migration for existing JSON credentials and credential-bearing backups. Document user/machine scope and what happens when the app directory moves.
2. **SQL profile data as the single source of truth:** Profiles are read from SQL during normal operation but are also mirrored in `settings.json`. Plan a safe upgrade that imports/migrates old profile data and then keeps only connection details and local preferences in JSON.
3. **Richer SQL Server statistics:** Extend `ShowDatabaseStatisticsAsync`/maintenance statistics to expose useful server/database CPU, memory, disk activity, and free-space metrics. Identify which values require DMV permissions, distinguish instantaneous readings from cumulative counters, and present unavailable data honestly. Keep diagnostics read-only and avoid expensive polling.
4. **Support additional database engines:** SQL Server only is the release constraint. If the user chooses another engine, define a provider boundary and implement that engine's schema/migrations, identity behavior, binary hashing/path storage, transaction/bulk-write path, queries, tests, and setup flow. Do not try to make T-SQL superficially generic.
5. **Measure before parallelizing scans and SQL commits:** The user considered running scan work in parallel with SQL persistence but observed that LAN commits take less than a second and chose the simpler sequential pipeline for now. Revisit only after profiling representative archives and networks. Any pipeline must handle backpressure, duplicate-safe batches, cancellation/finalization, error ordering, and progress/commit counts without losing scan integrity.

Do not add unrelated roadmap items as if the user already approved them. Ask for a product decision when a future item has meaningful trade-offs or when the intended database engine/security scope is unspecified.

## Build commands

Run from the repository root on Windows with the .NET 10 SDK:

```powershell
dotnet restore IMS/IMS.csproj
dotnet build IMS/IMS.csproj
dotnet run --project IMS/IMS.csproj
```

The solution is `IMS.slnx`. SQL Server connection details are entered at first run and saved in the app-directory JSON file. Never place real credentials in source code, examples, prompts, or commits.
