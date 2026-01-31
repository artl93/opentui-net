using OpenTUI.Core.Renderables;

namespace OpenTUI.Core.Input;

/// <summary>
/// Handles keyboard input and dispatches events to renderables.
/// </summary>
public class InputHandler : IDisposable
{
    private readonly KeyReader _keyReader;
    private readonly CliRenderer _renderer;
    private bool _disposed;

    /// <summary>Global key event before it's dispatched to focused element.</summary>
    public event EventHandler<KeyEventArgs>? KeyDown;

    /// <summary>Creates an input handler for the given renderer.</summary>
    public InputHandler(CliRenderer renderer)
    {
        _renderer = renderer;
        _keyReader = new KeyReader();
        _keyReader.KeyPressed += OnKeyPressed;
    }

    /// <summary>Starts listening for input.</summary>
    public void Start()
    {
        _keyReader.StartReading();
    }

    /// <summary>Stops listening for input.</summary>
    public void Stop()
    {
        _keyReader.StopReading();
    }

    /// <summary>Processes any available input synchronously.</summary>
    public void ProcessInput()
    {
        foreach (var keyEvent in _keyReader.ReadAvailableKeys())
        {
            DispatchKeyEvent(keyEvent);
        }
    }

    /// <summary>Reads and processes a single key synchronously.</summary>
    public bool ProcessSingleKey()
    {
        var keyEvent = _keyReader.ReadKey();
        if (keyEvent.HasValue)
        {
            DispatchKeyEvent(keyEvent.Value);
            return true;
        }
        return false;
    }

    private void OnKeyPressed(object? sender, KeyEvent keyEvent)
    {
        DispatchKeyEvent(keyEvent);
    }

    private void DispatchKeyEvent(KeyEvent keyEvent)
    {
        var args = new KeyEventArgs(keyEvent);
        
        // Raise global event first
        KeyDown?.Invoke(this, args);
        if (args.Handled)
            return;

        // Handle built-in navigation
        if (!args.Handled)
        {
            switch (keyEvent.Key)
            {
                case Key.Tab when !keyEvent.Shift:
                    _renderer.FocusNext();
                    args.Handled = true;
                    break;
                case Key.Tab when keyEvent.Shift:
                    _renderer.FocusPrevious();
                    args.Handled = true;
                    break;
            }
        }

        if (args.Handled)
            return;

        // Dispatch to focused element
        var focused = _renderer.Focused;
        if (focused != null)
        {
            DispatchToRenderable(focused, keyEvent);
        }
    }

    private void DispatchToRenderable(IRenderable renderable, KeyEvent keyEvent)
    {
        // Handle common widget interactions
        switch (renderable)
        {
            case InputRenderable input:
                HandleInputKey(input, keyEvent);
                break;
            case SelectRenderable select:
                HandleSelectKey(select, keyEvent);
                break;
            case TabSelectRenderable tabs:
                HandleTabSelectKey(tabs, keyEvent);
                break;
            case SliderRenderable slider:
                HandleSliderKey(slider, keyEvent);
                break;
            case ScrollBoxRenderable scroll:
                HandleScrollBoxKey(scroll, keyEvent);
                break;
        }
    }

    private static void HandleInputKey(InputRenderable input, KeyEvent keyEvent)
    {
        switch (keyEvent.Key)
        {
            case Key.Backspace:
                input.Backspace();
                break;
            case Key.Delete:
                input.Delete();
                break;
            case Key.Left:
                input.MoveCursorLeft();
                break;
            case Key.Right:
                input.MoveCursorRight();
                break;
            case Key.Home:
                input.MoveCursorHome();
                break;
            case Key.End:
                input.MoveCursorEnd();
                break;
            case Key.Enter:
                input.Submit();
                break;
            default:
                if (keyEvent.IsPrintable && keyEvent.Character.HasValue)
                {
                    input.Insert(keyEvent.Character.Value);
                }
                break;
        }
    }

    private static void HandleSelectKey(SelectRenderable select, KeyEvent keyEvent)
    {
        switch (keyEvent.Key)
        {
            case Key.Up:
                select.SelectPrevious();
                break;
            case Key.Down:
                select.SelectNext();
                break;
            case Key.Home:
                select.SelectFirst();
                break;
            case Key.End:
                select.SelectLast();
                break;
            case Key.Enter or Key.Space:
                select.Activate();
                break;
        }
    }

    private static void HandleTabSelectKey(TabSelectRenderable tabs, KeyEvent keyEvent)
    {
        switch (keyEvent.Key)
        {
            case Key.Left:
                tabs.SelectPrevious();
                break;
            case Key.Right:
                tabs.SelectNext();
                break;
        }
    }

    private static void HandleSliderKey(SliderRenderable slider, KeyEvent keyEvent)
    {
        switch (keyEvent.Key)
        {
            case Key.Left:
                slider.Decrement();
                break;
            case Key.Right:
                slider.Increment();
                break;
            case Key.Home:
                slider.SetToMin();
                break;
            case Key.End:
                slider.SetToMax();
                break;
        }
    }

    private static void HandleScrollBoxKey(ScrollBoxRenderable scroll, KeyEvent keyEvent)
    {
        switch (keyEvent.Key)
        {
            case Key.Up:
                scroll.ScrollUp();
                break;
            case Key.Down:
                scroll.ScrollDown();
                break;
            case Key.Left:
                scroll.ScrollLeft();
                break;
            case Key.Right:
                scroll.ScrollRight();
                break;
            case Key.PageUp:
                scroll.ScrollUp(10);
                break;
            case Key.PageDown:
                scroll.ScrollDown(10);
                break;
            case Key.Home:
                scroll.ScrollToTop();
                break;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _keyReader.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// Event arguments for key events.
/// </summary>
public class KeyEventArgs : EventArgs
{
    /// <summary>The key event.</summary>
    public KeyEvent KeyEvent { get; }
    
    /// <summary>Whether the event has been handled.</summary>
    public bool Handled { get; set; }

    public KeyEventArgs(KeyEvent keyEvent)
    {
        KeyEvent = keyEvent;
    }
}
