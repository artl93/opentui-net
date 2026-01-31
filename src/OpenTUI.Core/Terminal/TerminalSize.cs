using System.Runtime.InteropServices;
using SysConsole = System.Console;

namespace OpenTUI.Core.Terminal;

/// <summary>
/// Provides terminal size detection and resize handling.
/// </summary>
public class TerminalSize
{
    /// <summary>Width in columns.</summary>
    public int Width { get; }
    
    /// <summary>Height in rows.</summary>
    public int Height { get; }

    public TerminalSize(int width, int height)
    {
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Gets the current terminal size.
    /// </summary>
    public static TerminalSize GetCurrent()
    {
        try
        {
            // Try Console class first (works on most platforms)
            if (SysConsole.WindowWidth > 0 && SysConsole.WindowHeight > 0)
            {
                return new TerminalSize(SysConsole.WindowWidth, SysConsole.WindowHeight);
            }
        }
        catch
        {
            // Console properties may throw in non-interactive environments
        }

        // Fallback: try environment variables (commonly set by shells)
        var columns = Environment.GetEnvironmentVariable("COLUMNS");
        var lines = Environment.GetEnvironmentVariable("LINES");
        
        if (int.TryParse(columns, out var width) && int.TryParse(lines, out var height))
        {
            return new TerminalSize(width, height);
        }

        // Default fallback
        return new TerminalSize(80, 24);
    }

    /// <summary>
    /// Event raised when terminal size changes.
    /// </summary>
    public static event EventHandler<TerminalSizeEventArgs>? SizeChanged;

    private static TerminalSize? _lastSize;
    private static Timer? _pollTimer;

    /// <summary>
    /// Starts monitoring for terminal size changes.
    /// </summary>
    public static void StartMonitoring(int pollIntervalMs = 100)
    {
        _lastSize = GetCurrent();
        
        // On Unix, we could use SIGWINCH, but polling works cross-platform
        _pollTimer = new Timer(_ =>
        {
            var current = GetCurrent();
            if (_lastSize != null && (current.Width != _lastSize.Width || current.Height != _lastSize.Height))
            {
                _lastSize = current;
                SizeChanged?.Invoke(null, new TerminalSizeEventArgs(current));
            }
        }, null, pollIntervalMs, pollIntervalMs);
    }

    /// <summary>
    /// Stops monitoring for terminal size changes.
    /// </summary>
    public static void StopMonitoring()
    {
        _pollTimer?.Dispose();
        _pollTimer = null;
    }

    public override string ToString() => $"{Width}x{Height}";

    public override bool Equals(object? obj) 
        => obj is TerminalSize other && Width == other.Width && Height == other.Height;

    public override int GetHashCode() => HashCode.Combine(Width, Height);

    public static bool operator ==(TerminalSize? left, TerminalSize? right) 
        => left?.Equals(right) ?? right is null;

    public static bool operator !=(TerminalSize? left, TerminalSize? right) 
        => !(left == right);
}

/// <summary>
/// Event args for terminal size changes.
/// </summary>
public class TerminalSizeEventArgs : EventArgs
{
    public TerminalSize Size { get; }
    
    public TerminalSizeEventArgs(TerminalSize size)
    {
        Size = size;
    }
}
