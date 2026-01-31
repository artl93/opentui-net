using OpenTUI.Core.Colors;
using OpenTUI.Core.Layout;
using OpenTUI.Core.Renderables;
using OpenTUI.Core.Rendering;

namespace OpenTUI.Core.Console;

/// <summary>
/// A scrollable overlay that displays captured console output.
/// Can be toggled on/off during application execution.
/// </summary>
public class ConsoleOverlay : Renderable, IDisposable
{
    private readonly LogBuffer _logBuffer;
    private ConsoleInterceptor? _stdOutInterceptor;
    private ConsoleInterceptor? _stdErrInterceptor;
    
    private int _scrollOffset;
    private LogLevel _minLevel = LogLevel.Debug;
    private bool _autoScroll = true;
    private bool _isIntercepting;
    private bool _disposed;

    /// <summary>The log buffer containing captured messages.</summary>
    public LogBuffer LogBuffer => _logBuffer;

    /// <summary>Current scroll offset (0 = most recent at bottom).</summary>
    public int ScrollOffset
    {
        get => _scrollOffset;
        set
        {
            var max = Math.Max(0, GetVisibleEntries().Count() - VisibleLineCount);
            _scrollOffset = Math.Clamp(value, 0, max);
            MarkDirty();
        }
    }

    /// <summary>Minimum log level to display.</summary>
    public LogLevel MinLevel
    {
        get => _minLevel;
        set
        {
            _minLevel = value;
            MarkDirty();
        }
    }

    /// <summary>Auto-scroll to bottom on new entries.</summary>
    public bool AutoScroll
    {
        get => _autoScroll;
        set => _autoScroll = value;
    }

    /// <summary>Whether the overlay is visible.</summary>
    public bool IsVisible
    {
        get => Visible;
        set
        {
            Visible = value;
            MarkDirty();
        }
    }

    /// <summary>Border style for the overlay.</summary>
    public Renderables.BorderStyle Border { get; set; } = Renderables.BorderStyle.Single;

    /// <summary>Title shown in the border.</summary>
    public string Title { get; set; } = "Console";

    /// <summary>Overlay background color.</summary>
    public RGBA OverlayBackgroundColor { get; set; } = RGBA.FromValues(0.1f, 0.1f, 0.1f, 0.95f);

    /// <summary>Number of visible lines (excluding border).</summary>
    private int VisibleLineCount => Math.Max(0, (int)Layout.Layout.Height - 2);

    public ConsoleOverlay() : this(new LogBuffer()) { }

    public ConsoleOverlay(LogBuffer logBuffer)
    {
        _logBuffer = logBuffer;
        _logBuffer.EntryAdded += OnEntryAdded;
        
        // Default layout - bottom 30% of screen
        Layout.PositionType = PositionType.Absolute;
        Layout.Width = FlexValue.Percent(100);
        Layout.Height = FlexValue.Percent(30);
        Layout.Bottom = FlexValue.Points(0);
        Layout.Left = FlexValue.Points(0);
    }

    /// <summary>
    /// Creates an overlay that intercepts Console.Out and Console.Error.
    /// </summary>
    public static ConsoleOverlay CreateWithInterception(LogBuffer? buffer = null)
    {
        buffer ??= new LogBuffer();
        var overlay = new ConsoleOverlay(buffer);
        overlay.StartInterception();
        return overlay;
    }

    /// <summary>Starts intercepting Console.Out and Console.Error.</summary>
    public void StartInterception()
    {
        if (_isIntercepting) return;

        var stdOutInterceptor = new ConsoleInterceptor(
            System.Console.Out, _logBuffer, LogLevel.Info, "stdout");
        var stdErrInterceptor = new ConsoleInterceptor(
            System.Console.Error, _logBuffer, LogLevel.Error, "stderr");

        System.Console.SetOut(stdOutInterceptor);
        System.Console.SetError(stdErrInterceptor);
        _isIntercepting = true;
    }

    /// <summary>Stops intercepting and restores original console.</summary>
    public void StopInterception()
    {
        if (!_isIntercepting) return;

        if (_stdOutInterceptor != null)
            System.Console.SetOut(_stdOutInterceptor.Original);
        if (_stdErrInterceptor != null)
            System.Console.SetError(_stdErrInterceptor.Original);
        _isIntercepting = false;
    }

    private void OnEntryAdded(object? sender, LogEntry entry)
    {
        if (_autoScroll)
            _scrollOffset = 0;
        MarkDirty();
    }

    private IEnumerable<LogEntry> GetVisibleEntries()
        => _logBuffer.GetEntries(_minLevel);

