using System.Collections.Concurrent;

namespace OpenTUI.Core.Console;

/// <summary>
/// A circular buffer for storing log entries with a maximum capacity.
/// </summary>
public class LogBuffer
{
    private readonly ConcurrentQueue<LogEntry> _entries = new();
    private readonly int _maxEntries;
    private int _count;

    /// <summary>Maximum number of entries to keep.</summary>
    public int MaxEntries => _maxEntries;

    /// <summary>Current number of entries.</summary>
    public int Count => _count;

    /// <summary>Event raised when a new entry is added.</summary>
    public event EventHandler<LogEntry>? EntryAdded;

    /// <summary>Event raised when entries are cleared.</summary>
    public event EventHandler? Cleared;

    public LogBuffer(int maxEntries = 1000)
    {
        _maxEntries = maxEntries;
    }

    /// <summary>Adds a log entry.</summary>
    public void Add(LogEntry entry)
    {
        _entries.Enqueue(entry);
        Interlocked.Increment(ref _count);

        // Remove old entries if over capacity
        while (_count > _maxEntries && _entries.TryDequeue(out _))
        {
            Interlocked.Decrement(ref _count);
        }

        EntryAdded?.Invoke(this, entry);
    }

    /// <summary>Adds a message with the specified level.</summary>
    public void Add(string message, LogLevel level = LogLevel.Info, string? source = null)
    {
        Add(new LogEntry(message, level, source));
    }

    /// <summary>Adds a debug message.</summary>
    public void Debug(string message, string? source = null)
        => Add(message, LogLevel.Debug, source);

    /// <summary>Adds an info message.</summary>
    public void Info(string message, string? source = null)
        => Add(message, LogLevel.Info, source);

    /// <summary>Adds a warning message.</summary>
    public void Warning(string message, string? source = null)
        => Add(message, LogLevel.Warning, source);

    /// <summary>Adds an error message.</summary>
    public void Error(string message, string? source = null)
        => Add(message, LogLevel.Error, source);

    /// <summary>Gets all entries.</summary>
    public IEnumerable<LogEntry> GetEntries()
        => _entries.ToArray();

    /// <summary>Gets entries filtered by level.</summary>
    public IEnumerable<LogEntry> GetEntries(LogLevel minLevel)
        => _entries.Where(e => e.Level >= minLevel);

    /// <summary>Gets the most recent entries.</summary>
    public IEnumerable<LogEntry> GetRecentEntries(int count)
        => _entries.TakeLast(count);

    /// <summary>Clears all entries.</summary>
    public void Clear()
    {
        while (_entries.TryDequeue(out _))
        {
            Interlocked.Decrement(ref _count);
        }
        Cleared?.Invoke(this, EventArgs.Empty);
    }
}
