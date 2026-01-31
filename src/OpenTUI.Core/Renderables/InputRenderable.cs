using OpenTUI.Core.Colors;
using OpenTUI.Core.Rendering;

namespace OpenTUI.Core.Renderables;

/// <summary>
/// A text input field with cursor support.
/// </summary>
public class InputRenderable : Renderable
{
    private string _value = string.Empty;
    private string _placeholder = string.Empty;
    private int _cursorPosition;
    private bool _password;
    private char _passwordChar = '*';
    private int _scrollOffset;

    /// <summary>The current input value.</summary>
    public string Value
    {
        get => _value;
        set
        {
            var newValue = value ?? string.Empty;
            if (_value != newValue)
            {
                _value = newValue;
                _cursorPosition = Math.Min(_cursorPosition, _value.Length);
                MarkDirty();
                ValueChanged?.Invoke(this, _value);
            }
        }
    }

    /// <summary>Placeholder text shown when value is empty.</summary>
    public string Placeholder
    {
        get => _placeholder;
        set
        {
            if (_placeholder != value)
            {
                _placeholder = value ?? string.Empty;
                MarkDirty();
            }
        }
    }

    /// <summary>Current cursor position within the value.</summary>
    public int CursorPosition
    {
        get => _cursorPosition;
        set
        {
            var newPos = Math.Clamp(value, 0, _value.Length);
            if (_cursorPosition != newPos)
            {
                _cursorPosition = newPos;
                EnsureCursorVisible();
                MarkDirty();
            }
        }
    }

    /// <summary>Whether to mask input as a password field.</summary>
    public bool Password
    {
        get => _password;
        set
        {
            if (_password != value)
            {
                _password = value;
                MarkDirty();
            }
        }
    }

    /// <summary>Character used to mask password input.</summary>
    public char PasswordChar
    {
        get => _passwordChar;
        set
        {
            if (_passwordChar != value)
            {
                _passwordChar = value;
                MarkDirty();
            }
        }
    }

    /// <summary>Color for placeholder text.</summary>
    public RGBA PlaceholderColor { get; set; } = RGBA.FromValues(0.5f, 0.5f, 0.5f);

    /// <summary>Color for the cursor.</summary>
    public RGBA CursorColor { get; set; } = RGBA.White;

    /// <summary>Event raised when the value changes.</summary>
    public event EventHandler<string>? ValueChanged;

    /// <summary>Event raised when Enter is pressed.</summary>
    public event EventHandler<string>? Submitted;

    public InputRenderable()
    {
        Focusable = true;
    }

    /// <summary>Inserts text at the cursor position.</summary>
    public void Insert(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        _value = _value.Insert(_cursorPosition, text);
        _cursorPosition += text.Length;
        EnsureCursorVisible();
        MarkDirty();
        ValueChanged?.Invoke(this, _value);
    }

    /// <summary>Inserts a character at the cursor position.</summary>
    public void Insert(char ch)
    {
        Insert(ch.ToString());
    }

    /// <summary>Deletes the character before the cursor (backspace).</summary>
    public void Backspace()
    {
        if (_cursorPosition > 0)
        {
            _value = _value.Remove(_cursorPosition - 1, 1);
            _cursorPosition--;
            EnsureCursorVisible();
            MarkDirty();
            ValueChanged?.Invoke(this, _value);
        }
    }

    /// <summary>Deletes the character at the cursor position (delete key).</summary>
    public void Delete()
    {
        if (_cursorPosition < _value.Length)
        {
            _value = _value.Remove(_cursorPosition, 1);
            MarkDirty();
            ValueChanged?.Invoke(this, _value);
        }
    }

    /// <summary>Moves the cursor left.</summary>
    public void MoveCursorLeft()
    {
        if (_cursorPosition > 0)
        {
            _cursorPosition--;
            EnsureCursorVisible();
            MarkDirty();
        }
    }

    /// <summary>Moves the cursor right.</summary>
    public void MoveCursorRight()
    {
        if (_cursorPosition < _value.Length)
        {
            _cursorPosition++;
            EnsureCursorVisible();
            MarkDirty();
        }
    }

    /// <summary>Moves the cursor to the beginning.</summary>
    public void MoveCursorHome()
    {
        if (_cursorPosition != 0)
        {
            _cursorPosition = 0;
            _scrollOffset = 0;
            MarkDirty();
        }
    }

    /// <summary>Moves the cursor to the end.</summary>
    public void MoveCursorEnd()
    {
        if (_cursorPosition != _value.Length)
        {
            _cursorPosition = _value.Length;
            EnsureCursorVisible();
            MarkDirty();
        }
    }

    /// <summary>Clears the input.</summary>
    public new void Clear()
    {
        if (_value.Length > 0)
        {
            _value = string.Empty;
            _cursorPosition = 0;
            _scrollOffset = 0;
            MarkDirty();
            ValueChanged?.Invoke(this, _value);
        }
    }

    /// <summary>Submits the current value.</summary>
    public void Submit()
    {
        Submitted?.Invoke(this, _value);
    }

    protected override void RenderSelf(FrameBuffer buffer, int x, int y, int width, int height)
    {
        if (width <= 0 || height <= 0) return;

        var fg = ForegroundColor ?? RGBA.White;
        var bg = BackgroundColor ?? buffer.GetCell(y, x).Background;

        // Get display text
        string displayText;
        RGBA textColor;

        if (string.IsNullOrEmpty(_value) && !IsFocused)
        {
            displayText = _placeholder;
            textColor = PlaceholderColor;
        }
        else
        {
            displayText = _password ? new string(_passwordChar, _value.Length) : _value;
            textColor = fg;
        }

        // Calculate visible portion
        var visibleText = displayText.Length > _scrollOffset 
            ? displayText[_scrollOffset..] 
            : string.Empty;
        if (visibleText.Length > width)
            visibleText = visibleText[..width];

        // Draw text
        for (int i = 0; i < width; i++)
        {
            var ch = i < visibleText.Length ? visibleText[i].ToString() : " ";
            var cellBg = bg;
            var cellFg = textColor;

            // Highlight cursor position when focused
            if (IsFocused && i == _cursorPosition - _scrollOffset)
            {
                cellBg = CursorColor;
                cellFg = bg;
            }

            buffer.SetCell(x + i, y, new Cell(ch, cellFg, cellBg));
        }
    }

    private void EnsureCursorVisible()
    {
        var width = (int)Layout.Layout.Width;
        if (width <= 0) return;

        if (_cursorPosition < _scrollOffset)
        {
            _scrollOffset = _cursorPosition;
        }
        else if (_cursorPosition >= _scrollOffset + width)
        {
            _scrollOffset = _cursorPosition - width + 1;
        }
    }
}
