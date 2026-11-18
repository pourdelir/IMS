using IMS.Domain;

namespace IMS;

internal sealed class ScanPickerDialog : Form
{
    private readonly ComboBox _scans = new() { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };
    public ScanSnapshot? SelectedScan => _scans.SelectedItem as ScanSnapshot;

    public ScanPickerDialog(IReadOnlyList<ScanSnapshot> scans)
    {
        Text = "Select completed baseline";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        ClientSize = new Size(440, 115);
        _scans.DataSource = scans.ToList();
        _scans.DisplayMember = nameof(ScanSnapshot.ScanId);
        var accept = new Button { Text = "Select", DialogResult = DialogResult.OK, Anchor = AnchorStyles.Right | AnchorStyles.Bottom };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Anchor = AnchorStyles.Right | AnchorStyles.Bottom };
        accept.Location = new Point(250, 65);
        cancel.Location = new Point(340, 65);
        Controls.Add(_scans);
        Controls.Add(accept);
        Controls.Add(cancel);
        AcceptButton = accept;
        CancelButton = cancel;
    }
}
