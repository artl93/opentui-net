using OpenTUI.Components.Theme;
using OpenTUI.Core.Rendering;
using OpenTUI.Core.Terminal;

namespace OpenTUI.Components;

/// <summary>
/// Application root that provides theming and manages the render loop.
/// </summary>
public class App : IDisposable
{
    private readonly TerminalState _terminalState;
    private bool _running;
    private bool _disposed;
    
    /// <summary>Current theme.</summary>
    public Theme.Theme Theme
    {
        get => ThemeProvider.Current;
        set => ThemeProvider.Current = value;
    }
    
    /// <summary>Target frames per second.</summary>
    public int TargetFps { get; set; } = 30;
    
    /// <summary>
    /// Creates a new App with the specified theme.
    /// </summary>
    public App(Theme.Theme? theme = null)
    {
        _terminalState = new TerminalState();
        Theme = theme ?? Theme.Dark;
    }
    
    /// <summary>
    /// Runs the application with the given root component.
    /// </summary>
    public void Run(Func<Components.Core.Component> rootFactory)
    {
        _running = true;
        
        try
        {
            _terminalState.EnterAlternateScreen();
            _terminalState.HideCursor();
            
            Console.CancelKeyPress += (_, e) => 
            {
                _running = false;
                e.Cancel = true;
            };
            
            var frameDelay = TimeSpan.FromMilliseconds(1000.0 / TargetFps);
            
            while (_running)
            {
                var size = TerminalSize.GetCurrent();
                var buffer = new FrameBuffer(size.Width, size.Height);
                
                // Create and render root
                var root = rootFactory();
                root.Render(buffer, 0, 0);
                
                // Output
                Console.Write("\x1b[H");
                Console.Write(buffer.ToAnsiString());
                Console.Out.Flush();
                
                // Handle input
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Q && key.Modifiers.HasFlag(ConsoleModifiers.Control))
                    {
                        _running = false;
                    }
                }
                
                Thread.Sleep(frameDelay);
            }
        }
        finally
        {
            _terminalState.ShowCursor();
            _terminalState.ExitAlternateScreen();
        }
    }
    
    /// <summary>
    /// Stops the application.
    /// </summary>
    public void Stop() => _running = false;
    
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _running = false;
        }
    }
}
