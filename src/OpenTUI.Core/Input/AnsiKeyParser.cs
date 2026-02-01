namespace OpenTUI.Core.Input;

/// <summary>
/// Parses ANSI escape sequences into key events.
/// </summary>
public static class AnsiKeyParser
{
    /// <summary>
    /// Parses an ANSI escape sequence or character into a KeyEvent.
    /// </summary>
    public static KeyEvent? Parse(ReadOnlySpan<char> input)
    {
        if (input.IsEmpty)
            return null;

        // Single character
        if (input.Length == 1)
            return ParseSingleChar(input[0]);

        // Escape sequence
        if (input[0] == '\x1b')
            return ParseEscapeSequence(input);

        // First character as fallback
        return ParseSingleChar(input[0]);
    }

    /// <summary>
    /// Parses a ConsoleKeyInfo into a KeyEvent.
    /// </summary>
    public static KeyEvent FromConsoleKeyInfo(ConsoleKeyInfo keyInfo)
    {
        var modifiers = KeyModifiers.None;
        if ((keyInfo.Modifiers & ConsoleModifiers.Shift) != 0)
            modifiers |= KeyModifiers.Shift;
        if ((keyInfo.Modifiers & ConsoleModifiers.Alt) != 0)
            modifiers |= KeyModifiers.Alt;
        if ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0)
            modifiers |= KeyModifiers.Control;

        var key = ConsoleKeyToKey(keyInfo.Key);
        var ch = keyInfo.KeyChar != '\0' ? keyInfo.KeyChar : (char?)null;

