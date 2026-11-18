namespace IMS
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            menuStrip1 = new MenuStrip();
            filesToolStripMenuItem = new ToolStripMenuItem();
            newScanToolStripMenuItem = new ToolStripMenuItem();
            exportToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            profilesToolStripMenuItem = new ToolStripMenuItem();
            selectProfileToolStripMenuItem = new ToolStripMenuItem();
            addProfileToolStripMenuItem = new ToolStripMenuItem();
            editProfileToolStripMenuItem = new ToolStripMenuItem();
            activateProfileToolStripMenuItem = new ToolStripMenuItem();
            toolsToolStripMenuItem = new ToolStripMenuItem();
            compareScansStripMenuItem = new ToolStripMenuItem();
            toolStripMenuAdvanced = new ToolStripMenuItem();
            databaseStatusToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            versionsToolStripMenuItem = new ToolStripMenuItem();
            documentsToolStripMenuItem = new ToolStripMenuItem();
            statusStrip1 = new StatusStrip();
            lblGMTClock = new ToolStripStatusLabel();
            toolStripProgressBar = new ToolStripProgressBar();
            toolStripStatusLabel2 = new ToolStripStatusLabel();
            lblDatabaseStatus = new ToolStripStatusLabel();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            lblOperationStatus = new ToolStripStatusLabel();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            groupBox15 = new GroupBox();
            btnPauseQueue = new Button();
            lblScheduledStart = new Label();
            dtpScheduledStart = new DateTimePicker();
            btnCancelQueuedScan = new Button();
            dgvScanQueue = new DataGridView();
            btnAddToQueue = new Button();
            groupBox4 = new GroupBox();
            lblLastScanStatus = new Label();
            lblLastDuration = new Label();
            lblLastComparison = new Label();
            lblLastScan = new Label();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            dgvScanHistory = new DataGridView();
            btnExit = new Button();
            lblTotalSize = new Label();
            lblTotalDirectories = new Label();
            lblTotalFiles = new Label();
            BtnGoCompare = new Button();
            btnScan = new Button();
            txtRootPath = new TextBox();
            cmbArchiveProfile = new ComboBox();
            tabPage2 = new TabPage();
            btnBackgroun = new Button();
            groupBox8 = new GroupBox();
            lblErrReport = new Label();
            dataGridView2_errors = new DataGridView();
            groupBox7 = new GroupBox();
            lblCurrentOperation = new Label();
            lblCurrentDirectory = new Label();
            lblCurrentFile = new Label();
            groupBox6 = new GroupBox();
            lblCommit = new Label();
            lblFilesProcessed = new Label();
            lblDiscoveredSize = new Label();
            lblElapsedTime = new Label();
            lblDirectoriesProcessed = new Label();
            lblBytesHashed = new Label();
            groupBox5 = new GroupBox();
            lblCurrentScanStatus = new Label();
            lblStartTime = new Label();
            lblArchiveProfile = new Label();
            lblScanId = new Label();
            label14 = new Label();
            btnCancel = new Button();
            tabPage3 = new TabPage();
            label7 = new Label();
            label5 = new Label();
            cmbChangeFilter = new ComboBox();
            dataGridView1 = new DataGridView();
            btnExportHTML = new Button();
            btnExportCSV = new Button();
            btnCompare = new Button();
            cmbCurrentScan = new ComboBox();
            cmbBaselineScan = new ComboBox();
            tabPage4 = new TabPage();
            groupBox13 = new GroupBox();
            groupBox12 = new GroupBox();
            groupBox10 = new GroupBox();
            lblDuplicateSpace = new Label();
            lblDuplicateCount = new Label();
            btnFindDuplicates = new Button();
            btnEmptyDirectories = new Button();
            label6 = new Label();
            groupBox11 = new GroupBox();
            lblLargestFilesResult = new Label();
            numLargestFiles = new NumericUpDown();
            btnLargestFiles = new Button();
            groupBox9 = new GroupBox();
            lblVerificationResult = new Label();
            lblLastVerification = new Label();
            btnFullVerification = new Button();
            tabPage5 = new TabPage();
            groupBox2 = new GroupBox();
            lblConnectionTestResult = new Label();
            lblPassword = new Label();
            txtPassword = new TextBox();
            lblUsername = new Label();
            txtUsername = new TextBox();
            btnTestConnection = new Button();
            cmbDatabaseName = new ComboBox();
            lblDatabaseName = new Label();
            lblServerAddress = new Label();
            txtServerAddress = new TextBox();
            groupBox3 = new GroupBox();
            btnActivateProfile = new Button();
            btnRemoveProfile = new Button();
            btnEditProfile = new Button();
            btnAddProfile = new Button();
            dataGridView2 = new DataGridView();
            groupBox1 = new GroupBox();
            btnRestoreConfiguration = new Button();
            btnBackupConfiguration = new Button();
            btnCheckDatabase = new Button();
            btnDatabaseStatistics = new Button();
            chkUseGMT = new CheckBox();
            btnBrowseExportDirectory = new Button();
            txtExportDirectory = new TextBox();
            lblExportDirectory = new Label();
            tabPage6 = new TabPage();
            pictureBox1 = new PictureBox();
            groupBox14 = new GroupBox();
            textBox1 = new TextBox();
            label1 = new Label();
            menuStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            groupBox15.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvScanQueue).BeginInit();
            groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvScanHistory).BeginInit();
            tabPage2.SuspendLayout();
            groupBox8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView2_errors).BeginInit();
            groupBox7.SuspendLayout();
            groupBox6.SuspendLayout();
            groupBox5.SuspendLayout();
            tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            tabPage4.SuspendLayout();
            groupBox12.SuspendLayout();
            groupBox10.SuspendLayout();
            groupBox11.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numLargestFiles).BeginInit();
            groupBox9.SuspendLayout();
            tabPage5.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            groupBox1.SuspendLayout();
            tabPage6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            groupBox14.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Font = new Font("Segoe UI", 10F);
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { filesToolStripMenuItem, profilesToolStripMenuItem, toolsToolStripMenuItem, helpToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1182, 31);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // filesToolStripMenuItem
            // 
            filesToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newScanToolStripMenuItem, exportToolStripMenuItem, exitToolStripMenuItem });
            filesToolStripMenuItem.Name = "filesToolStripMenuItem";
            filesToolStripMenuItem.Size = new Size(49, 27);
            filesToolStripMenuItem.Text = "File";
            filesToolStripMenuItem.Click += filesToolStripMenuItem_Click;
            // 
            // newScanToolStripMenuItem
            // 
            newScanToolStripMenuItem.Name = "newScanToolStripMenuItem";
            newScanToolStripMenuItem.Size = new Size(199, 28);
            newScanToolStripMenuItem.Text = "Scan";
            newScanToolStripMenuItem.Click += newScanToolStripMenuItem_Click;
            // 
            // exportToolStripMenuItem
            // 
            exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            exportToolStripMenuItem.Size = new Size(199, 28);
            exportToolStripMenuItem.Text = "Export Report";
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(199, 28);
            exitToolStripMenuItem.Text = "Exit";
            // 
            // profilesToolStripMenuItem
            // 
            profilesToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { selectProfileToolStripMenuItem, addProfileToolStripMenuItem, editProfileToolStripMenuItem, activateProfileToolStripMenuItem });
            profilesToolStripMenuItem.Name = "profilesToolStripMenuItem";
            profilesToolStripMenuItem.Size = new Size(79, 27);
            profilesToolStripMenuItem.Text = "Profiles";
            // 
            // selectProfileToolStripMenuItem
            // 
            selectProfileToolStripMenuItem.Name = "selectProfileToolStripMenuItem";
            selectProfileToolStripMenuItem.Size = new Size(208, 28);
            selectProfileToolStripMenuItem.Text = "Select Profile";
            // 
            // addProfileToolStripMenuItem
            // 
            addProfileToolStripMenuItem.Name = "addProfileToolStripMenuItem";
            addProfileToolStripMenuItem.Size = new Size(208, 28);
            addProfileToolStripMenuItem.Text = "Add Profile";
            // 
            // editProfileToolStripMenuItem
            // 
            editProfileToolStripMenuItem.Name = "editProfileToolStripMenuItem";
            editProfileToolStripMenuItem.Size = new Size(208, 28);
            editProfileToolStripMenuItem.Text = "Edit Profile";
            // 
            // activateProfileToolStripMenuItem
            // 
            activateProfileToolStripMenuItem.Name = "activateProfileToolStripMenuItem";
            activateProfileToolStripMenuItem.Size = new Size(208, 28);
            activateProfileToolStripMenuItem.Text = "Activate Profile";
            // 
            // toolsToolStripMenuItem
            // 
            toolsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { compareScansStripMenuItem, toolStripMenuAdvanced, databaseStatusToolStripMenuItem });
            toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            toolsToolStripMenuItem.Size = new Size(62, 27);
            toolsToolStripMenuItem.Text = "Tools";
            // 
            // compareScansStripMenuItem
            // 
            compareScansStripMenuItem.Name = "compareScansStripMenuItem";
            compareScansStripMenuItem.Size = new Size(216, 28);
            compareScansStripMenuItem.Text = "Compare Scans";
            // 
            // toolStripMenuAdvanced
            // 
            toolStripMenuAdvanced.Name = "toolStripMenuAdvanced";
            toolStripMenuAdvanced.Size = new Size(216, 28);
            toolStripMenuAdvanced.Text = "Advanced Tools";
            // 
            // databaseStatusToolStripMenuItem
            // 
            databaseStatusToolStripMenuItem.Name = "databaseStatusToolStripMenuItem";
            databaseStatusToolStripMenuItem.Size = new Size(216, 28);
            databaseStatusToolStripMenuItem.Text = "Database Status";
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aboutToolStripMenuItem, versionsToolStripMenuItem, documentsToolStripMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(59, 27);
            helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(213, 28);
            aboutToolStripMenuItem.Text = "About";
            // 
            // versionsToolStripMenuItem
            // 
            versionsToolStripMenuItem.Name = "versionsToolStripMenuItem";
            versionsToolStripMenuItem.Size = new Size(213, 28);
            versionsToolStripMenuItem.Text = "Versions";
            // 
            // documentsToolStripMenuItem
            // 
            documentsToolStripMenuItem.Name = "documentsToolStripMenuItem";
            documentsToolStripMenuItem.Size = new Size(213, 28);
            documentsToolStripMenuItem.Text = "Documentation";
            documentsToolStripMenuItem.Click += documentsToolStripMenuItem_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.Font = new Font("Segoe UI", 10F);
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblGMTClock, toolStripProgressBar, toolStripStatusLabel2, lblDatabaseStatus, toolStripStatusLabel1, lblOperationStatus });
            statusStrip1.Location = new Point(0, 824);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1182, 29);
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
            // 
            // lblGMTClock
            // 
            lblGMTClock.Name = "lblGMTClock";
            lblGMTClock.Size = new Size(104, 23);
            lblGMTClock.Text = "lblGMTClock";
            lblGMTClock.ToolTipText = "Date/Time (GMT)";
            // 
            // toolStripProgressBar
            // 
            toolStripProgressBar.Name = "toolStripProgressBar";
            toolStripProgressBar.Size = new Size(200, 21);
            // 
            // toolStripStatusLabel2
            // 
            toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            toolStripStatusLabel2.Size = new Size(46, 23);
            toolStripStatusLabel2.Text = "^_^ ";
            // 
            // lblDatabaseStatus
            // 
            lblDatabaseStatus.Name = "lblDatabaseStatus";
            lblDatabaseStatus.Size = new Size(163, 23);
            lblDatabaseStatus.Text = "DB Status: Starting...";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(41, 23);
            toolStripStatusLabel1.Text = "<->";
            // 
            // lblOperationStatus
            // 
            lblOperationStatus.Name = "lblOperationStatus";
            lblOperationStatus.Size = new Size(151, 23);
            lblOperationStatus.Text = "lblOperationStatus";
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Controls.Add(tabPage4);
            tabControl1.Controls.Add(tabPage5);
            tabControl1.Controls.Add(tabPage6);
            tabControl1.Font = new Font("Segoe UI", 10F);
            tabControl1.ItemSize = new Size(100, 30);
            tabControl1.Location = new Point(0, 27);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1182, 794);
            tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(groupBox15);
            tabPage1.Controls.Add(groupBox4);
            tabPage1.Controls.Add(label4);
            tabPage1.Controls.Add(label3);
            tabPage1.Controls.Add(label2);
            tabPage1.Controls.Add(dgvScanHistory);
            tabPage1.Controls.Add(btnExit);
            tabPage1.Controls.Add(lblTotalSize);
            tabPage1.Controls.Add(lblTotalDirectories);
            tabPage1.Controls.Add(lblTotalFiles);
            tabPage1.Controls.Add(BtnGoCompare);
            tabPage1.Controls.Add(btnScan);
            tabPage1.Controls.Add(txtRootPath);
            tabPage1.Controls.Add(cmbArchiveProfile);
            tabPage1.Location = new Point(4, 34);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(1174, 756);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Dashboard";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBox15
            // 
            groupBox15.Controls.Add(btnPauseQueue);
            groupBox15.Controls.Add(lblScheduledStart);
            groupBox15.Controls.Add(dtpScheduledStart);
            groupBox15.Controls.Add(btnCancelQueuedScan);
            groupBox15.Controls.Add(dgvScanQueue);
            groupBox15.Controls.Add(btnAddToQueue);
            groupBox15.Location = new Point(10, 434);
            groupBox15.Name = "groupBox15";
            groupBox15.Size = new Size(1076, 246);
            groupBox15.TabIndex = 24;
            groupBox15.TabStop = false;
            groupBox15.Text = "Scan Queue ";
            // 
            // btnPauseQueue
            // 
            btnPauseQueue.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnPauseQueue.Location = new Point(709, 186);
            btnPauseQueue.Name = "btnPauseQueue";
            btnPauseQueue.Size = new Size(210, 50);
            btnPauseQueue.TabIndex = 29;
            btnPauseQueue.Text = "Pause Queue";
            btnPauseQueue.UseVisualStyleBackColor = true;
            // 
            // lblScheduledStart
            // 
            lblScheduledStart.AutoSize = true;
            lblScheduledStart.Location = new Point(709, 35);
            lblScheduledStart.Name = "lblScheduledStart";
            lblScheduledStart.Size = new Size(84, 23);
            lblScheduledStart.TabIndex = 28;
            lblScheduledStart.Text = "Start time";
            // 
            // dtpScheduledStart
            // 
            dtpScheduledStart.Location = new Point(818, 29);
            dtpScheduledStart.Name = "dtpScheduledStart";
            dtpScheduledStart.Size = new Size(218, 30);
            dtpScheduledStart.TabIndex = 27;
            // 
            // btnCancelQueuedScan
            // 
            btnCancelQueuedScan.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnCancelQueuedScan.Location = new Point(709, 130);
            btnCancelQueuedScan.Name = "btnCancelQueuedScan";
            btnCancelQueuedScan.Size = new Size(210, 50);
            btnCancelQueuedScan.TabIndex = 26;
            btnCancelQueuedScan.Text = "Cancel a &Job";
            btnCancelQueuedScan.UseVisualStyleBackColor = true;
            // 
            // dgvScanQueue
            // 
            dgvScanQueue.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvScanQueue.Location = new Point(17, 29);
            dgvScanQueue.Name = "dgvScanQueue";
            dgvScanQueue.RowHeadersWidth = 51;
            dgvScanQueue.Size = new Size(686, 211);
            dgvScanQueue.TabIndex = 0;
            // 
            // btnAddToQueue
            // 
            btnAddToQueue.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnAddToQueue.Location = new Point(709, 74);
            btnAddToQueue.Name = "btnAddToQueue";
            btnAddToQueue.Size = new Size(210, 50);
            btnAddToQueue.TabIndex = 25;
            btnAddToQueue.Text = "Add to Queue";
            btnAddToQueue.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(lblLastScanStatus);
            groupBox4.Controls.Add(lblLastDuration);
            groupBox4.Controls.Add(lblLastComparison);
            groupBox4.Controls.Add(lblLastScan);
            groupBox4.Location = new Point(8, 114);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(381, 314);
            groupBox4.TabIndex = 20;
            groupBox4.TabStop = false;
            groupBox4.Text = "Latest scan history";
            // 
            // lblLastScanStatus
            // 
            lblLastScanStatus.AutoSize = true;
            lblLastScanStatus.Location = new Point(20, 119);
            lblLastScanStatus.Name = "lblLastScanStatus";
            lblLastScanStatus.Size = new Size(132, 23);
            lblLastScanStatus.TabIndex = 23;
            lblLastScanStatus.Text = "Last Scan Status";
            // 
            // lblLastDuration
            // 
            lblLastDuration.AutoSize = true;
            lblLastDuration.Location = new Point(19, 156);
            lblLastDuration.Name = "lblLastDuration";
            lblLastDuration.Size = new Size(151, 23);
            lblLastDuration.TabIndex = 22;
            lblLastDuration.Text = "Last Scan duration";
            // 
            // lblLastComparison
            // 
            lblLastComparison.AutoSize = true;
            lblLastComparison.Location = new Point(20, 82);
            lblLastComparison.Name = "lblLastComparison";
            lblLastComparison.Size = new Size(134, 23);
            lblLastComparison.TabIndex = 21;
            lblLastComparison.Text = "Last comparison";
            // 
            // lblLastScan
            // 
            lblLastScan.AutoSize = true;
            lblLastScan.Location = new Point(20, 45);
            lblLastScan.Name = "lblLastScan";
            lblLastScan.Size = new Size(79, 23);
            lblLastScan.TabIndex = 20;
            lblLastScan.Text = "Last scan";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(465, 14);
            label4.Name = "label4";
            label4.Size = new Size(122, 23);
            label4.TabIndex = 18;
            label4.Text = "Root directory:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(137, 58);
            label3.Name = "label3";
            label3.Size = new Size(139, 23);
            label3.TabIndex = 17;
            label3.Text = "Archive statistics:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(8, 14);
            label2.Name = "label2";
            label2.Size = new Size(123, 23);
            label2.TabIndex = 16;
            label2.Text = "Archive profile:";
            // 
            // dgvScanHistory
            // 
            dgvScanHistory.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvScanHistory.Location = new Point(395, 114);
            dgvScanHistory.Name = "dgvScanHistory";
            dgvScanHistory.RowHeadersWidth = 51;
            dgvScanHistory.Size = new Size(691, 314);
            dgvScanHistory.TabIndex = 15;
            // 
            // btnExit
            // 
            btnExit.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnExit.Location = new Point(952, 700);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(210, 50);
            btnExit.TabIndex = 14;
            btnExit.Text = "E&xit";
            btnExit.UseVisualStyleBackColor = true;
            // 
            // lblTotalSize
            // 
            lblTotalSize.AutoSize = true;
            lblTotalSize.Location = new Point(675, 58);
            lblTotalSize.Name = "lblTotalSize";
            lblTotalSize.Size = new Size(79, 23);
            lblTotalSize.TabIndex = 10;
            lblTotalSize.Text = "Total size";
            // 
            // lblTotalDirectories
            // 
            lblTotalDirectories.AutoSize = true;
            lblTotalDirectories.Location = new Point(479, 58);
            lblTotalDirectories.Name = "lblTotalDirectories";
            lblTotalDirectories.Size = new Size(140, 23);
            lblTotalDirectories.TabIndex = 9;
            lblTotalDirectories.Text = "Directories count";
            // 
            // lblTotalFiles
            // 
            lblTotalFiles.AutoSize = true;
            lblTotalFiles.Location = new Point(332, 58);
            lblTotalFiles.Name = "lblTotalFiles";
            lblTotalFiles.Size = new Size(91, 23);
            lblTotalFiles.TabIndex = 8;
            lblTotalFiles.Text = "Files count";
            // 
            // BtnGoCompare
            // 
            BtnGoCompare.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            BtnGoCompare.Location = new Point(656, 700);
            BtnGoCompare.Name = "BtnGoCompare";
            BtnGoCompare.Size = new Size(210, 50);
            BtnGoCompare.TabIndex = 7;
            BtnGoCompare.Text = "C&ompare";
            BtnGoCompare.UseVisualStyleBackColor = true;
            // 
            // btnScan
            // 
            btnScan.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnScan.Location = new Point(8, 700);
            btnScan.Name = "btnScan";
            btnScan.Size = new Size(210, 50);
            btnScan.TabIndex = 6;
            btnScan.Text = "&Scan";
            btnScan.UseVisualStyleBackColor = true;
            btnScan.Click += btnScan_Click_1;
            // 
            // txtRootPath
            // 
            txtRootPath.Location = new Point(593, 11);
            txtRootPath.Name = "txtRootPath";
            txtRootPath.ReadOnly = true;
            txtRootPath.Size = new Size(453, 30);
            txtRootPath.TabIndex = 5;
            txtRootPath.Text = "Root directory";
            // 
            // cmbArchiveProfile
            // 
            cmbArchiveProfile.FormattingEnabled = true;
            cmbArchiveProfile.Location = new Point(137, 10);
            cmbArchiveProfile.Name = "cmbArchiveProfile";
            cmbArchiveProfile.Size = new Size(241, 31);
            cmbArchiveProfile.TabIndex = 4;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(btnBackgroun);
            tabPage2.Controls.Add(groupBox8);
            tabPage2.Controls.Add(groupBox7);
            tabPage2.Controls.Add(groupBox6);
            tabPage2.Controls.Add(groupBox5);
            tabPage2.Controls.Add(btnCancel);
            tabPage2.Location = new Point(4, 34);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(1174, 756);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Scan";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // btnBackgroun
            // 
            btnBackgroun.Location = new Point(224, 700);
            btnBackgroun.Name = "btnBackgroun";
            btnBackgroun.Size = new Size(210, 50);
            btnBackgroun.TabIndex = 34;
            btnBackgroun.Text = "Background";
            btnBackgroun.UseVisualStyleBackColor = true;
            // 
            // groupBox8
            // 
            groupBox8.Controls.Add(lblErrReport);
            groupBox8.Controls.Add(dataGridView2_errors);
            groupBox8.Location = new Point(15, 458);
            groupBox8.Name = "groupBox8";
            groupBox8.Size = new Size(654, 193);
            groupBox8.TabIndex = 33;
            groupBox8.TabStop = false;
            groupBox8.Text = "Scan Errors Log: ";
            // 
            // lblErrReport
            // 
            lblErrReport.AutoSize = true;
            lblErrReport.Location = new Point(19, 40);
            lblErrReport.Name = "lblErrReport";
            lblErrReport.Size = new Size(116, 23);
            lblErrReport.TabIndex = 19;
            lblErrReport.Text = "No Scan Error";
            lblErrReport.Visible = false;
            // 
            // dataGridView2_errors
            // 
            dataGridView2_errors.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView2_errors.Location = new Point(19, 40);
            dataGridView2_errors.Name = "dataGridView2_errors";
            dataGridView2_errors.RowHeadersWidth = 51;
            dataGridView2_errors.Size = new Size(556, 136);
            dataGridView2_errors.TabIndex = 18;
            dataGridView2_errors.Visible = false;
            // 
            // groupBox7
            // 
            groupBox7.Controls.Add(lblCurrentOperation);
            groupBox7.Controls.Add(lblCurrentDirectory);
            groupBox7.Controls.Add(lblCurrentFile);
            groupBox7.Location = new Point(15, 253);
            groupBox7.Name = "groupBox7";
            groupBox7.Size = new Size(466, 176);
            groupBox7.TabIndex = 32;
            groupBox7.TabStop = false;
            groupBox7.Text = "Current Activities";
            // 
            // lblCurrentOperation
            // 
            lblCurrentOperation.AutoSize = true;
            lblCurrentOperation.Location = new Point(6, 117);
            lblCurrentOperation.Name = "lblCurrentOperation";
            lblCurrentOperation.Size = new Size(150, 23);
            lblCurrentOperation.TabIndex = 34;
            lblCurrentOperation.Text = "Current Operation";
            // 
            // lblCurrentDirectory
            // 
            lblCurrentDirectory.AutoSize = true;
            lblCurrentDirectory.Location = new Point(6, 85);
            lblCurrentDirectory.Name = "lblCurrentDirectory";
            lblCurrentDirectory.Size = new Size(142, 23);
            lblCurrentDirectory.TabIndex = 32;
            lblCurrentDirectory.Text = "Current Directory";
            // 
            // lblCurrentFile
            // 
            lblCurrentFile.AutoSize = true;
            lblCurrentFile.Location = new Point(6, 53);
            lblCurrentFile.Name = "lblCurrentFile";
            lblCurrentFile.Size = new Size(95, 23);
            lblCurrentFile.TabIndex = 33;
            lblCurrentFile.Text = "Current file";
            // 
            // groupBox6
            // 
            groupBox6.Controls.Add(lblCommit);
            groupBox6.Controls.Add(lblFilesProcessed);
            groupBox6.Controls.Add(lblDiscoveredSize);
            groupBox6.Controls.Add(lblElapsedTime);
            groupBox6.Controls.Add(lblDirectoriesProcessed);
            groupBox6.Controls.Add(lblBytesHashed);
            groupBox6.Location = new Point(512, 35);
            groupBox6.Name = "groupBox6";
            groupBox6.Size = new Size(448, 294);
            groupBox6.TabIndex = 19;
            groupBox6.TabStop = false;
            groupBox6.Text = "Live Scanning Statistics";
            // 
            // lblCommit
            // 
            lblCommit.AutoSize = true;
            lblCommit.Location = new Point(18, 244);
            lblCommit.Name = "lblCommit";
            lblCommit.Size = new Size(83, 23);
            lblCommit.TabIndex = 31;
            lblCommit.Text = "Commits ";
            // 
            // lblFilesProcessed
            // 
            lblFilesProcessed.AutoSize = true;
            lblFilesProcessed.Location = new Point(18, 82);
            lblFilesProcessed.Name = "lblFilesProcessed";
            lblFilesProcessed.Size = new Size(116, 23);
            lblFilesProcessed.TabIndex = 30;
            lblFilesProcessed.Text = "File Processed";
            // 
            // lblDiscoveredSize
            // 
            lblDiscoveredSize.AutoSize = true;
            lblDiscoveredSize.Location = new Point(18, 40);
            lblDiscoveredSize.Name = "lblDiscoveredSize";
            lblDiscoveredSize.Size = new Size(126, 23);
            lblDiscoveredSize.TabIndex = 29;
            lblDiscoveredSize.Text = "Discovered size";
            // 
            // lblElapsedTime
            // 
            lblElapsedTime.AutoSize = true;
            lblElapsedTime.Location = new Point(18, 208);
            lblElapsedTime.Name = "lblElapsedTime";
            lblElapsedTime.Size = new Size(107, 23);
            lblElapsedTime.TabIndex = 28;
            lblElapsedTime.Text = "Elapsed time";
            // 
            // lblDirectoriesProcessed
            // 
            lblDirectoriesProcessed.AutoSize = true;
            lblDirectoriesProcessed.Location = new Point(18, 124);
            lblDirectoriesProcessed.Name = "lblDirectoriesProcessed";
            lblDirectoriesProcessed.Size = new Size(172, 23);
            lblDirectoriesProcessed.TabIndex = 26;
            lblDirectoriesProcessed.Text = "Directories processed";
            // 
            // lblBytesHashed
            // 
            lblBytesHashed.AutoSize = true;
            lblBytesHashed.Location = new Point(18, 166);
            lblBytesHashed.Name = "lblBytesHashed";
            lblBytesHashed.Size = new Size(110, 23);
            lblBytesHashed.TabIndex = 25;
            lblBytesHashed.Text = "Bytes hashed";
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(lblCurrentScanStatus);
            groupBox5.Controls.Add(lblStartTime);
            groupBox5.Controls.Add(lblArchiveProfile);
            groupBox5.Controls.Add(lblScanId);
            groupBox5.Controls.Add(label14);
            groupBox5.Location = new Point(15, 35);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new Size(466, 195);
            groupBox5.TabIndex = 18;
            groupBox5.TabStop = false;
            groupBox5.Text = "Current Scan Information";
            // 
            // lblCurrentScanStatus
            // 
            lblCurrentScanStatus.AutoSize = true;
            lblCurrentScanStatus.Location = new Point(10, 152);
            lblCurrentScanStatus.Name = "lblCurrentScanStatus";
            lblCurrentScanStatus.Size = new Size(131, 23);
            lblCurrentScanStatus.TabIndex = 31;
            lblCurrentScanStatus.Text = "Status (Scaning)";
            // 
            // lblStartTime
            // 
            lblStartTime.AutoSize = true;
            lblStartTime.Location = new Point(6, 115);
            lblStartTime.Name = "lblStartTime";
            lblStartTime.Size = new Size(135, 23);
            lblStartTime.TabIndex = 30;
            lblStartTime.Text = "Started at (GMT)";
            // 
            // lblArchiveProfile
            // 
            lblArchiveProfile.AutoSize = true;
            lblArchiveProfile.Location = new Point(6, 78);
            lblArchiveProfile.Name = "lblArchiveProfile";
            lblArchiveProfile.Size = new Size(119, 23);
            lblArchiveProfile.TabIndex = 24;
            lblArchiveProfile.Text = "Archive Profile";
            // 
            // lblScanId
            // 
            lblScanId.AutoSize = true;
            lblScanId.Location = new Point(6, 41);
            lblScanId.Name = "lblScanId";
            lblScanId.Size = new Size(68, 23);
            lblScanId.TabIndex = 0;
            lblScanId.Text = "Scan ID";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(65, 295);
            label14.Name = "label14";
            label14.Size = new Size(96, 23);
            label14.TabIndex = 22;
            label14.Text = "Error count";
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(8, 700);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(210, 50);
            btnCancel.TabIndex = 9;
            btnCancel.Text = "Cancel Scan";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(label7);
            tabPage3.Controls.Add(label5);
            tabPage3.Controls.Add(cmbChangeFilter);
            tabPage3.Controls.Add(dataGridView1);
            tabPage3.Controls.Add(btnExportHTML);
            tabPage3.Controls.Add(btnExportCSV);
            tabPage3.Controls.Add(btnCompare);
            tabPage3.Controls.Add(cmbCurrentScan);
            tabPage3.Controls.Add(cmbBaselineScan);
            tabPage3.Location = new Point(4, 34);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new Size(1174, 756);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Compare";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(391, 27);
            label7.Name = "label7";
            label7.Size = new Size(57, 23);
            label7.TabIndex = 15;
            label7.Text = "Target";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(19, 27);
            label5.Name = "label5";
            label5.Size = new Size(72, 23);
            label5.TabIndex = 14;
            label5.Text = "Baseline";
            // 
            // cmbChangeFilter
            // 
            cmbChangeFilter.FormattingEnabled = true;
            cmbChangeFilter.Location = new Point(136, 73);
            cmbChangeFilter.Name = "cmbChangeFilter";
            cmbChangeFilter.Size = new Size(151, 31);
            cmbChangeFilter.TabIndex = 13;
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(19, 129);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.Size = new Size(1067, 441);
            dataGridView1.TabIndex = 12;
            // 
            // btnExportHTML
            // 
            btnExportHTML.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnExportHTML.Location = new Point(679, 685);
            btnExportHTML.Name = "btnExportHTML";
            btnExportHTML.Size = new Size(210, 50);
            btnExportHTML.TabIndex = 11;
            btnExportHTML.Text = "Export &Html";
            btnExportHTML.UseVisualStyleBackColor = true;
            // 
            // btnExportCSV
            // 
            btnExportCSV.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnExportCSV.Location = new Point(349, 685);
            btnExportCSV.Name = "btnExportCSV";
            btnExportCSV.Size = new Size(210, 50);
            btnExportCSV.TabIndex = 10;
            btnExportCSV.Text = "Export &CSV";
            btnExportCSV.UseVisualStyleBackColor = true;
            // 
            // btnCompare
            // 
            btnCompare.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnCompare.Location = new Point(19, 685);
            btnCompare.Name = "btnCompare";
            btnCompare.Size = new Size(210, 50);
            btnCompare.TabIndex = 9;
            btnCompare.Text = "Com&pare";
            btnCompare.UseVisualStyleBackColor = true;
            // 
            // cmbCurrentScan
            // 
            cmbCurrentScan.FormattingEnabled = true;
            cmbCurrentScan.Location = new Point(493, 19);
            cmbCurrentScan.Name = "cmbCurrentScan";
            cmbCurrentScan.Size = new Size(151, 31);
            cmbCurrentScan.TabIndex = 1;
            // 
            // cmbBaselineScan
            // 
            cmbBaselineScan.FormattingEnabled = true;
            cmbBaselineScan.Location = new Point(136, 19);
            cmbBaselineScan.Name = "cmbBaselineScan";
            cmbBaselineScan.Size = new Size(151, 31);
            cmbBaselineScan.TabIndex = 0;
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(groupBox13);
            tabPage4.Controls.Add(groupBox12);
            tabPage4.Controls.Add(groupBox11);
            tabPage4.Controls.Add(groupBox9);
            tabPage4.Location = new Point(4, 34);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new Padding(3);
            tabPage4.Size = new Size(1174, 756);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "Advance";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // groupBox13
            // 
            groupBox13.Location = new Point(23, 448);
            groupBox13.Name = "groupBox13";
            groupBox13.Size = new Size(1014, 199);
            groupBox13.TabIndex = 7;
            groupBox13.TabStop = false;
            groupBox13.Text = "Insight";
            // 
            // groupBox12
            // 
            groupBox12.Controls.Add(groupBox10);
            groupBox12.Controls.Add(btnEmptyDirectories);
            groupBox12.Controls.Add(label6);
            groupBox12.Location = new Point(566, 193);
            groupBox12.Name = "groupBox12";
            groupBox12.Size = new Size(471, 229);
            groupBox12.TabIndex = 6;
            groupBox12.TabStop = false;
            groupBox12.Text = "Directory analysis";
            // 
            // groupBox10
            // 
            groupBox10.Controls.Add(lblDuplicateSpace);
            groupBox10.Controls.Add(lblDuplicateCount);
            groupBox10.Controls.Add(btnFindDuplicates);
            groupBox10.Location = new Point(61, 298);
            groupBox10.Name = "groupBox10";
            groupBox10.Size = new Size(334, 204);
            groupBox10.TabIndex = 8;
            groupBox10.TabStop = false;
            groupBox10.Text = "groupBox10";
            // 
            // lblDuplicateSpace
            // 
            lblDuplicateSpace.AutoSize = true;
            lblDuplicateSpace.Location = new Point(58, 120);
            lblDuplicateSpace.Name = "lblDuplicateSpace";
            lblDuplicateSpace.Size = new Size(145, 23);
            lblDuplicateSpace.TabIndex = 2;
            lblDuplicateSpace.Text = "lblDuplicateSpace";
            // 
            // lblDuplicateCount
            // 
            lblDuplicateCount.AutoSize = true;
            lblDuplicateCount.Location = new Point(58, 83);
            lblDuplicateCount.Name = "lblDuplicateCount";
            lblDuplicateCount.Size = new Size(147, 23);
            lblDuplicateCount.TabIndex = 1;
            lblDuplicateCount.Text = "lblDuplicateCount";
            // 
            // btnFindDuplicates
            // 
            btnFindDuplicates.Location = new Point(58, 41);
            btnFindDuplicates.Name = "btnFindDuplicates";
            btnFindDuplicates.Size = new Size(209, 29);
            btnFindDuplicates.TabIndex = 0;
            btnFindDuplicates.Text = "btnFindDuplicates";
            btnFindDuplicates.UseVisualStyleBackColor = true;
            // 
            // btnEmptyDirectories
            // 
            btnEmptyDirectories.Location = new Point(22, 43);
            btnEmptyDirectories.Name = "btnEmptyDirectories";
            btnEmptyDirectories.Size = new Size(210, 50);
            btnEmptyDirectories.TabIndex = 2;
            btnEmptyDirectories.Text = "btnEmptyDirectories";
            btnEmptyDirectories.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(22, 135);
            label6.Name = "label6";
            label6.Size = new Size(157, 23);
            label6.TabIndex = 1;
            label6.Text = "lblDirectoryAnalysis";
            // 
            // groupBox11
            // 
            groupBox11.Controls.Add(lblLargestFilesResult);
            groupBox11.Controls.Add(numLargestFiles);
            groupBox11.Controls.Add(btnLargestFiles);
            groupBox11.Location = new Point(23, 193);
            groupBox11.Name = "groupBox11";
            groupBox11.Size = new Size(484, 229);
            groupBox11.TabIndex = 5;
            groupBox11.TabStop = false;
            groupBox11.Text = "Large files (minimum size in GB)";
            // 
            // lblLargestFilesResult
            // 
            lblLargestFilesResult.AutoSize = true;
            lblLargestFilesResult.Location = new Point(15, 126);
            lblLargestFilesResult.Name = "lblLargestFilesResult";
            lblLargestFilesResult.Size = new Size(235, 23);
            lblLargestFilesResult.TabIndex = 8;
            lblLargestFilesResult.Text = "Large files count / total space";
            // 
            // numLargestFiles
            // 
            numLargestFiles.Location = new Point(280, 45);
            numLargestFiles.Name = "numLargestFiles";
            numLargestFiles.Size = new Size(124, 30);
            numLargestFiles.TabIndex = 7;
            // 
            // btnLargestFiles
            // 
            btnLargestFiles.Location = new Point(24, 43);
            btnLargestFiles.Name = "btnLargestFiles";
            btnLargestFiles.Size = new Size(210, 50);
            btnLargestFiles.TabIndex = 6;
            btnLargestFiles.Text = "btnLargestFiles";
            btnLargestFiles.UseVisualStyleBackColor = true;
            // 
            // groupBox9
            // 
            groupBox9.Controls.Add(lblVerificationResult);
            groupBox9.Controls.Add(lblLastVerification);
            groupBox9.Controls.Add(btnFullVerification);
            groupBox9.Location = new Point(19, 39);
            groupBox9.Name = "groupBox9";
            groupBox9.Size = new Size(1018, 133);
            groupBox9.TabIndex = 3;
            groupBox9.TabStop = false;
            groupBox9.Text = "Integrity verification";
            // 
            // lblVerificationResult
            // 
            lblVerificationResult.AutoSize = true;
            lblVerificationResult.Location = new Point(332, 77);
            lblVerificationResult.Name = "lblVerificationResult";
            lblVerificationResult.Size = new Size(342, 23);
            lblVerificationResult.TabIndex = 5;
            lblVerificationResult.Text = "lblVerificationResult (Result: All files verified)";
            // 
            // lblLastVerification
            // 
            lblLastVerification.AutoSize = true;
            lblLastVerification.Location = new Point(332, 29);
            lblLastVerification.Name = "lblLastVerification";
            lblLastVerification.Size = new Size(345, 23);
            lblLastVerification.TabIndex = 4;
            lblLastVerification.Text = "Select a completed base line for verification.";
            // 
            // btnFullVerification
            // 
            btnFullVerification.Location = new Point(28, 29);
            btnFullVerification.Name = "btnFullVerification";
            btnFullVerification.Size = new Size(210, 50);
            btnFullVerification.TabIndex = 3;
            btnFullVerification.Text = "Full Version &Verification";
            btnFullVerification.UseVisualStyleBackColor = true;
            btnFullVerification.Click += btnFullVerification_Click;
            // 
            // tabPage5
            // 
            tabPage5.Controls.Add(groupBox2);
            tabPage5.Controls.Add(groupBox3);
            tabPage5.Controls.Add(groupBox1);
            tabPage5.Location = new Point(4, 34);
            tabPage5.Name = "tabPage5";
            tabPage5.Padding = new Padding(3);
            tabPage5.Size = new Size(1174, 756);
            tabPage5.TabIndex = 4;
            tabPage5.Text = "Setting";
            tabPage5.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(lblConnectionTestResult);
            groupBox2.Controls.Add(lblPassword);
            groupBox2.Controls.Add(txtPassword);
            groupBox2.Controls.Add(lblUsername);
            groupBox2.Controls.Add(txtUsername);
            groupBox2.Controls.Add(btnTestConnection);
            groupBox2.Controls.Add(cmbDatabaseName);
            groupBox2.Controls.Add(lblDatabaseName);
            groupBox2.Controls.Add(lblServerAddress);
            groupBox2.Controls.Add(txtServerAddress);
            groupBox2.Location = new Point(6, 6);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(609, 295);
            groupBox2.TabIndex = 0;
            groupBox2.TabStop = false;
            groupBox2.Text = "Data Base ";
            // 
            // lblConnectionTestResult
            // 
            lblConnectionTestResult.AutoSize = true;
            lblConnectionTestResult.Location = new Point(345, 238);
            lblConnectionTestResult.Name = "lblConnectionTestResult";
            lblConnectionTestResult.Size = new Size(191, 23);
            lblConnectionTestResult.TabIndex = 9;
            lblConnectionTestResult.Text = "lblConnectionTestResult";
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(345, 114);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(98, 23);
            lblPassword.TabIndex = 8;
            lblPassword.Text = "lblPassword";
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(345, 151);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(191, 30);
            txtPassword.TabIndex = 7;
            txtPassword.Text = "DB Password";
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(345, 33);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(105, 23);
            lblUsername.TabIndex = 6;
            lblUsername.Text = "lblUsername";
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(345, 70);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(191, 30);
            txtUsername.TabIndex = 5;
            txtUsername.Text = "DB User Name";
            // 
            // btnTestConnection
            // 
            btnTestConnection.Location = new Point(41, 223);
            btnTestConnection.Name = "btnTestConnection";
            btnTestConnection.Size = new Size(210, 50);
            btnTestConnection.TabIndex = 4;
            btnTestConnection.Text = "Test &Connection";
            btnTestConnection.UseVisualStyleBackColor = true;
            // 
            // cmbDatabaseName
            // 
            cmbDatabaseName.FormattingEnabled = true;
            cmbDatabaseName.Location = new Point(41, 150);
            cmbDatabaseName.Name = "cmbDatabaseName";
            cmbDatabaseName.Size = new Size(151, 31);
            cmbDatabaseName.TabIndex = 3;
            // 
            // lblDatabaseName
            // 
            lblDatabaseName.AutoSize = true;
            lblDatabaseName.Location = new Point(41, 114);
            lblDatabaseName.Name = "lblDatabaseName";
            lblDatabaseName.Size = new Size(145, 23);
            lblDatabaseName.TabIndex = 2;
            lblDatabaseName.Text = "lblDatabaseName";
            // 
            // lblServerAddress
            // 
            lblServerAddress.AutoSize = true;
            lblServerAddress.Location = new Point(41, 33);
            lblServerAddress.Name = "lblServerAddress";
            lblServerAddress.Size = new Size(135, 23);
            lblServerAddress.TabIndex = 1;
            lblServerAddress.Text = "lblServerAddress";
            // 
            // txtServerAddress
            // 
            txtServerAddress.Location = new Point(41, 70);
            txtServerAddress.Name = "txtServerAddress";
            txtServerAddress.Size = new Size(222, 30);
            txtServerAddress.TabIndex = 0;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(btnActivateProfile);
            groupBox3.Controls.Add(btnRemoveProfile);
            groupBox3.Controls.Add(btnEditProfile);
            groupBox3.Controls.Add(btnAddProfile);
            groupBox3.Controls.Add(dataGridView2);
            groupBox3.Location = new Point(6, 320);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(609, 333);
            groupBox3.TabIndex = 0;
            groupBox3.TabStop = false;
            groupBox3.Text = "Archive Profiles";
            // 
            // btnActivateProfile
            // 
            btnActivateProfile.Location = new Point(447, 267);
            btnActivateProfile.Name = "btnActivateProfile";
            btnActivateProfile.Size = new Size(130, 50);
            btnActivateProfile.TabIndex = 4;
            btnActivateProfile.Text = "button1";
            btnActivateProfile.UseVisualStyleBackColor = true;
            // 
            // btnRemoveProfile
            // 
            btnRemoveProfile.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnRemoveProfile.Location = new Point(300, 267);
            btnRemoveProfile.Name = "btnRemoveProfile";
            btnRemoveProfile.Size = new Size(130, 50);
            btnRemoveProfile.TabIndex = 3;
            btnRemoveProfile.Text = "btnRemoveProfile";
            btnRemoveProfile.UseVisualStyleBackColor = true;
            // 
            // btnEditProfile
            // 
            btnEditProfile.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnEditProfile.Location = new Point(153, 267);
            btnEditProfile.Name = "btnEditProfile";
            btnEditProfile.Size = new Size(130, 50);
            btnEditProfile.TabIndex = 2;
            btnEditProfile.Text = "btnEditProfile";
            btnEditProfile.UseVisualStyleBackColor = true;
            // 
            // btnAddProfile
            // 
            btnAddProfile.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnAddProfile.Location = new Point(6, 267);
            btnAddProfile.Name = "btnAddProfile";
            btnAddProfile.Size = new Size(130, 50);
            btnAddProfile.TabIndex = 1;
            btnAddProfile.Text = "btnAddProfile";
            btnAddProfile.UseVisualStyleBackColor = true;
            // 
            // dataGridView2
            // 
            dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView2.Location = new Point(3, 23);
            dataGridView2.Name = "dataGridView2";
            dataGridView2.RowHeadersWidth = 51;
            dataGridView2.Size = new Size(600, 188);
            dataGridView2.TabIndex = 0;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(btnRestoreConfiguration);
            groupBox1.Controls.Add(btnBackupConfiguration);
            groupBox1.Controls.Add(btnCheckDatabase);
            groupBox1.Controls.Add(btnDatabaseStatistics);
            groupBox1.Controls.Add(chkUseGMT);
            groupBox1.Controls.Add(btnBrowseExportDirectory);
            groupBox1.Controls.Add(txtExportDirectory);
            groupBox1.Controls.Add(lblExportDirectory);
            groupBox1.Location = new Point(635, 6);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(451, 647);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Application Settings";
            // 
            // btnRestoreConfiguration
            // 
            btnRestoreConfiguration.Anchor = AnchorStyles.Top;
            btnRestoreConfiguration.Location = new Point(51, 424);
            btnRestoreConfiguration.Name = "btnRestoreConfiguration";
            btnRestoreConfiguration.Size = new Size(210, 50);
            btnRestoreConfiguration.TabIndex = 21;
            btnRestoreConfiguration.Text = "btnRestoreConfiguration";
            btnRestoreConfiguration.UseVisualStyleBackColor = true;
            // 
            // btnBackupConfiguration
            // 
            btnBackupConfiguration.Anchor = AnchorStyles.Top;
            btnBackupConfiguration.Location = new Point(51, 357);
            btnBackupConfiguration.Name = "btnBackupConfiguration";
            btnBackupConfiguration.Size = new Size(210, 50);
            btnBackupConfiguration.TabIndex = 20;
            btnBackupConfiguration.Text = "btnBackupConfiguration";
            btnBackupConfiguration.UseVisualStyleBackColor = true;
            // 
            // btnCheckDatabase
            // 
            btnCheckDatabase.Anchor = AnchorStyles.Top;
            btnCheckDatabase.Location = new Point(51, 290);
            btnCheckDatabase.Name = "btnCheckDatabase";
            btnCheckDatabase.Size = new Size(210, 50);
            btnCheckDatabase.TabIndex = 19;
            btnCheckDatabase.Text = "btnCheckDatabase";
            btnCheckDatabase.UseVisualStyleBackColor = true;
            // 
            // btnDatabaseStatistics
            // 
            btnDatabaseStatistics.Anchor = AnchorStyles.Top;
            btnDatabaseStatistics.Location = new Point(51, 223);
            btnDatabaseStatistics.Name = "btnDatabaseStatistics";
            btnDatabaseStatistics.Size = new Size(210, 50);
            btnDatabaseStatistics.TabIndex = 18;
            btnDatabaseStatistics.Text = "btnDatabaseStatistics";
            btnDatabaseStatistics.UseVisualStyleBackColor = true;
            // 
            // chkUseGMT
            // 
            chkUseGMT.AutoSize = true;
            chkUseGMT.Checked = true;
            chkUseGMT.CheckState = CheckState.Checked;
            chkUseGMT.Location = new Point(51, 498);
            chkUseGMT.Name = "chkUseGMT";
            chkUseGMT.Size = new Size(239, 27);
            chkUseGMT.TabIndex = 5;
            chkUseGMT.Text = "Display timestamps in GMT";
            chkUseGMT.UseVisualStyleBackColor = true;
            // 
            // btnBrowseExportDirectory
            // 
            btnBrowseExportDirectory.Anchor = AnchorStyles.Top;
            btnBrowseExportDirectory.Location = new Point(51, 156);
            btnBrowseExportDirectory.Name = "btnBrowseExportDirectory";
            btnBrowseExportDirectory.Size = new Size(210, 50);
            btnBrowseExportDirectory.TabIndex = 4;
            btnBrowseExportDirectory.Text = "Browse Export Directory";
            btnBrowseExportDirectory.UseVisualStyleBackColor = true;
            // 
            // txtExportDirectory
            // 
            txtExportDirectory.Location = new Point(51, 36);
            txtExportDirectory.Name = "txtExportDirectory";
            txtExportDirectory.Size = new Size(239, 30);
            txtExportDirectory.TabIndex = 3;
            txtExportDirectory.Text = "txtExportDirectory";
            // 
            // lblExportDirectory
            // 
            lblExportDirectory.AutoSize = true;
            lblExportDirectory.Location = new Point(51, 79);
            lblExportDirectory.Name = "lblExportDirectory";
            lblExportDirectory.Size = new Size(146, 23);
            lblExportDirectory.TabIndex = 0;
            lblExportDirectory.Text = "lblExportDirectory";
            // 
            // tabPage6
            // 
            tabPage6.Controls.Add(pictureBox1);
            tabPage6.Controls.Add(groupBox14);
            tabPage6.Controls.Add(label1);
            tabPage6.Location = new Point(4, 34);
            tabPage6.Name = "tabPage6";
            tabPage6.Padding = new Padding(3);
            tabPage6.Size = new Size(1174, 756);
            tabPage6.TabIndex = 5;
            tabPage6.Text = "About";
            tabPage6.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            pictureBox1.ErrorImage = null;
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.InitialImage = null;
            pictureBox1.Location = new Point(819, 120);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(250, 250);
            pictureBox1.TabIndex = 6;
            pictureBox1.TabStop = false;
            // 
            // groupBox14
            // 
            groupBox14.Controls.Add(textBox1);
            groupBox14.Location = new Point(79, 74);
            groupBox14.Name = "groupBox14";
            groupBox14.Size = new Size(710, 394);
            groupBox14.TabIndex = 5;
            groupBox14.TabStop = false;
            groupBox14.Text = "About";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(31, 46);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(644, 312);
            textBox1.TabIndex = 0;
            textBox1.Text = resources.GetString("textBox1.Text");
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label1.Location = new Point(79, 22);
            label1.Name = "label1";
            label1.Size = new Size(372, 20);
            label1.TabIndex = 4;
            label1.Text = "Archive Inventory and Integrity Monitoring System.";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1182, 853);
            Controls.Add(tabControl1);
            Controls.Add(statusStrip1);
            Controls.Add(menuStrip1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            MaximizeBox = false;
            Name = "Form1";
            Text = "Form1 IMS";
            Load += Form1_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            groupBox15.ResumeLayout(false);
            groupBox15.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvScanQueue).EndInit();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvScanHistory).EndInit();
            tabPage2.ResumeLayout(false);
            groupBox8.ResumeLayout(false);
            groupBox8.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView2_errors).EndInit();
            groupBox7.ResumeLayout(false);
            groupBox7.PerformLayout();
            groupBox6.ResumeLayout(false);
            groupBox6.PerformLayout();
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            tabPage4.ResumeLayout(false);
            groupBox12.ResumeLayout(false);
            groupBox12.PerformLayout();
            groupBox10.ResumeLayout(false);
            groupBox10.PerformLayout();
            groupBox11.ResumeLayout(false);
            groupBox11.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numLargestFiles).EndInit();
            groupBox9.ResumeLayout(false);
            groupBox9.PerformLayout();
            tabPage5.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            tabPage6.ResumeLayout(false);
            tabPage6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            groupBox14.ResumeLayout(false);
            groupBox14.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private StatusStrip statusStrip1;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TabPage tabPage3;
        private TabPage tabPage4;
        private TabPage tabPage5;
        private TabPage tabPage6;
        private ToolStripMenuItem filesToolStripMenuItem;
        private ToolStripMenuItem profilesToolStripMenuItem;
        private ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem newScanToolStripMenuItem;
        private ToolStripMenuItem exportToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem selectProfileToolStripMenuItem;
        private ToolStripMenuItem addProfileToolStripMenuItem;
        private ToolStripMenuItem editProfileToolStripMenuItem;
        private ToolStripMenuItem compareScansStripMenuItem;
        private ToolStripMenuItem toolStripMenuAdvanced;
        private ToolStripMenuItem databaseStatusToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ToolStripMenuItem versionsToolStripMenuItem;
        private ToolStripMenuItem documentsToolStripMenuItem;
        private Button BtnGoCompare;
        private Button btnScan;
        private TextBox txtRootPath;
        private ComboBox cmbArchiveProfile;
        private Label label1;
        private Label lblTotalFiles;
        private Label lblTotalDirectories;
        private Label lblTotalSize;
        private Button btnExit;
        private DataGridView dgvScanHistory;
        private Button btnCancel;
        private Button btnCompare;
        private ComboBox cmbCurrentScan;
        private ComboBox cmbBaselineScan;
        private Button btnExportHTML;
        private Button btnExportCSV;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private GroupBox groupBox1;
        private ToolStripStatusLabel lblGMTClock;
        private ToolStripProgressBar toolStripProgressBar;
        private ToolStripStatusLabel toolStripStatusLabel2;
        private Label label2;
        private Label label3;
        private Label label4;
        private GroupBox groupBox4;
        private Label lblLastScanStatus;
        private Label lblLastDuration;
        private Label lblLastComparison;
        private Label lblLastScan;
        private GroupBox groupBox5;
        private Label label14;
        private GroupBox groupBox6;
        private Label lblArchiveProfile;
        private Label lblScanId;
        private Label lblDiscoveredSize;
        private Label lblElapsedTime;
        private Label lblDirectoriesProcessed;
        private Label lblBytesHashed;
        private Label lblCurrentScanStatus;
        private Label lblStartTime;
        private Label lblFilesProcessed;
        private GroupBox groupBox8;
        private DataGridView dataGridView2_errors;
        private GroupBox groupBox7;
        private Label lblCurrentOperation;
        private Label lblCurrentDirectory;
        private Label lblCurrentFile;
        private DataGridView dataGridView1;
        private ComboBox cmbChangeFilter;
        private GroupBox groupBox9;
        private Label lblVerificationResult;
        private Label lblLastVerification;
        private Button btnFullVerification;
        private GroupBox groupBox11;
        private Button btnLargestFiles;
        private Label lblLargestFilesResult;
        private NumericUpDown numLargestFiles;
        private GroupBox groupBox12;
        private Button btnEmptyDirectories;
        private Label label6;
        private GroupBox groupBox13;
        private GroupBox groupBox10;
        private Label lblDuplicateSpace;
        private Label lblDuplicateCount;
        private Button btnFindDuplicates;
        private Label lblServerAddress;
        private TextBox txtServerAddress;
        private ComboBox cmbDatabaseName;
        private Label lblDatabaseName;
        private Label lblUsername;
        private TextBox txtUsername;
        private Button btnTestConnection;
        private Label lblPassword;
        private TextBox txtPassword;
        private Label lblConnectionTestResult;
        private Button btnAddProfile;
        private DataGridView dataGridView2;
        private Button btnRemoveProfile;
        private Button btnEditProfile;
        private CheckBox chkUseGMT;
        private Button btnBrowseExportDirectory;
        private TextBox txtExportDirectory;
        private Label lblExportDirectory;
        private GroupBox groupBox14;
        private TextBox textBox1;
        private ToolStripStatusLabel lblDatabaseStatus;
        private ToolStripStatusLabel lblOperationStatus;
        private Label lblErrReport;
        private Button btnRestoreConfiguration;
        private Button btnBackupConfiguration;
        private Button btnCheckDatabase;
        private Button btnDatabaseStatistics;
        private Button btnActivateProfile;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private Label lblCommit;
        private Button btnBackgroun;
        private ToolStripMenuItem activateProfileToolStripMenuItem;
        private Label label7;
        private Label label5;
        private PictureBox pictureBox1;
        private GroupBox groupBox15;
        private DataGridView dgvScanQueue;
        private Button btnCancelQueuedScan;
        private Button btnAddToQueue;
        private Label lblScheduledStart;
        private DateTimePicker dtpScheduledStart;
        private Button btnPauseQueue;
    }
}
