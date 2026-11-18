# User Guide

## First run

1. Start Archive Auditor from a writable application directory outside the archive you plan to monitor.
2. Enter the SQL Server and database name. The database defaults to `ArchiveAuditor`.
3. Choose Windows or SQL authentication. SQL passwords are stored in plain text in `settings.json` beside the executable in this release.
4. Test the SQL Server connection, then enter the initial profile name and select an existing archive directory.
5. Select **Initialize and Continue**. The application creates any missing database/schema objects and checks the schema version before opening the Dashboard.

SQL Server must permit database creation when the chosen database does not yet exist. The script is at `Database/001_CreateArchiveAuditor.sql` for inspection and manual execution.

## Dashboard and scans

Choose an active profile and use **Scan** on the Dashboard to enqueue an immediate scan. Use **Add to Queue** to enqueue it for later, optionally selecting a local scheduled start time with the date/time picker. The queue appears in the queue grid and is stored in the same SQL `dbo.Scans` table as scan history. Before each queued scan begins, the app waits for the configurable `ScanStartDelaySeconds` value (5 seconds by default). You can edit that value in the app-directory `settings.json` for now; accepted values are 0 through 3600 seconds.

Use **Pause Queue** to let the current scan finish and hold subsequent jobs; **Resume Queue** restarts processing. Select a queued row and use **Cancel Queued Scan** to remove it from the pending list. The app asks for confirmation, then marks the database row `Aborted` so the history is retained. This action applies only while the job is queued; use **Cancel Scan** to stop a scan that has already started. During a scan, the Scan tab shows progress. Cancelled and failed scans are not comparison choices. A `CompletedWithErrors` scan is partial but may be used for analysis or comparison; the app warns before comparing one because unreadable files may appear added or deleted. Full Verification only accepts a fully `Completed` scan.

Scans hash the full content of every discovered regular file, even if file size and timestamps match an earlier scan. The selected archive is read-only. At a drive root, both scanning and Full Verification exclude `System Volume Information`, `$RECYCLE.BIN`, and `$WINDOWS.~BT`; hidden attributes alone do not exclude files or folders. Symbolic links and other reparse points are skipped and recorded as scan errors.

## Compare and export

Use **Compare** from the Dashboard or Compare tab. Select two completed scans for the active profile, then select a change filter. Scans with errors are also selectable, but a confirmation warns about possible false additions or deletions. Renames appear as a deletion and an addition. Export changed records to CSV or HTML; unchanged files are summarized but not listed.

## Advanced analysis

The Advanced tab supports full verification against a completed scan, duplicate summaries, large-file count and total size, empty-directory counts, directory totals, and a historical size chart. These summaries can use the latest scan with errors and mark their results as partial. Verification reads and hashes current files without creating a scan and requires a fully completed baseline.

## Profiles and settings

Settings lets you add, edit, or deactivate profiles. Deactivation preserves historical scans. Database test, consistency check, statistics, and configuration backup/restore are also available. A restored configuration is checked against its database schema before replacing the active settings.

All displayed timestamps use UTC/GMT. Configuration backups contain credentials in plain text for this release.
