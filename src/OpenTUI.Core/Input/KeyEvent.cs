namespace OpenTUI.Core.Input;

/// <summary>
/// Represents a keyboard key.
/// </summary>
public enum Key
{
    None = 0,

    // Letters
    A, B, C, D, E, F, G, H, I, J, K, L, M,
    N, O, P, Q, R, S, T, U, V, W, X, Y, Z,

    // Numbers
    D0, D1, D2, D3, D4, D5, D6, D7, D8, D9,

    // Function keys
    F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12,

    // Navigation
    Up, Down, Left, Right,
    Home, End, PageUp, PageDown,

    // Editing
    Backspace, Delete, Insert,
    Enter, Tab, Escape, Space,

    // Punctuation
    Comma, Period, Semicolon, Quote,
    LeftBracket, RightBracket, Backslash, Slash,
    Minus, Equals, Backtick,

    // Special
    PrintScreen, ScrollLock, Pause,
    NumLock, CapsLock,

    // Numpad
    Numpad0, Numpad1, Numpad2, Numpad3, Numpad4,
    Numpad5, Numpad6, Numpad7, Numpad8, Numpad9,
    NumpadAdd, NumpadSubtract, NumpadMultiply, NumpadDivide,
    NumpadDecimal, NumpadEnter,

    // Unknown/unrecognized
    Unknown
}

/// <summary>
/// Keyboard modifier keys.
/// </summary>
[Flags]
public enum KeyModifiers
{
    None = 0,
    Shift = 1,
    Alt = 2,
    Control = 4,
    Meta = 8  // Windows key / Command key
}

/// <summary>
/// Represents a keyboard event.
/// </summary>
public readonly record struct KeyEvent
{
    /// <summary>The key that was pressed.</summary>
    public Key Key { get; init; }

    /// <summary>The character representation, if printable.</summary>
    public char? Character { get; init; }

    /// <summary>Modifier keys held during the event.</summary>
    public KeyModifiers Modifiers { get; init; }

    /// <summary>Whether this is a key down (press) event.</summary>
    public bool IsKeyDown { get; init; }

    /// <summary>Whether Shift is held.</summary>
    public bool Shift => (Modifiers & KeyModifiers.Shift) != 0;

    /// <summary>Whether Alt is held.</summary>
    public bool Alt => (Modifiers & KeyModifiers.Alt) != 0;

    /// <summary>Whether Control is held.</summary>
    public bool Control => (Modifiers & KeyModifiers.Control) != 0;

    /// <summary>Whether Meta (Windows/Command) is held.</summary>
    public bool Meta => (Modifiers & KeyModifiers.Meta) != 0;

    /// <summary>Whether this event represents a printable character.</summary>
    public bool IsPrintable => Character.HasValue && !char.IsControl(Character.Value);

    public KeyEvent(Key key, KeyModifiers modifiers = KeyModifiers.None, char? character = null, bool isKeyDown = true)
    {
        Key = key;
        Modifiers = modifiers;
        Character = character;
        IsKeyDown = isKeyDown;
    }

    /// <summary>Creates a key event from a character.</summary>
    public static KeyEvent FromChar(char c, KeyModifiers modifiers = KeyModifiers.None)
    {
        var key = CharToKey(c);
        return new KeyEvent(key, modifiers, c);
    }

    /// <summary>Creates a key event for a special key.</summary>
    public static KeyEvent FromKey(Key key, KeyModifiers modifiers = KeyModifiers.None)
    {
        return new KeyEvent(key, modifiers);
    }

    private static Key CharToKey(char c)
    {
        return c switch
        {
            >= 'a' and <= 'z' => (Key)((int)Key.A + (c - 'a')),
            >= 'A' and <= 'Z' => (Key)((int)Key.A + (c - 'A')),
            >= '0' and <= '9' => (Key)((int)Key.D0 + (c - '0')),
            ' ' => Key.Space,
            '\t' => Key.Tab,
            '\r' or '\n' => Key.Enter,
            '\x1b' => Key.Escape,
            '\x7f' or '\b' => Key.Backspace,
            ',' => Key.Comma,
            '.' => Key.Period,
            ';' => Key.Semicolon,
            '\'' => Key.Quote,
            '[' => Key.LeftBracket,
            ']' => Key.RightBracket,
            '\\' => Key.Backslash,
            '/' => Key.Slash,
            '-' => Key.Minus,
            '=' => Key.Equals,
            '`' => Key.Backtick,
            _ => Key.Unknown
        };
    }

    public override string ToString()
    {
        var mods = new List<string>();
        if (Control) mods.Add("Ctrl");
        if (Alt) mods.Add("Alt");
        if (Shift) mods.Add("Shift");
        if (Meta) mods.Add("Meta");

        var keyStr = Character.HasValue && IsPrintable
            ? $"'{Character}'"
            : Key.ToString();

        return mods.Count > 0
            ? $"{string.Join("+", mods)}+{keyStr}"
            : keyStr;
    }
}
