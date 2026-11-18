using System.Net;
using System.Text;
using IMS.Domain;

namespace IMS.Services;

public sealed class ReportExporter
{
    public async Task ExportCsvAsync(string path, ComparisonResult result, CancellationToken cancellationToken = default)
    {
        await using var writer = new StreamWriter(path, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        await writer.WriteLineAsync("RecordType,RelativePath,ChangeType,PreviousSize,CurrentSize,PreviousSHA256,CurrentSHA256");
        foreach (var row in result.Changes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var values = new[] { row.RecordType, row.RelativePath, row.ChangeType,
                row.PreviousSize?.ToString() ?? "", row.CurrentSize?.ToString() ?? "", row.PreviousHash ?? "", row.CurrentHash ?? "" };
            await writer.WriteLineAsync(string.Join(',', values.Select(Csv)));
        }
    }

    public async Task ExportHtmlAsync(string path, string profileName, ComparisonResult result,
        IReadOnlyList<ScanSnapshot> history, CancellationToken cancellationToken = default)
    {
        var rows = new StringBuilder();
        foreach (var row in result.Changes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            rows.Append("<tr><td>").Append(E(row.RecordType)).Append("</td><td><code>").Append(E(row.RelativePath))
                .Append("</code></td><td><span class=\"tag ").Append(E(row.ChangeType.ToLowerInvariant())).Append("\">")
                .Append(E(row.ChangeType)).Append("</span></td><td>").Append(row.PreviousSize?.ToString("N0") ?? "—")
                .Append("</td><td>").Append(row.CurrentSize?.ToString("N0") ?? "—").Append("</td><td><code>")
                .Append(E(row.PreviousHash ?? "—")).Append("</code></td><td><code>").Append(E(row.CurrentHash ?? "—"))
                .Append("</code></td></tr>");
        }
        var html = new StringBuilder();
        var version = typeof(ReportExporter).Assembly.GetName().Version?.ToString() ?? "1.0.0";
        html.AppendLine("<!doctype html><html lang=\"en\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"><title>Archive Auditor comparison</title>")
            .AppendLine("<style>body{font:15px/1.5 system-ui,sans-serif;max-width:1200px;margin:32px auto;padding:0 20px;color:#202938;background:#f6f8fb}h1,h2{color:#15365b}.card{background:white;border:1px solid #dce3ec;border-radius:10px;padding:20px;margin:18px 0}.stats{display:grid;grid-template-columns:repeat(auto-fit,minmax(150px,1fr));gap:12px}.stat{background:#f1f5f9;padding:14px;border-radius:8px}table{width:100%;border-collapse:collapse;background:white}th,td{text-align:left;padding:9px;border-bottom:1px solid #e2e8f0;vertical-align:top}th{background:#edf2f7;position:sticky;top:0}code{overflow-wrap:anywhere}.tag{padding:2px 8px;border-radius:12px;background:#e6edf5}.added{background:#dcfce7}.deleted{background:#fee2e2}.modified{background:#fef3c7}.table-wrap{overflow:auto;max-height:650px}svg{width:100%;height:260px}footer{color:#64748b;margin:24px 0}</style></head><body>")
            .AppendLine("<h1>Archive Auditor — comparison report</h1>")
            .Append("<p>Generated ").Append(result.ComparedAtUtc.ToString("MM-dd-MM-yyyy HH:mm:ss")).Append(" UTC · Application version ").Append(E(version)).AppendLine("</p>")
            .Append("<section class=\"card\"><h2>Archive</h2><p><b>Profile:</b> ").Append(E(profileName)).Append("<br><b>Root:</b> <code>").Append(E(result.Current.RootPath)).AppendLine("</code></p>")
            .Append("<p>Previous scan #").Append(result.Previous.ScanId).Append(" (").Append(result.Previous.StartedAtUtc.ToString("MM-dd-MM-yyyy HH:mm")).Append(" UTC) · Current scan #").Append(result.Current.ScanId).Append(" (").Append(result.Current.StartedAtUtc.ToString("MM-dd-MM-yyyy HH:mm")).AppendLine(" UTC)</p></section>")
            .AppendLine("<section class=\"card\"><h2>Summary</h2><div class=\"stats\">");
        AppendStat(html, "Added files", result.Changes.LongCount(x => x.RecordType == "File" && x.ChangeType == "Added"));
        AppendStat(html, "Deleted files", result.Changes.LongCount(x => x.RecordType == "File" && x.ChangeType == "Deleted"));
        AppendStat(html, "Modified files", result.Changes.LongCount(x => x.RecordType == "File" && x.ChangeType == "Modified"));
        AppendStat(html, "Added directories", result.Changes.LongCount(x => x.RecordType == "Directory" && x.ChangeType == "Added"));
        AppendStat(html, "Deleted directories", result.Changes.LongCount(x => x.RecordType == "Directory" && x.ChangeType == "Deleted"));
        AppendStat(html, "Unchanged files", result.UnchangedFiles);
        html.Append("<div class=\"stat\">Size difference: <b>").Append(result.SizeDifferenceBytes.ToString("+#,##0;-#,##0;0")).AppendLine(" bytes</b></div></div></section>");
        if (history.Count > 1) html.Append("<section class=\"card\"><h2>Historical archive growth</h2>").Append(BuildGrowthSvg(history)).AppendLine("</section>");
        html.AppendLine("<section class=\"card\"><h2>Changes</h2><div class=\"table-wrap\"><table><thead><tr><th>Type</th><th>Relative path</th><th>Change</th><th>Previous size</th><th>Current size</th><th>Previous SHA-256</th><th>Current SHA-256</th></tr></thead><tbody>")
            .Append(rows).AppendLine("</tbody></table></div></section><footer>Unchanged file listings are omitted. Hashes are SHA-256.</footer></body></html>");
        await File.WriteAllTextAsync(path, html.ToString(), new UTF8Encoding(false), cancellationToken);
    }

    private static string Csv(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";
    private static string E(string value) => WebUtility.HtmlEncode(value);

    private static void AppendStat(StringBuilder html, string title, long value) =>
        html.Append("<div class=\"stat\">").Append(E(title)).Append(": <b>").Append(value.ToString("N0")).AppendLine("</b></div>");

    private static string BuildGrowthSvg(IReadOnlyList<ScanSnapshot> scans)
    {
        if (scans.Count < 2) return string.Empty;
        var points = scans.OrderBy(s => s.StartedAtUtc).ToArray();
        var max = Math.Max(1, points.Max(p => p.TotalSizeBytes));
        var poly = string.Join(' ', points.Select((p, i) => $"{40 + (i * 900d / (points.Length - 1)):0},{220 - (p.TotalSizeBytes * 190d / max):0}"));
        var maxFiles = Math.Max(1, points.Max(p => p.TotalFiles));
        var filePoly = string.Join(' ', points.Select((p, i) => $"{40 + (i * 900d / (points.Length - 1)):0},{220 - (p.TotalFiles * 190d / maxFiles):0}"));
        return $"<svg viewBox=\"0 0 980 260\" role=\"img\" aria-label=\"Archive size and file count over successful scans\"><line x1=\"40\" y1=\"220\" x2=\"940\" y2=\"220\" stroke=\"#94a3b8\"/><polyline fill=\"none\" stroke=\"#2563eb\" stroke-width=\"4\" points=\"{poly}\"/><polyline fill=\"none\" stroke=\"#16803c\" stroke-width=\"3\" points=\"{filePoly}\"/></svg><p>Blue: archive size · green: file count · {points.Length} successful snapshots · latest {points[^1].TotalFiles:N0} files · {points[^1].TotalSizeBytes:N0} bytes</p>";
    }
}
