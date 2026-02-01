using System.Text;

namespace OpenTUI.Core.Console;

/// <summary>
/// A TextWriter that intercepts console output and redirects it to a LogBuffer.
/// </summary>
public class ConsoleInterceptor : TextWriter
{
    private readonly TextWriter _original;
    private readonly LogBuffer _buffer;
    private readonly LogLevel _defaultLevel;
    private readonly string? _source;
    private readonly StringBuilder _lineBuffer = new();

    public override Encoding Encoding => _original.Encoding;

    /// <summary>The original TextWriter being intercepted.</summary>
    public TextWriter Original => _original;

    /// <summary>The log buffer receiving intercepted output.</summary>
    public LogBuffer Buffer => _buffer;

    /// <summary>Whether to also write to the original output.</summary>
    public bool PassThrough { get; set; }

    public ConsoleInterceptor(TextWriter original, LogBuffer buffer,
        LogLevel defaultLevel = LogLevel.Info, string? source = null)
    {
        _original = original;
        _buffer = buffer;
        _defaultLevel = defaultLevel;
        _source = source;
    }

    public override void Write(char value)
    {
        if (value == '\n')
        {
            FlushLine();
        }
        else if (value != '\r')
        {
            _lineBuffer.Append(value);
        }

        if (PassThrough)
            _original.Write(value);
    }

    public override void Write(string? value)
    {
        if (value == null) return;

        foreach (var ch in value)
        {
            if (ch == '\n')
            {
                FlushLine();
            }
            else if (ch != '\r')
            {
                _lineBuffer.Append(ch);
            }
        }

        if (PassThrough)
            _original.Write(value);
    }

    public override void WriteLine()
    {
        FlushLine();
        if (PassThrough)
            _original.WriteLine();
    }

    public override void WriteLine(string? value)
    {
        if (value != null)
        {
            // Strip carriage returns
            foreach (var ch in value)
            {
                if (ch != '\r')
                    _lineBuffer.Append(ch);
            }
        }
        FlushLine();

        if (PassThrough)
            _original.WriteLine(value);
    }

    private void FlushLine()
    {
        if (_lineBuffer.Length > 0)
        {
            var message = _lineBuffer.ToString();
            _lineBuffer.Clear();

            // Try to detect log level from message prefix
            var (level, cleanMessage) = DetectLogLevel(message);
            _buffer.Add(cleanMessage, level, _source);
        }
    }

    private (LogLevel, string) DetectLogLevel(string message)
    {
        var upper = message.TrimStart();

        if (upper.StartsWith("[ERROR]", StringComparison.OrdinalIgnoreCase) ||
            upper.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase))
            return (LogLevel.Error, message);

        if (upper.StartsWith("[WARN]", StringComparison.OrdinalIgnoreCase) ||
            upper.StartsWith("[WARNING]", StringComparison.OrdinalIgnoreCase) ||
            upper.StartsWith("WARNING:", StringComparison.OrdinalIgnoreCase))
            return (LogLevel.Warning, message);

        if (upper.StartsWith("[DEBUG]", StringComparison.OrdinalIgnoreCase) ||
            upper.StartsWith("DEBUG:", StringComparison.OrdinalIgnoreCase))
            return (LogLevel.Debug, message);

        if (upper.StartsWith("[INFO]", StringComparison.OrdinalIgnoreCase) ||
            upper.StartsWith("INFO:", StringComparison.OrdinalIgnoreCase))
            return (LogLevel.Info, message);

        return (_defaultLevel, message);
    }

    public override void Flush()
    {
        if (_lineBuffer.Length > 0)
            FlushLine();
        _original.Flush();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Flush();
        }
        base.Dispose(disposing);
    }
}
