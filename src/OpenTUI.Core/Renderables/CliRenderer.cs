using System.Diagnostics;
using OpenTUI.Core.Colors;
using OpenTUI.Core.Layout;
using OpenTUI.Core.Rendering;
using OpenTUI.Core.Terminal;
using SysConsole = System.Console;

namespace OpenTUI.Core.Renderables;

/// <summary>
/// Options for creating a CLI renderer.
/// </summary>
public class CliRendererOptions
{
    /// <summary>Target frames per second for the render loop.</summary>
    public int TargetFps { get; set; } = 60;

    /// <summary>Whether to use the alternate screen buffer.</summary>
    public bool UseAlternateScreen { get; set; } = true;

    /// <summary>Whether to hide the cursor.</summary>
    public bool HideCursor { get; set; } = true;

    /// <summary>Whether to enable mouse input.</summary>
    public bool EnableMouse { get; set; } = true;

    /// <summary>Default background color.</summary>
    public RGBA BackgroundColor { get; set; } = RGBA.Black;

    /// <summary>Default foreground color.</summary>
    public RGBA ForegroundColor { get; set; } = RGBA.White;
}

/// <summary>
/// The main renderer for CLI applications.
/// Manages the render loop, terminal state, and root renderable.
/// </summary>
public class CliRenderer : IDisposable
{
    private readonly TerminalState _terminal;
    private readonly CliRendererOptions _options;
    private FrameBuffer _buffer;
    private FrameBuffer _backBuffer;
    private bool _isRunning;
    private bool _disposed;
    private IRenderable? _focusedElement;
    private readonly Stopwatch _frameTimer = new();

    /// <summary>The root renderable container.</summary>
    public Renderable Root { get; }

    /// <summary>Terminal capabilities.</summary>
    public TerminalCapabilities Capabilities => _terminal.Capabilities;

    /// <summary>Current terminal size.</summary>
    public TerminalSize Size { get; private set; }

    /// <summary>Current width.</summary>
    public int Width => Size.Width;

    /// <summary>Current height.</summary>
    public int Height => Size.Height;

    /// <summary>Whether the render loop is running.</summary>
    public bool IsRunning => _isRunning;

    /// <summary>The currently focused element.</summary>
    public IRenderable? Focused => _focusedElement;

    /// <summary>Target FPS.</summary>
    public int TargetFps
    {
        get => _options.TargetFps;
        set => _options.TargetFps = value;
    }

    /// <summary>Default background color.</summary>
    public RGBA DefaultBackground
    {
        get => _options.BackgroundColor;
        set
        {
            _options.BackgroundColor = value;
            Root.BackgroundColor = value;
        }
    }

    /// <summary>Target frame time in milliseconds.</summary>
    public double TargetFrameTime => 1000.0 / _options.TargetFps;

    /// <summary>Event raised before each frame renders.</summary>
    public event EventHandler? BeforeRender;

    /// <summary>Event raised after each frame renders.</summary>
    public event EventHandler? AfterRender;

    /// <summary>Event raised when terminal is resized.</summary>
    public event EventHandler<TerminalSizeEventArgs>? Resized;

    private CliRenderer(CliRendererOptions options, TerminalCapabilities? capabilities = null)
    {
        _options = options;
        _terminal = new TerminalState(SysConsole.Out, capabilities);

        Size = TerminalSize.GetCurrent();
        _buffer = new FrameBuffer(Size.Width, Size.Height);
        _backBuffer = new FrameBuffer(Size.Width, Size.Height);

        Root = new GroupRenderable
        {
            Layout =
            {
                Width = Size.Width,
                Height = Size.Height
            },
            BackgroundColor = options.BackgroundColor
        };

        TerminalSize.SizeChanged += OnTerminalResized;
    }

    /// <summary>
    /// Creates a new CLI renderer.
    /// </summary>
    public static CliRenderer Create(CliRendererOptions? options = null)
    {
        return new CliRenderer(options ?? new CliRendererOptions());
    }

    /// <summary>
    /// Creates a renderer for testing (no terminal interaction).
    /// </summary>
    public static CliRenderer CreateForTesting(int width = 80, int height = 24)
    {
        var renderer = new CliRenderer(
            new CliRendererOptions { UseAlternateScreen = false, HideCursor = false, EnableMouse = false },
            TerminalCapabilities.Dumb
        );
        renderer.Size = new TerminalSize(width, height);
        renderer._buffer = new FrameBuffer(width, height);
        renderer._backBuffer = new FrameBuffer(width, height);
        renderer.Root.Layout.Width = width;
        renderer.Root.Layout.Height = height;
        return renderer;
    }

    /// <summary>
    /// Sets up the terminal for TUI mode.
    /// </summary>
    public void Setup()
    {
        if (_options.UseAlternateScreen)
            _terminal.EnterAlternateScreen();
        if (_options.HideCursor)
            _terminal.HideCursor();
        if (_options.EnableMouse)
            _terminal.EnableMouse();

        _terminal.EnableBracketedPaste();
        _terminal.ClearScreen();

        TerminalSize.StartMonitoring();
    }

    /// <summary>
    /// Starts the render loop.
    /// </summary>
    public void Start()
    {
        Setup();
        _isRunning = true;

        while (_isRunning)
        {
            _frameTimer.Restart();

            RenderFrame();

            // Frame timing
            var elapsed = _frameTimer.Elapsed.TotalMilliseconds;
            var sleepTime = TargetFrameTime - elapsed;
            if (sleepTime > 0)
            {
                Thread.Sleep((int)sleepTime);
            }
        }
    }

