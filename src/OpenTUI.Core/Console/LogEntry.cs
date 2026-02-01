namespace OpenTUI.Core.Console;

/// <summary>
/// Log level for console messages.
/// </summary>
public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}

/// <summary>
/// A log entry captured from console output.
/// </summary>
public readonly record struct LogEntry
{
    /// <summary>The log message.</summary>
    public string Message { get; init; }

    /// <summary>The log level.</summary>
    public LogLevel Level { get; init; }

    /// <summary>When the entry was created.</summary>
    public DateTime Timestamp { get; init; }

    /// <summary>Optional source/category.</summary>
    public string? Source { get; init; }

    public LogEntry(string message, LogLevel level = LogLevel.Info, string? source = null)
    {
        Message = message;
        Level = level;
        Timestamp = DateTime.Now;
        Source = source;
    }

    public override string ToString()
    {
        var time = Timestamp.ToString("HH:mm:ss.fff");
        var level = Level.ToString().ToUpper()[0];
        return Source != null
            ? $"[{time}] [{level}] [{Source}] {Message}"
            : $"[{time}] [{level}] {Message}";
    }
}