        return new KeyEvent(key, modifiers, ch);
    }

    private static KeyEvent ParseSingleChar(char c)
    {
        // Control characters
        if (c >= 1 && c <= 26)
        {
            // Ctrl+A through Ctrl+Z
            var key = (Key)((int)Key.A + (c - 1));
            return new KeyEvent(key, KeyModifiers.Control);
        }

        if (c == 0)
            return new KeyEvent(Key.None);

        if (c == 27)
            return new KeyEvent(Key.Escape);

        if (c == 127 || c == 8)
            return new KeyEvent(Key.Backspace);

        return KeyEvent.FromChar(c);
    }

    private static KeyEvent? ParseEscapeSequence(ReadOnlySpan<char> input)
    {
        if (input.Length < 2)
            return new KeyEvent(Key.Escape);

        // Alt+key (Escape followed by character)
        if (input.Length == 2 && input[1] != '[' && input[1] != 'O')
        {
            var keyEvent = ParseSingleChar(input[1]);
            return new KeyEvent(keyEvent.Key, keyEvent.Modifiers | KeyModifiers.Alt, keyEvent.Character);
        }

        // CSI sequences: ESC [ ...
        if (input[1] == '[')
            return ParseCsiSequence(input[2..]);

        // SS3 sequences: ESC O ... (function keys on some terminals)
        if (input[1] == 'O')
            return ParseSs3Sequence(input[2..]);

        return new KeyEvent(Key.Escape);
    }

    private static KeyEvent? ParseCsiSequence(ReadOnlySpan<char> input)
    {
        if (input.IsEmpty)
            return null;

        // Simple arrow keys and navigation
        if (input.Length == 1)
        {
            return input[0] switch
            {
                'A' => new KeyEvent(Key.Up),
                'B' => new KeyEvent(Key.Down),
                'C' => new KeyEvent(Key.Right),
                'D' => new KeyEvent(Key.Left),
                'H' => new KeyEvent(Key.Home),
                'F' => new KeyEvent(Key.End),
                'Z' => new KeyEvent(Key.Tab, KeyModifiers.Shift), // Shift+Tab
                _ => null
            };
        }

        // Tilde sequences: ESC [ n ~
        if (input[^1] == '~')
        {
            var numStr = input[..^1];
            if (int.TryParse(numStr, out var num))
            {
                return num switch
                {
                    1 => new KeyEvent(Key.Home),
                    2 => new KeyEvent(Key.Insert),
                    3 => new KeyEvent(Key.Delete),
                    4 => new KeyEvent(Key.End),
                    5 => new KeyEvent(Key.PageUp),
                    6 => new KeyEvent(Key.PageDown),
                    7 => new KeyEvent(Key.Home),
                    8 => new KeyEvent(Key.End),
                    11 => new KeyEvent(Key.F1),
                    12 => new KeyEvent(Key.F2),
                    13 => new KeyEvent(Key.F3),
                    14 => new KeyEvent(Key.F4),
                    15 => new KeyEvent(Key.F5),
                    17 => new KeyEvent(Key.F6),
                    18 => new KeyEvent(Key.F7),
                    19 => new KeyEvent(Key.F8),
                    20 => new KeyEvent(Key.F9),
                    21 => new KeyEvent(Key.F10),
                    23 => new KeyEvent(Key.F11),
                    24 => new KeyEvent(Key.F12),
                    _ => null
                };
            }

            // Modifier sequences: ESC [ n ; m ~
            var parts = numStr.ToString().Split(';');
            if (parts.Length == 2 && int.TryParse(parts[0], out var keyNum) && int.TryParse(parts[1], out var modNum))
            {
                var modifiers = ParseModifierNumber(modNum);
                var baseEvent = ParseCsiSequence($"{keyNum}~".AsSpan());
                if (baseEvent.HasValue)
                    return new KeyEvent(baseEvent.Value.Key, modifiers);
            }
        }

        // Modified arrow keys: ESC [ 1 ; m A/B/C/D
        if (input.Length >= 3 && input[0] == '1' && input[1] == ';')
        {
            var remaining = input[2..];
            if (remaining.Length >= 2 && char.IsDigit(remaining[0]))
            {
                var modNum = remaining[0] - '0';
                var modifiers = ParseModifierNumber(modNum);
                var key = remaining[1] switch
                {
                    'A' => Key.Up,
                    'B' => Key.Down,
                    'C' => Key.Right,
                    'D' => Key.Left,
                    'H' => Key.Home,
                    'F' => Key.End,
                    _ => Key.Unknown
                };
                if (key != Key.Unknown)
                    return new KeyEvent(key, modifiers);
            }
        }

        return null;
    }

    private static KeyEvent? ParseSs3Sequence(ReadOnlySpan<char> input)
    {
        if (input.IsEmpty)
            return null;

        // SS3 function keys (some terminals)
        return input[0] switch
        {
            'P' => new KeyEvent(Key.F1),
            'Q' => new KeyEvent(Key.F2),
            'R' => new KeyEvent(Key.F3),
            'S' => new KeyEvent(Key.F4),
            'A' => new KeyEvent(Key.Up),
            'B' => new KeyEvent(Key.Down),
            'C' => new KeyEvent(Key.Right),
            'D' => new KeyEvent(Key.Left),
            'H' => new KeyEvent(Key.Home),
            'F' => new KeyEvent(Key.End),
            _ => null
        };
    }

    private static KeyModifiers ParseModifierNumber(int modNum)
    {
        // Modifier encoding: 1 + (shift ? 1 : 0) + (alt ? 2 : 0) + (ctrl ? 4 : 0) + (meta ? 8 : 0)
        var modifiers = KeyModifiers.None;
        modNum -= 1; // Remove base 1

        if ((modNum & 1) != 0) modifiers |= KeyModifiers.Shift;
        if ((modNum & 2) != 0) modifiers |= KeyModifiers.Alt;
        if ((modNum & 4) != 0) modifiers |= KeyModifiers.Control;
        if ((modNum & 8) != 0) modifiers |= KeyModifiers.Meta;

        return modifiers;
    }

    private static Key ConsoleKeyToKey(ConsoleKey consoleKey)
    {
        return consoleKey switch
        {
            >= ConsoleKey.A and <= ConsoleKey.Z => (Key)((int)Key.A + (consoleKey - ConsoleKey.A)),
            >= ConsoleKey.D0 and <= ConsoleKey.D9 => (Key)((int)Key.D0 + (consoleKey - ConsoleKey.D0)),
            >= ConsoleKey.NumPad0 and <= ConsoleKey.NumPad9 => (Key)((int)Key.Numpad0 + (consoleKey - ConsoleKey.NumPad0)),
            >= ConsoleKey.F1 and <= ConsoleKey.F12 => (Key)((int)Key.F1 + (consoleKey - ConsoleKey.F1)),
            ConsoleKey.UpArrow => Key.Up,
            ConsoleKey.DownArrow => Key.Down,
            ConsoleKey.LeftArrow => Key.Left,
            ConsoleKey.RightArrow => Key.Right,
            ConsoleKey.Home => Key.Home,
            ConsoleKey.End => Key.End,
            ConsoleKey.PageUp => Key.PageUp,
            ConsoleKey.PageDown => Key.PageDown,
            ConsoleKey.Insert => Key.Insert,
            ConsoleKey.Delete => Key.Delete,
            ConsoleKey.Backspace => Key.Backspace,
            ConsoleKey.Tab => Key.Tab,
            ConsoleKey.Enter => Key.Enter,
            ConsoleKey.Escape => Key.Escape,
            ConsoleKey.Spacebar => Key.Space,
            _ => Key.Unknown
        };
    }
}