    /// <summary>
    /// Stops the render loop.
    /// </summary>
    public void Stop()
    {
        _isRunning = false;
    }

    /// <summary>
    /// Renders a single frame.
    /// </summary>
    public void RenderFrame()
    {
        BeforeRender?.Invoke(this, EventArgs.Empty);

        // Calculate layout if dirty
        if (Root.Layout.IsDirty)
        {
            Root.Layout.CalculateLayout(Size.Width, Size.Height);
        }

        // Clear back buffer
        _backBuffer.Clear(_options.BackgroundColor);

        // Render tree to back buffer
        Root.Render(_backBuffer, 0, 0);

        // Output to terminal (differential update)
        var output = GetDifferentialOutput();
        if (!string.IsNullOrEmpty(output))
        {
            _terminal.Write(output);
        }

        // Swap buffers
        (_buffer, _backBuffer) = (_backBuffer, _buffer);
        _buffer.ClearDirty();

        AfterRender?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Renders immediately without the render loop.
    /// </summary>
    public void Render()
    {
        if (Root.Layout.IsDirty)
        {
            Root.Layout.CalculateLayout(Size.Width, Size.Height);
        }

        _buffer.Clear(_options.BackgroundColor);
        Root.Render(_buffer, 0, 0);
    }

    /// <summary>
    /// Gets the current buffer content (for testing).
    /// </summary>
    public FrameBuffer GetBuffer() => _buffer;

    /// <summary>
    /// Focuses an element.
    /// </summary>
    public void SetFocused(IRenderable? element)
    {
        if (_focusedElement == element) return;

        _focusedElement?.Blur();
        _focusedElement = element;
        _focusedElement?.Focus();
    }

    /// <summary>
    /// Gets the rendered output as an ANSI string.
    /// </summary>
    /// <param name="differential">If true, returns only changes since last output.</param>
    public string GetOutput(bool differential = false)
    {
        if (differential)
        {
            return _buffer.ToDifferentialAnsiString();
        }
        return _buffer.ToAnsiString();
    }

    /// <summary>
    /// Resizes the renderer.
    /// </summary>
    public void Resize(int width, int height)
    {
        Size = new TerminalSize(width, height);
        _buffer = new FrameBuffer(width, height);
        _backBuffer = new FrameBuffer(width, height);
        Root.Layout.Width = width;
        Root.Layout.Height = height;
        Root.MarkDirty();
    }

    /// <summary>
    /// Moves focus to the next focusable element.
    /// </summary>
    public void FocusNext()
    {
        var focusable = Root.GetFocusableDescendants().ToList();
        if (focusable.Count == 0) return;

        var currentIndex = _focusedElement != null ? focusable.IndexOf(_focusedElement) : -1;
        var nextIndex = (currentIndex + 1) % focusable.Count;
        SetFocused(focusable[nextIndex]);
    }

    /// <summary>
    /// Moves focus to the previous focusable element.
    /// </summary>
    public void FocusPrevious()
    {
        var focusable = Root.GetFocusableDescendants().ToList();
        if (focusable.Count == 0) return;

        var currentIndex = _focusedElement != null ? focusable.IndexOf(_focusedElement) : 0;
        var prevIndex = currentIndex <= 0 ? focusable.Count - 1 : currentIndex - 1;
        SetFocused(focusable[prevIndex]);
    }

    private string GetDifferentialOutput()
    {
        // Compare back buffer with current buffer and generate minimal ANSI output
        var sb = new System.Text.StringBuilder();
        int lastRow = -1, lastCol = -1;
        RGBA? lastFg = null, lastBg = null;
        TextAttributes lastAttrs = TextAttributes.None;

        for (var r = 0; r < Size.Height && r < _backBuffer.Height; r++)
        {
            for (var c = 0; c < Size.Width && c < _backBuffer.Width; c++)
            {
                var newCell = _backBuffer.GetCell(r, c);
                var oldCell = _buffer.GetCell(r, c);

                if (newCell == oldCell) continue;

                // Move cursor if needed
                if (r != lastRow || c != lastCol + 1)
                {
                    sb.Append(Ansi.MoveCursor(r + 1, c + 1));
                }

                // Set style if changed
                if (lastFg != newCell.Foreground || lastBg != newCell.Background || lastAttrs != newCell.Attributes)
                {
                    sb.Append(Ansi.SetStyle(newCell.Foreground, newCell.Background, newCell.Attributes));
                    lastFg = newCell.Foreground;
                    lastBg = newCell.Background;
                    lastAttrs = newCell.Attributes;
                }

                sb.Append(newCell.Character);
                lastRow = r;
                lastCol = c;
            }
        }

        if (sb.Length > 0)
        {
            sb.Append(Ansi.Reset);
        }

        return sb.ToString();
    }

    private void OnTerminalResized(object? sender, TerminalSizeEventArgs e)
    {
        Size = e.Size;

        // Resize buffers
        _buffer = new FrameBuffer(Size.Width, Size.Height);
        _backBuffer = new FrameBuffer(Size.Width, Size.Height);

        // Update root size
        Root.Layout.Width = Size.Width;
        Root.Layout.Height = Size.Height;
        Root.MarkDirty();

        Resized?.Invoke(this, e);
    }

    public void Dispose()
    {
        if (_disposed) return;

        Stop();
        TerminalSize.StopMonitoring();
        TerminalSize.SizeChanged -= OnTerminalResized;
        _terminal.Dispose();

        _disposed = true;
    }
}

/// <summary>
/// A simple group container for renderables.
/// </summary>
public class GroupRenderable : Renderable
{
    public GroupRenderable()
    {
        Layout.FlexDirection = FlexDirection.Column;
    }
}
