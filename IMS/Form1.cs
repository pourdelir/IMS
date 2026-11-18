using IMS.Configuration;
using IMS.Data;
using IMS.Domain;
using IMS.Services;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using AppConfig = IMS.Configuration.ApplicationConfiguration;

namespace IMS
{
    public partial class Form1 : Form
    {
        private readonly ConfigurationStore _configurationStore = new();
        private readonly ApplicationLog _log = new();
        private AppConfig? _configuration;
        private SqlArchiveProfileService? _profiles;
        private SqlScanRepository? _scans;
        private SqlScanDataWriter? _scanWriter;
        private SqlDashboardQueryService? _queries;
        private IArchiveScanService? _scanService;
        private CancellationTokenSource? _scanCancellation;
        private CancellationTokenSource? _queueWorkerCancellation;
        private CancellationTokenSource? _queueCountdownCancellation;
        private Task? _queueWorkerTask;
        private SqlConnection? _queueWorkerLease;
        private readonly SemaphoreSlim _queueWakeSignal = new(0, 1);
        private bool _queuePaused;
        private bool _allowFormClose;
        private bool _shutdownInProgress;
        private long? _queueCountdownScanId;
        private bool _scanActive;
        private IReadOnlyList<ArchiveProfile> _activeProfiles = [];
        private ComparisonResult? _lastComparison;
        private readonly ReportExporter _reportExporter = new();
        private readonly ArchiveGrowthChartControl _growthChart = new() { Location = new Point(10, 25), Size = new Size(820, 115), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        private readonly Label _annualGrowthLabel = new() { Location = new Point(10, 140), AutoSize = true, Text = "Historical archive growth" };
        private readonly Label _comparisonSummaryLabel = new() { Location = new Point(335, 82), Size = new Size(520, 44), AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft };
        private Stopwatch? _scanTimer;

        public Form1() => InitializeComponent();

        private async void Form1_Load(object sender, EventArgs e)
        {
            lblOperationStatus.Text = " Initialising...";
            Text = "Archive Auditor — Inventory Monitoring System";
            SetControlCaptions();
            ConfigureGrids();
            cmbChangeFilter.Items.AddRange(["All", "Added", "Deleted", "Modified", "Unchanged"]);
            cmbChangeFilter.SelectedIndex = 0;
            cmbChangeFilter.SelectedIndexChanged += cmbChangeFilter_SelectedIndexChanged;
            FormClosing += Form1_FormClosing;
            btnExit.Click += (_, _) => Close();
            btnCancel.Click += (_, _) => ConfirmCancelScan();
            btnBackgroun.Click += (_, _) => WindowState = FormWindowState.Minimized; 
            btnScan.Click += async (_, _) => await AddSelectedProfileToQueueAsync(runImmediately: true);
            btnAddToQueue.Click += async (_, _) => await AddSelectedProfileToQueueAsync(runImmediately: false);
            btnCancelQueuedScan.Click += async (_, _) => await ConfirmAbortQueuedScanAsync();
            btnPauseQueue.Click += (_, _) => ToggleQueuePause();
            dgvScanQueue.SelectionChanged += (_, _) => btnCancelQueuedScan.Enabled = dgvScanQueue.CurrentRow is not null;
            BtnGoCompare.Click += async (_, _) => { tabControl1.SelectedTab = tabPage3; await LoadComparisonChoicesAsync(); };
            btnCompare.Click += async (_, _) => await CompareAsync();
            btnTestConnection.Click += async (_, _) => await TestConfiguredConnectionAsync();
            btnBrowseExportDirectory.Click += (_, _) => BrowseExportDirectory();
            chkUseGMT.CheckedChanged += async (_, _) =>
            {
                if (_configuration is null) return;
                _configuration.DisplayTimestampsInUtc = chkUseGMT.Checked;
                await _configurationStore.SaveAsync(_configuration);
            };
            btnAddProfile.Click += async (_, _) => await AddProfileAsync();
            btnRemoveProfile.Click += async (_, _) => await RemoveProfileAsync();
            btnEditProfile.Click += async (_, _) => await EditProfileAsync();
            btnActivateProfile.Click += async (_, _) => await ActivateProfileAsync();
            cmbArchiveProfile.SelectedIndexChanged += async (_, _) => await RefreshProfileAsync();
            exitToolStripMenuItem.Click += (_, _) => Close();
            newScanToolStripMenuItem.Click += (_, _) => btnScan.PerformClick();
            btnExportCSV.Click += async (_, _) => await ExportAsync(html: false);
            btnExportHTML.Click += async (_, _) => await ExportAsync(html: true);
            btnLargestFiles.Click += async (_, _) => await ShowLargestFilesAsync();
            btnFindDuplicates.Click += async (_, _) => await ShowDuplicatesAsync();
            btnEmptyDirectories.Click += async (_, _) => await ShowDirectoryAnalysisAsync();

            btnFullVerification.Click += async (_, _) => await VerifyArchiveAsync();
            btnDatabaseStatistics.Click += async (_, _) => await ShowDatabaseStatisticsAsync();
            btnCheckDatabase.Click += async (_, _) => await CheckDatabaseAsync();
            btnBackupConfiguration.Click += async (_, _) => await BackupConfigurationAsync();
            btnRestoreConfiguration.Click += async (_, _) => await RestoreConfigurationAsync();

            /* MENU ITEMS */
            addProfileToolStripMenuItem.Click += async (_, _) => await AddProfileAsync();
            editProfileToolStripMenuItem.Click += async (_, _) => await EditProfileAsync();
            activateProfileToolStripMenuItem.Click += async (_, _) => await ActivateProfileAsync();
            compareScansStripMenuItem.Click += async (_, _) => tabControl1.SelectedTab = tabPage3;
            toolStripMenuAdvanced.Click += (_, _) => tabControl1.SelectedTab = tabPage4;
            databaseStatusToolStripMenuItem.Click += async (_, _) => await ShowDatabaseStatisticsAsync();
            aboutToolStripMenuItem.Click += (_, _) => tabControl1.SelectedTab = tabPage6;
            exportToolStripMenuItem.Click += async (_, _) => tabControl1.SelectedTab = tabPage3;
            versionsToolStripMenuItem.Click += (_, _) => tabControl1.SelectedTab = tabPage6;

            try
            {
                lblOperationStatus.Text = " Initialising Datalink...";
                SetDatabaseStatus("Connecting");
                SetDatabaseStatus("Loading configuration");
                _configuration = await _configurationStore.LoadAsync();
                var createdConfiguration = _configuration is null;
                if (_configuration is null)
                {
                    using var setup = new FirstRunSetupForm();
                    if (setup.ShowDialog(this) != DialogResult.OK || setup.Configuration is null)
                    {
                        Close();
                        return;
                    }
                    _configuration = setup.Configuration;
                }

                InitializeServices();
                SetDatabaseStatus("Checking database and schema");
                var initializer = new SqlDatabaseInitializer(_configuration, new SqlServerConnectionFactory(_configuration));
                await initializer.InitializeAsync();
                var schema = new SqlSchemaService(new SqlServerConnectionFactory(_configuration));
                await schema.EnsureCompatibleAsync();

                _activeProfiles = await _profiles!.GetProfilesAsync(activeOnly: true);
                if (createdConfiguration && _configuration.Profiles.Count > 0)
                {
                    var profile = _configuration.Profiles[0];
                    await _profiles.AddProfileAsync(profile.ProfileName, profile.RootPath);
                    _activeProfiles = await _profiles.GetProfilesAsync(activeOnly: true);
                }
                await _configurationStore.SaveAsync(_configuration);
                PopulateProfiles();
                await RefreshQueueAsync();
                await StartQueueWorkerAsync();
                SetDatabaseStatus("Connected");
                lblOperationStatus.Text = "☑️ Ready.";
            }
            catch (Exception exception)
            {
                SetDatabaseStatus("Connection or database error");
                lblOperationStatus.Text = "Error";
                await _log.WriteAsync("Error", "Startup Failed", "Application startup or database initialization failed.", exception);
                MessageBox.Show(this, $"Archive Auditor could not start:\n\n{exception.Message}", "Startup error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                /* Close(); */
            }
        }

        private void InitializeServices()
        {
            var connections = new SqlServerConnectionFactory(_configuration!);
            _profiles = new SqlArchiveProfileService(connections);
            _scans = new SqlScanRepository(connections);
            _scanWriter = new SqlScanDataWriter(connections);
            _queries = new SqlDashboardQueryService(connections);
            _scanService = new ArchiveScanService(_scans, _scanWriter, _log);
        }

        private void ConfigureGrids()
        {
            numLargestFiles.DecimalPlaces = 1;
            numLargestFiles.Increment = 0.1m;
            numLargestFiles.Minimum = 0.1m;
            numLargestFiles.Maximum = 1000000m;
            numLargestFiles.Value = 1m;
            dgvScanHistory.Columns.Clear();
            dgvScanHistory.AutoGenerateColumns = false;
            dgvScanHistory.Columns.Add(new DataGridViewTextBoxColumn { Name = "ScanId", HeaderText = "Scan ID", DataPropertyName = "ScanId" });
            dgvScanHistory.Columns.Add(new DataGridViewTextBoxColumn { Name = "Started", HeaderText = "Started (UTC)", DataPropertyName = "Started" });
            dgvScanHistory.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", DataPropertyName = "Status" });
            dgvScanHistory.Columns.Add(new DataGridViewTextBoxColumn { Name = "Files", HeaderText = "Files", DataPropertyName = "Files" });
            dgvScanHistory.Columns.Add(new DataGridViewTextBoxColumn { Name = "Size", HeaderText = "Size", DataPropertyName = "Size" });
            dgvScanHistory.ReadOnly = true;
            dgvScanHistory.AllowUserToAddRows = false;
            dgvScanQueue.Columns.Clear();
            foreach (var (name, header) in new[]
                     {
                         ("ScanId", "Scan ID"), ("Profile", "Profile"), ("RootPath", "Root Path"),
                         ("ScheduledAt", "Scheduled Start (Local)")
                     })
                dgvScanQueue.Columns.Add(name, header);
            dgvScanQueue.ReadOnly = true;
            dgvScanQueue.AllowUserToAddRows = false;
            dgvScanQueue.AllowUserToDeleteRows = false;
            dgvScanQueue.MultiSelect = false;
            dgvScanQueue.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvScanQueue.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dtpScheduledStart.ShowCheckBox = true;
            dtpScheduledStart.Checked = false;
            dtpScheduledStart.Format = DateTimePickerFormat.Custom;
            dtpScheduledStart.CustomFormat = "dd-MMM-yyyy HH:mm";
            btnCancelQueuedScan.Enabled = false;
            btnPauseQueue.Enabled = false;
            dataGridView2_errors.Columns.Clear();
            foreach (var (name, header) in new[] { ("Path", "Relative Path"), ("Type", "Error Type"), ("Message", "Message"), ("At", "Occurred (UTC)") })
                dataGridView2_errors.Columns.Add(name, header);
            dataGridView2_errors.ReadOnly = true;
            dataGridView2_errors.AllowUserToAddRows = false;
            dataGridView2_errors.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView2_errors.Visible = false;
            lblErrReport.Visible = false;
            dataGridView2.Columns.Clear();
            foreach (var (name, header) in new[] { ("Id", "Profile ID"), ("Name", "Profile Name"), ("Root", "Root Directory"), ("Active", "Active") })
                dataGridView2.Columns.Add(name, header);
            dataGridView2.ReadOnly = true;
            dataGridView2.AllowUserToAddRows = false;
            dataGridView1.Columns.Clear();
            foreach (var (name, header) in new[] { ("RecordType", "Record Type"), ("RelativePath", "Relative Path"),
                         ("ChangeType", "Change"), ("PreviousSize", "Previous Size"), ("CurrentSize", "Current Size"),
                         ("PreviousHash", "Previous SHA-256"), ("CurrentHash", "Current SHA-256") })
                dataGridView1.Columns.Add(name, header);
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            toolStripProgressBar.Style = ProgressBarStyle.Marquee;
            toolStripProgressBar.Visible = false;
            groupBox13.Controls.Add(_growthChart);
            groupBox13.Controls.Add(_annualGrowthLabel);
            tabPage3.Controls.Add(_comparisonSummaryLabel);
            btnCancel.Enabled = false;
            txtPassword.UseSystemPasswordChar = true;
            lblGMTClock.Text = DateTime.UtcNow.ToString("dd-MMM-yyyy HH:mm:ss 'GMT'");
            var clock = new System.Windows.Forms.Timer { Interval = 1000 };
            clock.Tick += (_, _) => lblGMTClock.Text = DateTime.UtcNow.ToString(" dd-MMM-yyyy HH:mm:ss 'GMT' ");
            clock.Start();
        }

        private void SetControlCaptions()
        {
            btnScan.Text = "Scan";
            btnCompare.Text = "Compare";
            btnExportCSV.Text = "Export CSV";
            btnExportHTML.Text = "Export HTML";
            btnFullVerification.Text = "Full Verification";
            btnFindDuplicates.Text = "Find Duplicates";
            btnLargestFiles.Text = "Count Large Files";
            btnEmptyDirectories.Text = "Find Empty Directories";

            btnTestConnection.Text = "Test Connection";
            btnAddProfile.Text = "Add Profile";
            btnEditProfile.Text = "Edit Profile";
            btnRemoveProfile.Text = "Deactivate Profile";
            btnActivateProfile.Text = "Activate Profile";
            btnDatabaseStatistics.Text = "Database Statistics";
            btnCheckDatabase.Text = "Check Database";
            btnBackupConfiguration.Text = "Backup Configuration";
            btnRestoreConfiguration.Text = "Restore Configuration";
            lblServerAddress.Text = "SQL Server";
            lblDatabaseName.Text = "Database";
            lblUsername.Text = "SQL Username";
            lblPassword.Text = "SQL Password";
            lblExportDirectory.Text = "Export Directory";
        }

        private void PopulateProfiles()
        {
            if (_activeProfiles.Any(profile => IsApplicationInsideArchive(profile.RootPath)))
                throw new InvalidOperationException("The application directory is inside a monitored archive. Move the application folder or change the archive root so settings and logs stay outside the archive.");
            cmbArchiveProfile.DataSource = null;
            cmbArchiveProfile.DataSource = _activeProfiles.ToList();
            cmbArchiveProfile.DisplayMember = nameof(ArchiveProfile.ProfileName);
            cmbArchiveProfile.ValueMember = nameof(ArchiveProfile.ProfileId);
            dataGridView2.Rows.Clear();
            foreach (var profile in _activeProfiles)
                dataGridView2.Rows.Add(profile.ProfileId, profile.ProfileName, profile.RootPath, profile.IsActive);
            btnScan.Enabled = _activeProfiles.Count > 0;
            if (_configuration is not null)
            {
                txtServerAddress.Text = _configuration.Database.Server;
                cmbDatabaseName.Text = _configuration.Database.DatabaseName;
                txtUsername.Text = _configuration.Database.UserName;
                txtPassword.Text = _configuration.Database.Password;
                txtExportDirectory.Text = _configuration.ExportDirectory;
                chkUseGMT.Checked = _configuration.DisplayTimestampsInUtc;
            }
        }

        private async Task SaveProfileConfigurationAsync()
        {
            if (_configuration is null) return;
            _configuration.Profiles = _activeProfiles.Select(p => new ArchiveProfileConfiguration
            { ProfileName = p.ProfileName, RootPath = p.RootPath, IsActive = p.IsActive }).ToList();
            await _configurationStore.SaveAsync(_configuration);
        }

        private async Task RefreshProfileAsync()
        {
            if (cmbArchiveProfile.SelectedItem is not ArchiveProfile profile || _queries is null) return;
            txtRootPath.Text = profile.RootPath;
            lblArchiveProfile.Text = $"Profile: {profile.ProfileName}";
            var scans = await _queries.GetScansAsync(profile.ProfileId);
            var completedHistory = scans.Where(s => s.Status == ScanStatus.Completed).ToList();
            _growthChart.SetScans(completedHistory);
            if (completedHistory.Count > 1)
            {
                var first = completedHistory.OrderBy(s => s.StartedAtUtc).First();
                var last = completedHistory.OrderBy(s => s.StartedAtUtc).Last();
                var years = (last.StartedAtUtc - first.StartedAtUtc).TotalDays / 365.25;
                var growth = years > 0 && first.TotalSizeBytes > 0 ? (Math.Pow((double)last.TotalSizeBytes / first.TotalSizeBytes, 1 / years) - 1) * 100 : 0;
                _annualGrowthLabel.Text = $"Annualized size growth: {growth:0.0}% · {completedHistory.Count} successful snapshots";
            }
            else _annualGrowthLabel.Text = "Need at least two successful snapshots to calculate growth.";
            dgvScanHistory.Rows.Clear();
            foreach (var scan in scans)
                dgvScanHistory.Rows.Add(scan.ScanId, scan.StartedAtUtc.ToString("dd-MMM-yyyy HH:mm:ss"), scan.Status,
                    scan.TotalFiles, FormatBytes(scan.TotalSizeBytes));
            var latest = scans.FirstOrDefault(scan => scan.Status == ScanStatus.Completed);
            lblTotalFiles.Text = latest is null ? "Files: —" : $"Files: {latest.TotalFiles:N0}";
            lblTotalDirectories.Text = latest is null ? "Directories: —" : $"Directories: {latest.TotalDirectories:N0}";
            lblTotalSize.Text = latest is null ? "Size: —" : $"Size: {FormatBytes(latest.TotalSizeBytes)}";
            lblLastScan.Text = latest is null ? "Last scan: —" : $"Last scan: {latest.StartedAtUtc:dd-MM-yyyy HH:mm} UTC";
            lblLastScanStatus.Text = scans.FirstOrDefault()?.Status.ToString() ?? "No scans yet";
            lblLastDuration.Text = latest?.ScanDurationSeconds is long seconds ? $"Duration: {TimeSpan.FromSeconds(seconds)}" : "Duration: —";
            lblLastComparison.Text = _lastComparison?.ProfileId == profile.ProfileId
                ? $"Last comparison: {_lastComparison.ComparedAtUtc:dd-MMM-yyyy HH:mm} UTC"
                : "Last comparison: See Compare tab";
            await LoadComparisonChoicesAsync();
        }

        private async Task LoadComparisonChoicesAsync()
        {
            if (cmbArchiveProfile.SelectedItem is not ArchiveProfile profile || _queries is null) return;
            var scans = await _queries.GetScansAsync(profile.ProfileId, completedOnly: true);
            var descending = scans.OrderByDescending(s => s.StartedAtUtc).ToList();
            cmbBaselineScan.DataSource = descending.ToList();
            cmbBaselineScan.DisplayMember = nameof(ScanSnapshot.ScanId);
            cmbBaselineScan.ValueMember = nameof(ScanSnapshot.ScanId);
            cmbCurrentScan.DataSource = descending.ToList();
            cmbCurrentScan.DisplayMember = nameof(ScanSnapshot.ScanId);
            cmbCurrentScan.ValueMember = nameof(ScanSnapshot.ScanId);
            if (descending.Count > 1) cmbBaselineScan.SelectedIndex = 1;
            if (descending.Count > 0) cmbCurrentScan.SelectedIndex = 0;
        }

        private async Task AddSelectedProfileToQueueAsync(bool runImmediately)
        {
            if (cmbArchiveProfile.SelectedItem is not ArchiveProfile profile || _scans is null) return;

            var scheduledAtUtc = DateTime.UtcNow;
            if (!runImmediately && dtpScheduledStart.Checked)
            {
                scheduledAtUtc = DateTime.SpecifyKind(dtpScheduledStart.Value, DateTimeKind.Local).ToUniversalTime();
                if (scheduledAtUtc <= DateTime.UtcNow)
                {
                    MessageBox.Show(this, "Choose a future start time, or clear the checkbox to queue the scan immediately.",
                        "Invalid start time", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            try
            {
                // The queue row snapshots the selected profile's root and receives its ScanId now.
                var scanId = await _scans.EnqueueScanAsync(profile.ProfileId, profile.RootPath, scheduledAtUtc);
                await RefreshQueueAsync();
                SignalQueueWorker();
                lblOperationStatus.Text = $"Scan {scanId} queued for {profile.ProfileName}";
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Queue scan", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task RefreshQueueAsync()
        {
            if (_scans is null || IsDisposed) return;
            var queuedScans = await _scans.GetQueuedScansAsync();
            dgvScanQueue.Rows.Clear();
            foreach (var scan in queuedScans)
            {
                var scheduledLocal = DateTime.SpecifyKind(scan.ScheduledAtUtc, DateTimeKind.Utc).ToLocalTime();
                dgvScanQueue.Rows.Add(scan.ScanId, scan.ProfileName, scan.RootPath, scheduledLocal.ToString("dd-MMM-yyyy HH:mm:ss"));
            }
            if (dgvScanQueue.Rows.Count > 0)
            {
                dgvScanQueue.Rows[0].Selected = true;
                dgvScanQueue.CurrentCell = dgvScanQueue.Rows[0].Cells[0];
            }
            btnCancelQueuedScan.Enabled = dgvScanQueue.Rows.Count > 0;
        }

        private async Task ConfirmAbortQueuedScanAsync()
        {
            if (dgvScanQueue.CurrentRow is null || _scans is null) return;
            var scanId = Convert.ToInt64(dgvScanQueue.CurrentRow.Cells["ScanId"].Value);
            var profileName = Convert.ToString(dgvScanQueue.CurrentRow.Cells["Profile"].Value) ?? "selected profile";
            var result = MessageBox.Show(this,
                $"Remove scan {scanId} for '{profileName}' from the queue?\n\nThe job will be marked Aborted in SQL and retained in the scan history.",
                "Abort queued scan?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes) return;

            var aborted = await _scans.AbortQueuedScanAsync(scanId);
            if (!aborted)
            {
                MessageBox.Show(this, "That job is no longer queued. If it has started, use Cancel Scan to stop it.",
                    "Queue changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_queueCountdownScanId == scanId)
                _queueCountdownCancellation?.Cancel();
            await RefreshQueueAsync();
            SignalQueueWorker();
            await RefreshProfileAsync();
        }

        private void ToggleQueuePause()
        {
            _queuePaused = !_queuePaused;
            btnPauseQueue.Text = _queuePaused ? "Resume Queue" : "Pause Queue";
            lblOperationStatus.Text = _queuePaused ? "Queue paused; current scan will finish." : "Queue resumed.";
            if (_queuePaused)
                _queueCountdownCancellation?.Cancel();
            SignalQueueWorker();
        }

        private async Task StartQueueWorkerAsync()
        {
            if (_scans is null) return;
            _queueWorkerLease = await _scans.TryAcquireQueueWorkerLockAsync();
            if (_queueWorkerLease is null)
            {
                btnPauseQueue.Enabled = false;
                lblOperationStatus.Text = "Queue is being processed by another application instance.";
                return;
            }

            // The exclusive SQL application lock makes it safe to finalize abandoned Running rows.
            await _scans.RecoverInterruptedScansAsync();
            _queueWorkerCancellation = new CancellationTokenSource();
            btnPauseQueue.Enabled = true;
            _queueWorkerTask = ProcessScanQueueAsync(_queueWorkerCancellation.Token);
        }

        private async Task ProcessScanQueueAsync(CancellationToken cancellationToken)
        {
            // One worker starts due jobs in order; the SQL status update claims each job once.
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_queuePaused)
                    {
                        await _queueWakeSignal.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken);
                        continue;
                    }

                    var queuedScan = await _scans!.GetNextQueuedScanAsync(cancellationToken);
                    if (queuedScan is null)
                    {
                        await _queueWakeSignal.WaitAsync(TimeSpan.FromSeconds(10), cancellationToken);
                        continue;
                    }

                    var untilScheduled = queuedScan.ScheduledAtUtc - DateTime.UtcNow;
                    if (untilScheduled > TimeSpan.Zero)
                    {
                        await _queueWakeSignal.WaitAsync(untilScheduled < TimeSpan.FromSeconds(30)
                            ? untilScheduled : TimeSpan.FromSeconds(30), cancellationToken);
                        continue;
                    }

                    _queueCountdownScanId = queuedScan.ScanId;
                    using var delayCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    _queueCountdownCancellation = delayCancellation;
                    var delaySeconds = _configuration?.ScanStartDelaySeconds ?? 5;
                    for (var seconds = delaySeconds; seconds > 0; seconds--)
                    {
                        lblOperationStatus.Text = $"Scan {queuedScan.ScanId} starts in {seconds} seconds...";
                        await Task.Delay(1000, delayCancellation.Token);
                    }

                    if (_queuePaused || cancellationToken.IsCancellationRequested) continue;
                    if (!await _scans.TryStartQueuedScanAsync(queuedScan.ScanId, cancellationToken)) continue;

                    await RefreshQueueAsync();
                    var profile = new ArchiveProfile(queuedScan.ProfileId, queuedScan.ProfileName, queuedScan.RootPath, true);
                    await RunQueuedScanAsync(queuedScan.ScanId, profile, cancellationToken);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    // A user aborted the pending job or paused during its pre-start delay.
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    await _log.WriteAsync("Error", "QueueWorkerFailed", "The SQL scan queue worker hit an error.", exception);
                    if (!IsDisposed)
                        lblOperationStatus.Text = "Queue worker error; it will retry.";
                    await _queueWakeSignal.WaitAsync(TimeSpan.FromSeconds(10), cancellationToken);
                }
                finally
                {
                    _queueCountdownCancellation = null;
                    _queueCountdownScanId = null;
                    if (!cancellationToken.IsCancellationRequested && !_queuePaused && !_scanActive)
                        lblOperationStatus.Text = "OperationStatus: Ready";
                    if (!IsDisposed)
                        await RefreshQueueAsync();
                }
            }
        }

        private async Task RunQueuedScanAsync(long scanId, ArchiveProfile profile, CancellationToken queueToken)
        {
            _scanCancellation = CancellationTokenSource.CreateLinkedTokenSource(queueToken);
            _scanActive = true;
            btnScan.Enabled = false;
            btnCancel.Enabled = true;
            lblOperationStatus.Text = "Operation Status: Scanning...";
            lblCurrentScanStatus.Text = "Running";
            lblScanId.Text = $"Scan ID: {scanId}";
            lblCommit.Text = $"Commits to {_configuration!.Database.DatabaseName} : 0";
            ResetScanErrorDisplay();
            _scanTimer = Stopwatch.StartNew();
            lblStartTime.Text = $"Started: {DateTime.UtcNow:dd-MM-yyyy HH:mm:ss} UTC";
            lblArchiveProfile.Text = $"Profile: {profile.ProfileName}";
            lblFilesProcessed.Text = "Files: 0";
            lblDirectoriesProcessed.Text = "Directories: 0";
            lblBytesHashed.Text = "Bytes hashed: 0";
            toolStripProgressBar.Visible = true;
            toolStripProgressBar.Style = ProgressBarStyle.Marquee;
            tabControl1.SelectedTab = tabPage2;
            try
            {
                var progress = new Progress<ScanProgress>(p =>
                {
                    lblScanId.Text = $"Scan ID: {p.ScanId}";
                    lblFilesProcessed.Text = $"Files: {p.FilesProcessed:N0}";
                    lblDirectoriesProcessed.Text = $"Directories: {p.DirectoriesProcessed:N0}";
                    lblBytesHashed.Text = $"Bytes hashed: {FormatBytes(p.BytesHashed)}";
                    lblCurrentDirectory.Text = $"Directory: {p.CurrentDirectory}";
                    lblCurrentFile.Text = $"File: {p.CurrentFile}";
                    lblCurrentOperation.Text = p.Operation switch
                    {
                        "Saving to SQL" => "Saving to SQL",
                        "Operation Status: Ready" => "Scan complete",
                        _ => "Hashing files and saving inventory"
                    };
                    lblOperationStatus.Text = p.Operation;
                    lblCommit.Text = $"Commits to {_configuration!.Database.DatabaseName} : {p.CommitCount:N0}";
                    lblDiscoveredSize.Text = $"Discovered size: {FormatBytes(p.TotalSizeDiscovered)}";
                    label14.Text = $"Error count: {p.ErrorCount:N0}";
                    lblElapsedTime.Text = $"Elapsed: {_scanTimer?.Elapsed:hh\\:mm\\:ss}";
                    if (p.ErrorCount > 0)
                    {
                        dataGridView2_errors.Visible = true;
                        lblErrReport.Visible = false;
                    }
                    toolStripProgressBar.Style = ProgressBarStyle.Marquee;
                });
                await Task.Run(() => _scanService!.ScanAsync(scanId, profile, progress, _scanCancellation.Token), _scanCancellation.Token);
            }
            catch (OperationCanceledException)
            {
                lblCurrentScanStatus.Text = "Cancelled";
                lblOperationStatus.Text = "Cancelled";
            }
            catch (Exception exception)
            {
                lblCurrentScanStatus.Text = "Failed";
                lblOperationStatus.Text = "Error";
                MessageBox.Show(this, $"Scan {scanId} failed: {exception.Message}", "Scan error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _scanCancellation.Dispose();
                _scanCancellation = null;
                _scanActive = false;
                _scanTimer?.Stop();
                btnScan.Enabled = _activeProfiles.Count > 0;
                btnCancel.Enabled = false;
                toolStripProgressBar.Visible = false;
                await RefreshProfileAsync();
                var errors = await _queries!.GetErrorsAsync(scanId);
                dataGridView2_errors.Rows.Clear();
                foreach (var error in errors)
                    dataGridView2_errors.Rows.Add(error.RelativePath ?? string.Empty, error.ErrorType, error.ErrorMessage, error.OccurredAtUtc);
                dataGridView2_errors.Visible = errors.Count > 0;
                lblErrReport.Text = errors.Count == 0 ? "No scan errors." : $"{errors.Count:N0} scan errors.";
                lblErrReport.Visible = errors.Count == 0;
            }
        }

        private void SignalQueueWorker()
        {
            if (_queueWakeSignal.CurrentCount == 0)
            {
                try { _queueWakeSignal.Release(); }
                catch (SemaphoreFullException) { }
            }
        }

        private async void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_allowFormClose) return;
            if (_shutdownInProgress)
            {
                e.Cancel = true;
                return;
            }

            if (_scanActive && MessageBox.Show(this,
                    "A scan is running. Exit and cancel it after the current SQL work is finalized? Pending queue entries will remain in SQL.",
                    "Exit during scan?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }

            if (_queueWorkerTask is null || _queueWorkerTask.IsCompleted)
            {
                _allowFormClose = true;
                if (_queueWorkerLease is not null)
                    await _queueWorkerLease.DisposeAsync();
                return;
            }

            e.Cancel = true;
            _shutdownInProgress = true;
            _queueWorkerCancellation?.Cancel();
            _queueCountdownCancellation?.Cancel();
            _scanCancellation?.Cancel();
            try { await _queueWorkerTask; }
            catch (OperationCanceledException) { }
            finally
            {
                _queueWorkerCancellation?.Dispose();
                _queueWorkerCancellation = null;
                if (_queueWorkerLease is not null)
                {
                    await _queueWorkerLease.DisposeAsync();
                    _queueWorkerLease = null;
                }
                _allowFormClose = true;
                Close();
            }
        }

        private void ConfirmCancelScan()
        {
            if (!_scanActive || _scanCancellation is null) return;

            var result = MessageBox.Show(
                this,
                "Are you sure you want to cancel the current scan?\n\n" +
                "The scan will stop after saving the files and errors collected so far, and its status will be marked as Cancelled.",
                "Cancel scan?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
                _scanCancellation.Cancel();
        }

        private void ResetScanErrorDisplay()
        {
            dataGridView2_errors.Rows.Clear();
            dataGridView2_errors.Visible = false;
            lblErrReport.Text = "Scan in progress — checking for errors...";
            lblErrReport.Visible = true;
        }

        private void SetDatabaseStatus(string status) => lblDatabaseStatus.Text = $"DB Status: {status}";
        private static string FormatBytes(long bytes)
        {
            string[] units = ["B", "KB", "MB", "GB", "TB"];
            double value = bytes;
            var unit = 0;
            while (value >= 1024 && unit < units.Length - 1) { value /= 1024; unit++; }
            return $"{value:0.##} {units[unit]}";
        }

        private async Task TestConfiguredConnectionAsync()
        {
            if (_configuration is null) return;
            _configuration.Database.Server = txtServerAddress.Text.Trim();
            _configuration.Database.DatabaseName = string.IsNullOrWhiteSpace(cmbDatabaseName.Text) ? "ArchiveAuditor" : cmbDatabaseName.Text.Trim();
            _configuration.Database.UseIntegratedSecurity = string.IsNullOrWhiteSpace(txtUsername.Text);
            _configuration.Database.UserName = txtUsername.Text.Trim();
            _configuration.Database.Password = txtPassword.Text;
            try
            {
                SetDatabaseStatus("Connecting");
                await new SqlServerAdministrationService(_configuration).TestConnectionAsync();
                var databaseNames = await new SqlServerAdministrationService(_configuration).GetAvailableDatabasesAsync();
                var selectedDatabase = cmbDatabaseName.Text.Trim();
                cmbDatabaseName.Items.Clear();
                cmbDatabaseName.Items.AddRange(databaseNames.Cast<object>().ToArray());
                cmbDatabaseName.Text = selectedDatabase;
                _configuration.Database.DatabaseName = selectedDatabase;
                if (databaseNames.Contains(selectedDatabase, StringComparer.OrdinalIgnoreCase))
                    await new SqlServerAdministrationService(_configuration).TestTargetDatabaseAsync();
                lblConnectionTestResult.Text = "Connection succeeded";
                SetDatabaseStatus("Connected");
                _configuration.Database.DatabaseName = cmbDatabaseName.Text.Trim();
                _configuration.Database.UserName = txtUsername.Text.Trim();
                _configuration.Database.Password = txtPassword.Text;
                _configuration.ExportDirectory = txtExportDirectory.Text.Trim();
                _configuration.DisplayTimestampsInUtc = chkUseGMT.Checked;
                await _configurationStore.SaveAsync(_configuration);
            }
            catch (Exception exception)
            {
                lblConnectionTestResult.Text = "Connection failed";
                SetDatabaseStatus("Connection failed");
                MessageBox.Show(this, exception.Message, "Connection test", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task AddProfileAsync()
        {
            using var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            var name = Microsoft.VisualBasic.Interaction.InputBox("Profile name:", "Add archive profile", Path.GetFileName(dialog.SelectedPath));
            if (string.IsNullOrWhiteSpace(name)) return;
            if (IsApplicationInsideArchive(dialog.SelectedPath))
            {
                MessageBox.Show(this, "The application directory must be outside the monitored archive because settings.json is stored beside the executable.", "Invalid archive root", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                await _profiles!.AddProfileAsync(name, dialog.SelectedPath);
                _activeProfiles = await _profiles.GetProfilesAsync(activeOnly: true);
                await SaveProfileConfigurationAsync();
                PopulateProfiles();
            }
            catch (Exception exception) { MessageBox.Show(this, exception.Message, "Profile error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private async Task<ScanSnapshot?> GetLatestAnalyzableScanAsync()
        {
            if (cmbArchiveProfile.SelectedItem is not ArchiveProfile profile || _queries is null) return null;
            return (await _queries.GetScansAsync(profile.ProfileId, completedOnly: true)).FirstOrDefault();
        }

        private async Task ShowLargestFilesAsync()
        {
            try
            {
                var scan = await GetLatestAnalyzableScanAsync();
                if (scan is null) { lblLargestFilesResult.Text = "No completed scan is available."; return; }
                var service = new SqlArchiveAnalysisService(new SqlServerConnectionFactory(_configuration!));
                var minimumSizeBytes = decimal.ToInt64(decimal.Round(numLargestFiles.Value * 1024m * 1024m * 1024m, 0, MidpointRounding.AwayFromZero));
                var summary = await service.GetLargeFilesSummaryAsync(scan.ScanId, minimumSizeBytes);
                var partialNote = scan.Status == ScanStatus.CompletedWithErrors ? $" (partial scan; {scan.ErrorCount:N0} errors)" : string.Empty;
                lblLargestFilesResult.Text = $"{summary.Count:N0} large files in {FormatBytes(summary.TotalBytes)}{partialNote}";
                lblOperationStatus.Text = $"Found {summary.Count:N0} large files";
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Largest files", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private async Task ShowDuplicatesAsync()
        {
            try
            {
                var scan = await GetLatestAnalyzableScanAsync();
                if (scan is null) { lblDuplicateCount.Text = "No completed scan"; return; }
                lblOperationStatus.Text = "Analysing";
                var summary = await new SqlArchiveAnalysisService(new SqlServerConnectionFactory(_configuration!)).GetDuplicatesAsync(scan.ScanId);
                var partialNote = scan.Status == ScanStatus.CompletedWithErrors ? $" · partial scan ({scan.ErrorCount:N0} errors)" : string.Empty;
                lblDuplicateCount.Text = $"Duplicate groups: {summary.DuplicateGroups:N0} ({summary.ExtraCopies:N0} extra copies){partialNote}";
                lblDuplicateSpace.Text = $"Potentially reclaimable: {FormatBytes(summary.WastedBytes)}";
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Duplicate analysis", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { lblOperationStatus.Text = "Ready"; }
        }

        private async Task ShowDirectoryAnalysisAsync()
        {
            try
            {
                var scan = await GetLatestAnalyzableScanAsync();
                if (scan is null) { label6.Text = "No completed scan is available."; return; }
                var stats = await new SqlArchiveAnalysisService(new SqlServerConnectionFactory(_configuration!)).GetDirectoryStatisticsAsync(scan.ScanId);
                var partialNote = scan.Status == ScanStatus.CompletedWithErrors ? $" · partial scan ({scan.ErrorCount:N0} errors)" : string.Empty;
                label6.Text = $"Empty directories: {stats.EmptyDirectories:N0} · Directories: {stats.Directories:N0} · Files: {stats.Files:N0} · Total size: {FormatBytes(stats.Bytes)}{partialNote}";
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Directory analysis", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private async Task VerifyArchiveAsync()
        {
            if (cmbArchiveProfile.SelectedItem is not ArchiveProfile profile || _queries is null) return;
            var baselines = (await _queries.GetScansAsync(profile.ProfileId, completedOnly: true))
                .Where(scan => scan.Status == ScanStatus.Completed)
                .ToList();
            using var picker = new ScanPickerDialog(baselines);
            if (picker.ShowDialog(this) != DialogResult.OK || picker.SelectedScan is not ScanSnapshot baseline) return;
            try
            {
                lblOperationStatus.Text = "Verifying...";
                btnFullVerification.Enabled = false;
                var verifier = new ArchiveVerificationService(new SqlServerConnectionFactory(_configuration!));
                var result = await Task.Run(() => verifier.VerifyAsync(baseline.ScanId, profile.RootPath));
                lblLastVerification.Text = $"Last verification: {DateTime.UtcNow:dd-MMM-yy HH:mm} UTC";
                lblVerificationResult.Text = $"Checked {result.Checked:N0}; missing {result.Missing:N0}; modified {result.Modified:N0}; additional {result.Additional:N0}; unreadable/errors {result.Unreadable:N0}.";
                await _log.WriteAsync("Information", "VerificationCompleted", $"Verified profile {profile.ProfileId} against scan {baseline.ScanId}.");
                if (result.SampleMessages.Count > 0)
                    MessageBox.Show(this, string.Join(Environment.NewLine, result.SampleMessages.Take(20)), "Verification details (first 20)", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException) { lblVerificationResult.Text = "Verification cancelled."; }
            catch (Exception ex)
            {
                lblVerificationResult.Text = "Verification failed.";
                await _log.WriteAsync("Error", "VerificationFailed", "Verification failed.", ex);
                MessageBox.Show(this, ex.Message, "Verification error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { btnFullVerification.Enabled = true; lblOperationStatus.Text = "Ready"; }
        }

        private async Task RemoveProfileAsync()
        {
            if (dataGridView2.CurrentRow is null || !int.TryParse(Convert.ToString(dataGridView2.CurrentRow.Cells[0].Value), out var id)) return;
            if (MessageBox.Show(this, "Deactivate this profile? Historical scans will be preserved.", "Remove profile", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            await _profiles!.DeactivateProfileAsync(id);
            _activeProfiles = await _profiles.GetProfilesAsync(activeOnly: true);
            await SaveProfileConfigurationAsync();
            PopulateProfiles();
        }

        private async Task EditProfileAsync()
        {
            if (dataGridView2.CurrentRow is null || !int.TryParse(Convert.ToString(dataGridView2.CurrentRow.Cells[0].Value), out var id)) return;
            var selected = _activeProfiles.FirstOrDefault(p => p.ProfileId == id);
            if (selected is null) return;
            var name = Microsoft.VisualBasic.Interaction.InputBox("Profile name:", "Edit archive profile", selected.ProfileName);
            if (string.IsNullOrWhiteSpace(name)) return;
            using var dialog = new FolderBrowserDialog { SelectedPath = selected.RootPath };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            if (IsApplicationInsideArchive(dialog.SelectedPath))
            {
                MessageBox.Show(this, "The application directory must be outside the monitored archive.", "Invalid archive root", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            await _profiles!.EditProfileAsync(id, name, dialog.SelectedPath);
            _activeProfiles = await _profiles.GetProfilesAsync(activeOnly: true);
            PopulateProfiles();
        }

        private async Task ActivateProfileAsync()
        {
            try
            {
                var inactiveProfiles = (await _profiles!.GetProfilesAsync(activeOnly: false))
                    .Where(profile => !profile.IsActive)
                    .ToList();
                if (inactiveProfiles.Count == 0)
                {
                    MessageBox.Show(this, "There are no deactivated profiles to activate.", "Activate profile", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using var picker = new ProfilePickerDialog(inactiveProfiles);
                if (picker.ShowDialog(this) != DialogResult.OK || picker.SelectedProfile is not ArchiveProfile selected)
                    return;
                if (IsApplicationInsideArchive(selected.RootPath))
                {
                    MessageBox.Show(this, "The application directory is inside this archive. Move the application folder or choose another profile before activating it.", "Invalid archive root", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await _profiles.ActivateProfileAsync(selected.ProfileId);
                _activeProfiles = await _profiles.GetProfilesAsync(activeOnly: true);
                await SaveProfileConfigurationAsync();
                PopulateProfiles();
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Profile activation error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void BrowseExportDirectory()
        {
            using var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog(this) == DialogResult.OK) txtExportDirectory.Text = dialog.SelectedPath;
        }

        private static bool IsApplicationInsideArchive(string root)
        {
            var relative = Path.GetRelativePath(Path.GetFullPath(root), Path.GetFullPath(AppContext.BaseDirectory));
            return relative == "." || (!Path.IsPathRooted(relative) && relative != ".." && !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal));
        }

        private bool IsPathInsideArchive(string path)
        {
            var fullPath = Path.GetFullPath(path);
            return _activeProfiles.Any(profile =>
            {
                var relative = Path.GetRelativePath(Path.GetFullPath(profile.RootPath), fullPath);
                return relative == "." || (!Path.IsPathRooted(relative) && relative != ".." && !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal));
            });
        }

        private async Task ShowDatabaseStatisticsAsync()
        {
            try
            {
                SetDatabaseStatus("Checking database...");
                var result = await new SqlMaintenanceService(new SqlServerConnectionFactory(_configuration!)).GetStatisticsAsync();
                MessageBox.Show(this, result, "Database statistics", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Database statistics", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { SetDatabaseStatus("Ready"); }
        }

        private async Task CheckDatabaseAsync()
        {
            try
            {
                SetDatabaseStatus("Checking database");
                lblOperationStatus.Text = "Analysing";
                await new SqlMaintenanceService(new SqlServerConnectionFactory(_configuration!)).CheckDatabaseAsync();
                MessageBox.Show(this, "SQL Server reports no database consistency errors.", "Database check", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Database check", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { SetDatabaseStatus("Ready"); lblOperationStatus.Text = "Ready"; }
        }

        private async Task BackupConfigurationAsync()
        {
            using var dialog = new SaveFileDialog { Filter = "JSON configuration (*.json)|*.json", FileName = "ArchiveAuditor-settings-backup.json" };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            if (IsPathInsideArchive(dialog.FileName))
            {
                MessageBox.Show(this, "Choose a backup location outside every monitored archive.", "Invalid backup path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                await _configurationStore.SaveAsync(_configuration!);
                File.Copy(_configurationStore.ConfigurationPath, dialog.FileName, overwrite: true);
                MessageBox.Show(this, "Configuration backup saved. It contains the SQL credentials in plain text.", "Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Backup error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private async Task RestoreConfigurationAsync()
        {
            using var dialog = new OpenFileDialog { Filter = "JSON configuration (*.json)|*.json" };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                var restored = await new ConfigurationStore(dialog.FileName).LoadAsync()
                    ?? throw new InvalidDataException("The selected configuration file is empty.");
                await new SqlServerAdministrationService(restored).TestConnectionAsync();
                await new SqlSchemaService(new SqlServerConnectionFactory(restored)).EnsureCompatibleAsync();
                await _configurationStore.SaveAsync(restored);
                _configuration = restored;
                InitializeServices();
                _activeProfiles = await _profiles!.GetProfilesAsync(activeOnly: true);
                await SaveProfileConfigurationAsync();
                PopulateProfiles();
                MessageBox.Show(this, "Configuration restored and database connection validated.", "Restore", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Restore error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private async Task CompareAsync()
        {
            if (cmbArchiveProfile.SelectedItem is not ArchiveProfile profile || cmbBaselineScan.SelectedItem is not ScanSnapshot previous || cmbCurrentScan.SelectedItem is not ScanSnapshot current)
            {
                MessageBox.Show(this, "Select a profile and two completed scans (with or without errors).", "Compare", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (previous.ScanId == current.ScanId)
            {
                MessageBox.Show(this, "Choose two different scans.", "Compare", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (previous.Status == ScanStatus.CompletedWithErrors || current.Status == ScanStatus.CompletedWithErrors)
            {
                var partialScans = new[] { previous, current }
                    .Where(scan => scan.Status == ScanStatus.CompletedWithErrors)
                    .Select(scan => $"Scan {scan.ScanId}: {scan.ErrorCount:N0} error(s)");
                var warning = "At least one selected scan is partial. Files that could not be read may appear as deleted or added in the comparison." +
                    Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, partialScans) +
                    Environment.NewLine + Environment.NewLine + "Continue with the comparison?";
                if (MessageBox.Show(this, warning, "Compare partial scans?", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                    return;
            }
            try
            {
                lblOperationStatus.Text = "Comparing";
                SetDatabaseStatus("Loading comparison results");
                _lastComparison = await new SqlComparisonService(new SqlServerConnectionFactory(_configuration!))
                    .CompareAsync(profile.ProfileId, previous.ScanId, current.ScanId);
                RenderComparison();
                await _log.WriteAsync("Information", "ComparisonCompleted", $"Compared scans {previous.ScanId} and {current.ScanId}.");
            }
            catch (Exception exception)
            {
                await _log.WriteAsync("Error", "ComparisonFailed", "Comparison failed.", exception);
                MessageBox.Show(this, exception.Message, "Comparison error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { lblOperationStatus.Text = "Ready"; SetDatabaseStatus("Ready"); }
        }

        private void RenderComparison()
        {
            dataGridView1.Rows.Clear();
            if (_lastComparison is null) return;
            var filter = Convert.ToString(cmbChangeFilter.SelectedItem) ?? "All";
            foreach (var row in _lastComparison.Changes.Where(row => filter == "All" || row.ChangeType == filter))
                dataGridView1.Rows.Add(row.RecordType, row.RelativePath, row.ChangeType,
                    row.PreviousSize?.ToString("N0") ?? "", row.CurrentSize?.ToString("N0") ?? "",
                    FormatHashForDisplay(row.PreviousHash), FormatHashForDisplay(row.CurrentHash));
            lblOperationStatus.Text = $"Added {_lastComparison.Changes.Count(c => c.ChangeType == "Added")}; deleted {_lastComparison.Changes.Count(c => c.ChangeType == "Deleted")}; modified {_lastComparison.Changes.Count(c => c.ChangeType == "Modified")}; unchanged {_lastComparison.UnchangedFiles:N0}; size {_lastComparison.SizeDifferenceBytes:+#,##0;-#,##0;0} bytes";
            _comparisonSummaryLabel.Text = lblOperationStatus.Text;
            lblLastComparison.Text = $"Last comparison: {_lastComparison.ComparedAtUtc:MM-dd-MM-yyyy HH:mm} UTC";
        }

        private static string FormatHashForDisplay(string? hash) =>
            string.IsNullOrEmpty(hash) || hash.Length <= 8 ? hash ?? string.Empty : $"{hash[..4]}...{hash[^4..]}";

        private async Task ExportAsync(bool html)
        {
            if (_lastComparison is null || _configuration is null)
            {
                MessageBox.Show(this, "Run a comparison before exporting a report.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using var dialog = new SaveFileDialog
            {
                Filter = html ? "HTML report (*.html)|*.html" : "CSV file (*.csv)|*.csv",
                FileName = $"ArchiveAuditor-{_lastComparison.ComparisonId}.{(html ? "html" : "csv")}",
                InitialDirectory = Directory.Exists(_configuration.ExportDirectory) ? _configuration.ExportDirectory : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            if (IsPathInsideArchive(dialog.FileName))
            {
                MessageBox.Show(this, "Choose an export location outside every monitored archive.", "Invalid export path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                lblOperationStatus.Text = "Exporting";
                if (html)
                {
                    var history = (await _queries!.GetScansAsync(_lastComparison.ProfileId, completedOnly: true))
                        .Where(scan => scan.Status == ScanStatus.Completed)
                        .ToList();
                    var profileName = _activeProfiles.FirstOrDefault(p => p.ProfileId == _lastComparison.ProfileId)?.ProfileName ?? "Archive";
                    await _reportExporter.ExportHtmlAsync(dialog.FileName, profileName, _lastComparison, history, CancellationToken.None);
                }
                else await _reportExporter.ExportCsvAsync(dialog.FileName, _lastComparison, CancellationToken.None);
                _configuration.ExportDirectory = Path.GetDirectoryName(dialog.FileName) ?? _configuration.ExportDirectory;
                await _configurationStore.SaveAsync(_configuration);
                await _log.WriteAsync("Information", "ReportExported", $"Exported report to {dialog.FileName}.");
                MessageBox.Show(this, "Report exported.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                await _log.WriteAsync("Error", "ReportExportFailed", "Report export failed.", exception);
                MessageBox.Show(this, exception.Message, "Export error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { lblOperationStatus.Text = "Ready"; }
        }

        private void cmbChangeFilter_SelectedIndexChanged(object? sender, EventArgs e) => RenderComparison();

        private void filesToolStripMenuItem_Click(object sender, EventArgs e) { }
        private void documentsToolStripMenuItem_Click(object sender, EventArgs e) => tabControl1.SelectedTab = tabPage6;

        private void btnScan_Click_1(object sender, EventArgs e)
        {

        }

        private void btnFullVerification_Click(object sender, EventArgs e)
        {

        }

        private void newScanToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