     /// <summary>
    /// Renders the console overlay.
    /// </summary>
    protected override void RenderSelf(FrameBuffer buffer, int x, int y, int w, int h)
    {

        if (w <= 0 || h <= 0) return;

        // Fill background
        buffer.FillRect(x, y, w, h, OverlayBackgroundColor);

        // Draw border
        if (Border != Renderables.BorderStyle.None && h >= 2 && w >= 2)
        {
            var borderChars = GetBorderChars(Border);
            DrawBorder(buffer, x, y, w, h, borderChars);

            // Draw title
            if (!string.IsNullOrEmpty(Title) && w > 4)
            {
                var title = Title.Length > w - 4 ? Title[..(w - 4)] : Title;
                var titleX = x + 2;
                buffer.DrawText($" {title} ", titleX, y, RGBA.White, OverlayBackgroundColor);
            }

            // Draw level filter indicator
            var levelText = $" [{_minLevel}+] ";
            if (w > levelText.Length + 4)
            {
                buffer.DrawText(levelText, x + w - levelText.Length - 2, y, 
                    GetLevelColor(_minLevel), OverlayBackgroundColor);
            }
        }

        // Draw log entries
        var entries = GetVisibleEntries().ToList();
        var visibleLines = VisibleLineCount;
        var startIndex = Math.Max(0, entries.Count - visibleLines - _scrollOffset);
        var endIndex = Math.Min(entries.Count, startIndex + visibleLines);

        var innerX = x + 1;
        var innerWidth = w - 2;
        var lineY = y + 1;

        for (var i = startIndex; i < endIndex && lineY < y + h - 1; i++)
        {
            var entry = entries[i];
            var line = FormatEntry(entry, innerWidth);
            var color = GetLevelColor(entry.Level);
            buffer.DrawText(line, innerX, lineY, color, OverlayBackgroundColor);
            lineY++;
        }

        // Draw scroll indicator if scrolled
        if (_scrollOffset > 0 && h >= 3)
        {
            var indicator = $"▲ {_scrollOffset} more";
            buffer.DrawText(indicator, x + w - indicator.Length - 2, y + h - 1, 
                RGBA.Yellow, OverlayBackgroundColor);
        }
    }

    private static string FormatEntry(LogEntry entry, int maxWidth)
    {
        var formatted = entry.ToString();
        return formatted.Length > maxWidth ? formatted[..maxWidth] : formatted.PadRight(maxWidth);
    }

    private static RGBA GetLevelColor(LogLevel level) => level switch
    {
        LogLevel.Debug => RGBA.FromValues(0.5f, 0.5f, 0.5f), // Gray
        LogLevel.Info => RGBA.FromValues(0.8f, 0.8f, 0.8f),  // Light gray
        LogLevel.Warning => RGBA.FromValues(1f, 0.8f, 0f),    // Yellow
        LogLevel.Error => RGBA.FromValues(1f, 0.3f, 0.3f),    // Red
        _ => RGBA.White
    };

    private static (string tl, string tr, string bl, string br, string horiz, string vert) GetBorderChars(Renderables.BorderStyle style) 
        => style switch
    {
        Renderables.BorderStyle.Single => ("┌", "┐", "└", "┘", "─", "│"),
        Renderables.BorderStyle.Double => ("╔", "╗", "╚", "╝", "═", "║"),
        Renderables.BorderStyle.Rounded => ("╭", "╮", "╰", "╯", "─", "│"),
        Renderables.BorderStyle.Bold => ("┏", "┓", "┗", "┛", "━", "┃"),
        Renderables.BorderStyle.Dashed => ("+", "+", "+", "+", "-", "|"),
        _ => (" ", " ", " ", " ", " ", " ")
    };

    private static void DrawBorder(FrameBuffer buffer, int x, int y, int w, int h,
        (string tl, string tr, string bl, string br, string horiz, string vert) chars)
    {
        var borderColor = RGBA.FromValues(0.4f, 0.4f, 0.4f);

        // Corners
        buffer.SetCell(x, y, new Cell(chars.tl, borderColor, RGBA.Transparent));
        buffer.SetCell(x + w - 1, y, new Cell(chars.tr, borderColor, RGBA.Transparent));
        buffer.SetCell(x, y + h - 1, new Cell(chars.bl, borderColor, RGBA.Transparent));
        buffer.SetCell(x + w - 1, y + h - 1, new Cell(chars.br, borderColor, RGBA.Transparent));

        // Horizontal lines
        for (var i = 1; i < w - 1; i++)
        {
            buffer.SetCell(x + i, y, new Cell(chars.horiz, borderColor, RGBA.Transparent));
            buffer.SetCell(x + i, y + h - 1, new Cell(chars.horiz, borderColor, RGBA.Transparent));
        }

        // Vertical lines
        for (var j = 1; j < h - 1; j++)
        {
            buffer.SetCell(x, y + j, new Cell(chars.vert, borderColor, RGBA.Transparent));
            buffer.SetCell(x + w - 1, y + j, new Cell(chars.vert, borderColor, RGBA.Transparent));
        }
    }

    /// <summary>Scrolls up by the specified number of lines.</summary>
    public void ScrollUp(int lines = 1)
    {
        var max = Math.Max(0, GetVisibleEntries().Count() - VisibleLineCount);
        ScrollOffset = Math.Min(_scrollOffset + lines, max);
    }

    /// <summary>Scrolls down by the specified number of lines.</summary>
    public void ScrollDown(int lines = 1)
    {
        ScrollOffset = Math.Max(0, _scrollOffset - lines);
    }

    /// <summary>Scrolls to the top (oldest entries).</summary>
    public void ScrollToTop()
    {
        var max = Math.Max(0, GetVisibleEntries().Count() - VisibleLineCount);
        ScrollOffset = max;
    }

    /// <summary>Scrolls to the bottom (newest entries).</summary>
    public void ScrollToBottom()
    {
        ScrollOffset = 0;
    }

    /// <summary>Cycles through log level filters.</summary>
    public void CycleLogLevel()
    {
        MinLevel = (LogLevel)(((int)_minLevel + 1) % 4);
    }

    /// <summary>Clears all log entries.</summary>
    public void ClearLog()
    {
        _logBuffer.Clear();
        _scrollOffset = 0;
        MarkDirty();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        
        if (disposing)
        {
            StopInterception();
            _logBuffer.EntryAdded -= OnEntryAdded;
        }
        _disposed = true;
    }
}
