using System.Text;
using OpenTUI.Core.Colors;

namespace OpenTUI.Core.Rendering;

/// <summary>
/// ANSI escape sequence generator for terminal output.
/// </summary>
public static class Ansi
{
    public const string Escape = "\x1b";
    public const string Csi = "\x1b[";
    public const string Osc = "\x1b]";
    
    // Cursor control
    public static string MoveCursor(int row, int col) => $"{Csi}{row};{col}H";
    public static string MoveCursorUp(int n = 1) => $"{Csi}{n}A";
    public static string MoveCursorDown(int n = 1) => $"{Csi}{n}B";
    public static string MoveCursorRight(int n = 1) => $"{Csi}{n}C";
    public static string MoveCursorLeft(int n = 1) => $"{Csi}{n}D";
    public static string SaveCursorPosition => $"{Csi}s";
    public static string RestoreCursorPosition => $"{Csi}u";
    public static string HideCursor => $"{Csi}?25l";
    public static string ShowCursor => $"{Csi}?25h";
    
    // Screen control
    public static string ClearScreen => $"{Csi}2J";
    public static string ClearScreenFromCursor => $"{Csi}0J";
    public static string ClearScreenToCursor => $"{Csi}1J";
    public static string ClearLine => $"{Csi}2K";
    public static string ClearLineFromCursor => $"{Csi}0K";
    public static string ClearLineToCursor => $"{Csi}1K";
    
    // Alternative screen buffer
    public static string EnterAlternateScreen => $"{Csi}?1049h";
    public static string ExitAlternateScreen => $"{Csi}?1049l";
    
    // Mouse support
    public static string EnableMouse => $"{Csi}?1000h{Csi}?1002h{Csi}?1015h{Csi}?1006h";
    public static string DisableMouse => $"{Csi}?1006l{Csi}?1015l{Csi}?1002l{Csi}?1000l";
    
    // Bracketed paste
    public static string EnableBracketedPaste => $"{Csi}?2004h";
    public static string DisableBracketedPaste => $"{Csi}?2004l";
    
    // Reset
    public static string Reset => $"{Csi}0m";
    
    /// <summary>
    /// Generates ANSI escape sequence for 24-bit (true color) foreground.
    /// </summary>
    public static string SetForeground(RGBA color)
    {
        var (r, g, b, _) = color.ToInts();
        return $"{Csi}38;2;{r};{g};{b}m";
    }
    
    /// <summary>
    /// Generates ANSI escape sequence for 24-bit (true color) background.
    /// </summary>
    public static string SetBackground(RGBA color)
    {
        var (r, g, b, _) = color.ToInts();
        return $"{Csi}48;2;{r};{g};{b}m";
    }
    
    /// <summary>
    /// Generates ANSI escape sequences for text attributes.
    /// </summary>
    public static string SetAttributes(TextAttributes attrs)
    {
        if (attrs == TextAttributes.None)
            return string.Empty;
            
        var sb = new StringBuilder();
        
        if (attrs.HasFlag(TextAttributes.Bold))
            sb.Append($"{Csi}1m");
        if (attrs.HasFlag(TextAttributes.Dim))
            sb.Append($"{Csi}2m");
        if (attrs.HasFlag(TextAttributes.Italic))
            sb.Append($"{Csi}3m");
        if (attrs.HasFlag(TextAttributes.Underline))
            sb.Append($"{Csi}4m");
        if (attrs.HasFlag(TextAttributes.Blink))
            sb.Append($"{Csi}5m");
        if (attrs.HasFlag(TextAttributes.Reverse))
            sb.Append($"{Csi}7m");
        if (attrs.HasFlag(TextAttributes.Hidden))
            sb.Append($"{Csi}8m");
        if (attrs.HasFlag(TextAttributes.Strikethrough))
            sb.Append($"{Csi}9m");
            
        return sb.ToString();
    }
    
    /// <summary>
    /// Generates complete style sequence with foreground, background, and attributes.
    /// </summary>
    public static string SetStyle(RGBA? fg, RGBA? bg, TextAttributes attrs = TextAttributes.None)
    {
        var sb = new StringBuilder();
        sb.Append(Reset);
        
        if (fg.HasValue)
            sb.Append(SetForeground(fg.Value));
        if (bg.HasValue)
            sb.Append(SetBackground(bg.Value));
        if (attrs != TextAttributes.None)
            sb.Append(SetAttributes(attrs));
            
        return sb.ToString();
    }
    
    /// <summary>
    /// Strips all ANSI escape sequences from a string.
    /// </summary>
    public static string StripAnsi(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
            
        var result = new StringBuilder(text.Length);
        var i = 0;
        
        while (i < text.Length)
        {
            if (text[i] == '\x1b' && i + 1 < text.Length)
            {
                var next = text[i + 1];
                if (next == '[')
                {
                    // CSI sequence - skip until we find a letter
                    i += 2;
                    while (i < text.Length && !char.IsLetter(text[i]))
                        i++;
                    if (i < text.Length)
                        i++; // Skip the final letter
                }
                else if (next == ']')
                {
                    // OSC sequence - skip until ST or BEL
                    i += 2;
                    while (i < text.Length)
                    {
                        if (text[i] == '\x07') // BEL
                        {
                            i++;
                            break;
                        }
                        if (text[i] == '\x1b' && i + 1 < text.Length && text[i + 1] == '\\') // ST
                        {
                            i += 2;
                            break;
                        }
                        i++;
                    }
                }
                else
                {
                    i++; // Skip just the escape
                }
            }
            else
            {
                result.Append(text[i]);
                i++;
            }
        }
        
        return result.ToString();
    }
    
    /// <summary>
    /// Gets the display width of a string, ignoring ANSI sequences.
    /// </summary>
    public static int GetDisplayWidth(string text)
    {
        var stripped = StripAnsi(text);
        // For now, assume each character is width 1
        // TODO: Handle wide characters (CJK, emoji) properly
        return stripped.Length;
    }
}
