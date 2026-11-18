using IMS.Domain;

namespace IMS;

internal sealed class ArchiveGrowthChartControl : Control
{
    private IReadOnlyList<ScanSnapshot> _scans = [];

    public ArchiveGrowthChartControl()
    {
        DoubleBuffered = true;
        BackColor = Color.White;
    }

    public void SetScans(IReadOnlyList<ScanSnapshot> scans)
    {
        _scans = scans.OrderBy(x => x.StartedAtUtc).ToArray();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var area = Rectangle.Inflate(ClientRectangle, -35, -20);
        e.Graphics.DrawLine(Pens.SlateGray, area.Left, area.Bottom, area.Right, area.Bottom);
        e.Graphics.DrawLine(Pens.SlateGray, area.Left, area.Top, area.Left, area.Bottom);
        if (_scans.Count == 0)
        {
            TextRenderer.DrawText(e.Graphics, "No completed scan history", Font, area, Color.DimGray,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            return;
        }
        var max = Math.Max(1, _scans.Max(s => s.TotalSizeBytes));
        var points = _scans.Select((scan, index) => new PointF(
            area.Left + (float)(index * area.Width / (double)Math.Max(1, _scans.Count - 1)),
            area.Bottom - (float)(scan.TotalSizeBytes * area.Height / (double)max))).ToArray();
        if (points.Length == 1) e.Graphics.FillEllipse(Brushes.RoyalBlue, points[0].X - 4, points[0].Y - 4, 8, 8);
        else { using var sizePen = new Pen(Color.RoyalBlue, 3); e.Graphics.DrawLines(sizePen, points); }
        var maxFiles = Math.Max(1, _scans.Max(s => s.TotalFiles));
        var filePoints = _scans.Select((scan, index) => new PointF(
            area.Left + (float)(index * area.Width / (double)Math.Max(1, _scans.Count - 1)),
            area.Bottom - (float)(scan.TotalFiles * area.Height / (double)maxFiles))).ToArray();
        if (filePoints.Length == 1) e.Graphics.FillEllipse(Brushes.SeaGreen, filePoints[0].X - 4, filePoints[0].Y - 4, 8, 8);
        else { using var filesPen = new Pen(Color.SeaGreen, 2); e.Graphics.DrawLines(filesPen, filePoints); }
        TextRenderer.DrawText(e.Graphics, "Archive size (blue) · file count (green)", Font, new Point(area.Left, area.Top - 2), ForeColor);
        TextRenderer.DrawText(e.Graphics, $"{_scans.Count} snapshots · latest {_scans[^1].TotalFiles:N0} files · {FormatSize(_scans[^1].TotalSizeBytes)}",
            Font, new Point(area.Left, area.Bottom + 2), ForeColor);
    }

    private static string FormatSize(long bytes) => bytes >= 1L << 30 ? $"{bytes / (double)(1L << 30):0.##} GB" : $"{bytes / (double)(1L << 20):0.##} MB";
}
