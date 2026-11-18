using IMS.Domain;

namespace IMS;

internal sealed class ProfilePickerDialog : Form
{
    private readonly ComboBox _profiles = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _rootPath = new() { AutoEllipsis = true, AutoSize = false };

    public ArchiveProfile? SelectedProfile => _profiles.SelectedItem as ArchiveProfile;

    public ProfilePickerDialog(IReadOnlyList<ArchiveProfile> profiles)
    {
        Text = "Activate archive profile";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = false;
        MaximizeBox = false;
        ClientSize = new Size(580, 166);

        var prompt = new Label { AutoSize = true, Location = new Point(12, 12), Text = "Select a deactivated profile:" };
        _profiles.Location = new Point(12, 39);
        _profiles.Size = new Size(556, 31);
        _profiles.DataSource = profiles.ToList();
        _profiles.DisplayMember = nameof(ArchiveProfile.ProfileName);
        _profiles.SelectedIndexChanged += (_, _) => UpdateRootPath();

        _rootPath.Location = new Point(12, 76);
        _rootPath.Size = new Size(556, 30);

        var activate = new Button { Text = "Activate", DialogResult = DialogResult.OK, Location = new Point(386, 118), Size = new Size(86, 32) };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(482, 118), Size = new Size(86, 32) };

        Controls.AddRange([prompt, _profiles, _rootPath, activate, cancel]);
        AcceptButton = activate;
        CancelButton = cancel;
        UpdateRootPath();
    }

    private void UpdateRootPath() => _rootPath.Text = SelectedProfile?.RootPath ?? string.Empty;
}
