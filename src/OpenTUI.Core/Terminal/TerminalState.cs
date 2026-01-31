using System.Runtime.InteropServices;
using System.Text;
using OpenTUI.Core.Rendering;

namespace OpenTUI.Core.Terminal;

/// <summary>
/// Manages terminal state including raw mode, cursor visibility, and alternate screen.
/// </summary>
public sealed class TerminalState : IDisposable
{
    private bool _isRawMode;
    private bool _isAlternateScreen;
    private bool _isCursorHidden;
    private bool _isMouseEnabled;
    private bool _isBracketedPasteEnabled;
    private bool _disposed;
    
    private readonly TextWriter _output;
    private readonly TerminalCapabilities _capabilities;
    
    // Unix terminal settings backup
    private string? _originalTermSettings;

    public TerminalState(TextWriter? output = null, TerminalCapabilities? capabilities = null)
    {
        _output = output ?? Console.Out;
        _capabilities = capabilities ?? TerminalCapabilities.Detect();
    }

    /// <summary>
    /// Gets the terminal capabilities.
    /// </summary>
    public TerminalCapabilities Capabilities => _capabilities;

    /// <summary>
    /// Whether raw mode is currently enabled.
    /// </summary>
    public bool IsRawMode => _isRawMode;

    /// <summary>
    /// Enables raw mode (disables line buffering, echo, and special key processing).
    /// </summary>
    public void EnableRawMode()
    {
        if (_isRawMode) return;
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            EnableRawModeWindows();
        }
        else
        {
            EnableRawModeUnix();
        }
        
        _isRawMode = true;
    }

    /// <summary>
    /// Disables raw mode, restoring normal terminal behavior.
    /// </summary>
    public void DisableRawMode()
    {
        if (!_isRawMode) return;
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            DisableRawModeWindows();
        }
        else
        {
            DisableRawModeUnix();
        }
        
        _isRawMode = false;
    }

    /// <summary>
    /// Enters the alternate screen buffer.
    /// </summary>
    public void EnterAlternateScreen()
    {
        if (_isAlternateScreen || !_capabilities.SupportsAlternateScreen) return;
        
        Write(Ansi.EnterAlternateScreen);
        _isAlternateScreen = true;
    }

    /// <summary>
    /// Exits the alternate screen buffer.
    /// </summary>
    public void ExitAlternateScreen()
    {
        if (!_isAlternateScreen) return;
        
        Write(Ansi.ExitAlternateScreen);
        _isAlternateScreen = false;
    }

    /// <summary>
    /// Hides the cursor.
    /// </summary>
    public void HideCursor()
    {
        if (_isCursorHidden) return;
        
        Write(Ansi.HideCursor);
        _isCursorHidden = true;
    }

    /// <summary>
    /// Shows the cursor.
    /// </summary>
    public void ShowCursor()
    {
        if (!_isCursorHidden) return;
        
        Write(Ansi.ShowCursor);
        _isCursorHidden = false;
    }

    /// <summary>
    /// Enables mouse input reporting.
    /// </summary>
    public void EnableMouse()
    {
        if (_isMouseEnabled || !_capabilities.SupportsMouse) return;
        
        Write(Ansi.EnableMouse);
        _isMouseEnabled = true;
    }

    /// <summary>
    /// Disables mouse input reporting.
    /// </summary>
    public void DisableMouse()
    {
        if (!_isMouseEnabled) return;
        
        Write(Ansi.DisableMouse);
        _isMouseEnabled = false;
    }

    /// <summary>
    /// Enables bracketed paste mode.
    /// </summary>
    public void EnableBracketedPaste()
    {
        if (_isBracketedPasteEnabled || !_capabilities.SupportsBracketedPaste) return;
        
        Write(Ansi.EnableBracketedPaste);
        _isBracketedPasteEnabled = true;
    }

    /// <summary>
    /// Disables bracketed paste mode.
    /// </summary>
    public void DisableBracketedPaste()
    {
        if (!_isBracketedPasteEnabled) return;
        
        Write(Ansi.DisableBracketedPaste);
        _isBracketedPasteEnabled = false;
    }

    /// <summary>
    /// Clears the screen.
    /// </summary>
    public void ClearScreen()
    {
        Write(Ansi.ClearScreen);
        Write(Ansi.MoveCursor(1, 1));
    }

    /// <summary>
    /// Writes directly to the terminal output.
    /// </summary>
    public void Write(string text)
    {
        _output.Write(text);
        _output.Flush();
    }

    /// <summary>
    /// Sets up the terminal for TUI mode.
    /// </summary>
    public void Setup()
    {
        EnableRawMode();
        EnterAlternateScreen();
        HideCursor();
        EnableMouse();
        EnableBracketedPaste();
        ClearScreen();
    }

    /// <summary>
    /// Restores terminal to normal state.
    /// </summary>
    public void Restore()
    {
        DisableBracketedPaste();
        DisableMouse();
        ShowCursor();
        ExitAlternateScreen();
        DisableRawMode();
        Write(Ansi.Reset);
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        Restore();
        _disposed = true;
    }

    // Platform-specific implementations
    
    private void EnableRawModeWindows()
    {
        // On Windows, we primarily need to handle Console settings
        try
        {
            Console.TreatControlCAsInput = true;
            Console.CursorVisible = false;
        }
        catch
        {
            // May fail in non-interactive environments
        }
    }

    private void DisableRawModeWindows()
    {
        try
        {
            Console.TreatControlCAsInput = false;
            Console.CursorVisible = true;
        }
        catch
        {
            // May fail in non-interactive environments
        }
    }

    private void EnableRawModeUnix()
    {
        try
        {
            // Save current terminal settings
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "stty",
                    Arguments = "-g",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            _originalTermSettings = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            
            // Enable raw mode: disable echo, canonical mode, and signal processing
            process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "stty",
                    Arguments = "raw -echo",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
        }
        catch
        {
            // stty may not be available
        }
    }

    private void DisableRawModeUnix()
    {
        if (_originalTermSettings == null) return;
        
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "stty",
                    Arguments = _originalTermSettings,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
        }
        catch
        {
            // Try fallback
            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "stty",
                        Arguments = "sane",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();
            }
            catch
            {
                // Best effort
            }
        }
    }
}
