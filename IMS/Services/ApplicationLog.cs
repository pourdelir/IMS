using System.Text.Json;

namespace IMS.Services;

/// <summary>Small structured JSON-lines log stored outside monitored archives.</summary>
public sealed class ApplicationLog
{
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly string _logPath;

    public ApplicationLog(string? logPath = null)
    {
        var logDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
        _logPath = logPath ?? Path.Combine(logDirectory, $"archive-auditor-{DateTime.UtcNow:yyyyMMdd}.jsonl");
    }

    public async Task WriteAsync(string level, string eventName, string message,
        Exception? exception = null, CancellationToken cancellationToken = default)
    {
        var entry = new LogEntry(DateTime.UtcNow, level, eventName, message, exception?.ToString());
        var line = JsonSerializer.Serialize(entry) + Environment.NewLine;
        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);
            await File.AppendAllTextAsync(_logPath, line, cancellationToken);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private sealed record LogEntry(DateTime TimestampUtc, string Level, string Event, string Message, string? Exception);
}
