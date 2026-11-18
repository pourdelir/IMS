using IMS.Configuration;
using IMS.Data;
using AppConfig = IMS.Configuration.ApplicationConfiguration;

namespace IMS;

internal sealed class FirstRunSetupForm : Form
{
    private readonly TextBox _server = new() { Text = @"(localdb)\MSSQLLocalDB", Width = 270 };
    private readonly ComboBox _database = new() { Text = "ArchiveAuditor", Width = 270, DropDownStyle = ComboBoxStyle.DropDown };
    private readonly TextBox _username = new() { Width = 270 };
    private readonly TextBox _password = new() { Width = 270, UseSystemPasswordChar = true };
    private readonly CheckBox _integrated = new() { Text = "Use Windows authentication", Checked = false, AutoSize = true };
    private readonly TextBox _profile = new() { Text = "My Archive", Width = 270 };
    private readonly TextBox _root = new() { Width = 270 };
    private readonly Label _result = new() { AutoSize = true, MaximumSize = new Size(420, 0) };
    private readonly Button _continue = new() { Text = "Initialize and Continue", AutoSize = true, Enabled = false };

    public AppConfig? Configuration { get; private set; }

    public FirstRunSetupForm()
    {
        Text = "Archive Auditor First Run Setup";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        ClientSize = new Size(550, 550);

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 2, RowCount = 10, AutoSize = false };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddRow(layout, 0, "SQL Server", _server);
        AddRow(layout, 1, "Database", _database);
        AddRow(layout, 2, "", _integrated);
        AddRow(layout, 3, "SQL username", _username);
        AddRow(layout, 4, "SQL password", _password);
        AddRow(layout, 5, "Profile name", _profile);
        var rootRow = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
        rootRow.Controls.Add(_root);
        var browse = new Button { Text = "Browse…", AutoSize = true };
        browse.Click += (_, _) =>
        {
            using var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog(this) == DialogResult.OK) _root.Text = dialog.SelectedPath;
        };
        rootRow.Controls.Add(browse);
        AddRow(layout, 6, "Archive root", rootRow);
        var test = new Button { Text = "Test SQL Connection", AutoSize = true };
        test.Click += async (_, _) => await TestConnectionAsync();
        AddRow(layout, 7, "", test);
        AddRow(layout, 8, "", _result);
        _continue.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_profile.Text) || !Directory.Exists(_root.Text))
            {
                _result.Text = "Enter a profile name and select an existing archive directory.";
                return;
            }
            if (IsApplicationInsideArchive(_root.Text))
            {
                _result.Text = "The application directory must be outside the monitored archive because settings.json is stored beside the executable.";
                return;
            }
            Configuration = BuildConfiguration();
            DialogResult = DialogResult.OK;
            Close();
        };
        AddRow(layout, 9, "", _continue);
        _integrated.CheckedChanged += (_, _) =>
        {
            _username.Enabled = _password.Enabled = !_integrated.Checked;
            _continue.Enabled = false;
        };
        _server.TextChanged += (_, _) => _continue.Enabled = false;
        _database.TextChanged += (_, _) => _continue.Enabled = false;
        _username.TextChanged += (_, _) => _continue.Enabled = false;
        _password.TextChanged += (_, _) => _continue.Enabled = false;
        Controls.Add(layout);
    }

    private static void AddRow(TableLayoutPanel layout, int row, string caption, Control control)
    {
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(new Label { Text = caption, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
        layout.Controls.Add(control, 1, row);
    }

    private AppConfig BuildConfiguration()
    {
        var config = new AppConfig();
        config.Database.Server = _server.Text.Trim();
        config.Database.DatabaseName = _database.Text.Trim();
        config.Database.UseIntegratedSecurity = _integrated.Checked;
        config.Database.UserName = _username.Text.Trim();
        config.Database.Password = _password.Text;
        config.Profiles.Add(new ArchiveProfileConfiguration { ProfileName = _profile.Text.Trim(), RootPath = Path.GetFullPath(_root.Text) });
        return config;
    }

    private static bool IsApplicationInsideArchive(string root)
    {
        var relative = Path.GetRelativePath(Path.GetFullPath(root), Path.GetFullPath(AppContext.BaseDirectory));
        return relative == "." || (!Path.IsPathRooted(relative) && relative != ".." && !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal));
    }

    private void InitializeComponent()
    {

    }

    private async Task TestConnectionAsync()
    {
        _continue.Enabled = false;
        _result.Text = "Connecting…";
        try
        {
            var admin = new SqlServerAdministrationService(BuildConfiguration());
            await admin.TestConnectionAsync();
            var selected = _database.Text;
            try
            {
                var names = await admin.GetAvailableDatabasesAsync();
                _database.Items.Clear();
                _database.Items.AddRange(names.Cast<object>().ToArray());
                _database.Text = selected;
            }
            catch { /* Database discovery is optional when server permissions hide the catalog. */ }
            _result.Text = "Connection succeeded. You can initialize the database now.";
            _continue.Enabled = true;
        }
        catch (Exception exception)
        {
            _result.Text = $"Connection failed: {exception.Message}";
        }
    }
}
