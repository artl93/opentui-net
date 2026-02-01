using OpenTUI.Components.Components.Core;
using OpenTUI.Components.Style;
using OpenTUI.Components.Theme;
using OpenTUI.Core.Colors;
using OpenTUI.Core.Rendering;
using BorderStyle = OpenTUI.Core.Renderables.BorderStyle;

namespace OpenTUI.Components.Components.Form;

/// <summary>
/// TextField variants.
/// </summary>
public enum TextFieldVariant
{
    Normal,
    Ghost
}

/// <summary>
/// A themed text input field with label, placeholder, and validation.
/// </summary>
public class TextField : Component
{
    private string _value = "";
    private int _cursorPosition;
    private int _scrollOffset;

    /// <summary>Label displayed above the input.</summary>
    public string? Label { get; set; }

    /// <summary>Placeholder text when empty.</summary>
    public string? Placeholder { get; set; }

    /// <summary>Description text below the input.</summary>
    public string? Description { get; set; }

    /// <summary>Error message to display.</summary>
    public string? Error { get; set; }

    /// <summary>Input variant.</summary>
    public TextFieldVariant Variant { get; set; } = TextFieldVariant.Normal;

    /// <summary>Whether the field is read-only.</summary>
    public bool ReadOnly { get; set; }

    /// <summary>Whether to show a copy button.</summary>
    public bool Copyable { get; set; }

    /// <summary>Maximum input length.</summary>
    public int? MaxLength { get; set; }

    /// <summary>Input width in characters.</summary>
    public int Width { get; set; } = 30;

    /// <summary>Validation function returning error message or null if valid.</summary>
    public Func<string, string?>? Validation { get; set; }

    /// <summary>Current input value.</summary>
    public string Value
    {
        get => _value;
        set
        {
            if (_value != value)
            {
                _value = value ?? "";
                _cursorPosition = Math.Min(_cursorPosition, _value.Length);
                OnChange?.Invoke(_value);
                ValidateValue();
                MarkDirty();
            }
        }
    }

    /// <summary>Change handler.</summary>
    public Action<string>? OnChange { get; set; }

    /// <summary>Submit handler (Enter key).</summary>
    public Action<string>? OnSubmit { get; set; }

    private void ValidateValue()
    {
        if (Validation != null)
        {
            Error = Validation(_value);
        }
    }

    /// <summary>
    /// Handles a key press.
    /// </summary>
    public void HandleKey(ConsoleKeyInfo key)
    {
        if (Disabled || ReadOnly) return;

        switch (key.Key)
        {
            case ConsoleKey.Backspace:
                if (_cursorPosition > 0)
                {
                    Value = _value.Remove(_cursorPosition - 1, 1);
                    _cursorPosition--;
                }
                break;

            case ConsoleKey.Delete:
                if (_cursorPosition < _value.Length)
                {
                    Value = _value.Remove(_cursorPosition, 1);
                }
                break;

            case ConsoleKey.LeftArrow:
                if (_cursorPosition > 0) _cursorPosition--;
                MarkDirty();
                break;

            case ConsoleKey.RightArrow:
                if (_cursorPosition < _value.Length) _cursorPosition++;
                MarkDirty();
                break;

            case ConsoleKey.Home:
                _cursorPosition = 0;
                MarkDirty();
                break;

            case ConsoleKey.End:
                _cursorPosition = _value.Length;
                MarkDirty();
                break;

            case ConsoleKey.Enter:
                OnSubmit?.Invoke(_value);
                break;

            default:
                if (!char.IsControl(key.KeyChar))
                {
                    if (MaxLength == null || _value.Length < MaxLength)
                    {
                        Value = _value.Insert(_cursorPosition, key.KeyChar.ToString());
                        _cursorPosition++;
                    }
                }
                break;
        }
    }

    protected override void RenderSelf(FrameBuffer buffer, int x, int y, int width, int height)
    {
        var currentY = y;

        var hasError = !string.IsNullOrEmpty(Error);
        var inputBg = Variant == TextFieldVariant.Ghost
            ? RGBA.Transparent
            : GetColor(ColorToken.InputBase);
        var borderColor = hasError
            ? GetColor(ColorToken.BorderCritical)
            : Focused
                ? GetColor(ColorToken.BorderSelected)
                : GetColor(ColorToken.BorderBase);

        // Label
        if (!string.IsNullOrEmpty(Label))
        {
            buffer.DrawText(Label, x, currentY, GetColor(ColorToken.TextWeak));
            currentY++;
        }

        // Input box
        var inputWidth = Width + 2; // +2 for borders
        var inputHeight = 1;

        if (Variant == TextFieldVariant.Normal)
        {
            // Background
            buffer.FillRect(x, currentY, inputWidth, inputHeight + 2, inputBg);

            // Border
            DrawThemedBox(buffer, x, currentY, inputWidth, inputHeight + 2,
                background: hasError ? GetColor(ColorToken.CriticalWeak) : inputBg,
                borderColor: borderColor,
                borderStyle: BorderStyle.Rounded);

            currentY++; // Move inside border
        }

        // Input content
        var contentX = x + 1;
        var displayWidth = Width;

        // Adjust scroll offset to keep cursor visible
        if (_cursorPosition - _scrollOffset >= displayWidth)
        {
            _scrollOffset = _cursorPosition - displayWidth + 1;
        }
        else if (_cursorPosition < _scrollOffset)
        {
            _scrollOffset = _cursorPosition;
        }

        // Display text or placeholder
        var displayText = string.IsNullOrEmpty(_value) && !Focused
            ? Placeholder ?? ""
            : _value;
        var textColor = string.IsNullOrEmpty(_value) && !Focused
            ? GetColor(ColorToken.TextWeak)
            : GetColor(ColorToken.TextStrong);

        if (Disabled) textColor = GetColor(ColorToken.TextDisabled);

        var visibleText = displayText.Length > _scrollOffset
            ? displayText.Substring(_scrollOffset, Math.Min(displayWidth, displayText.Length - _scrollOffset))
            : "";

        buffer.DrawText(visibleText.PadRight(displayWidth), contentX, currentY, textColor);

        // Cursor
        if (Focused && !ReadOnly)
        {
            var cursorX = contentX + (_cursorPosition - _scrollOffset);
            if (cursorX < contentX + displayWidth)
            {
                var cursorChar = _cursorPosition < _value.Length ? _value[_cursorPosition].ToString() : " ";
                buffer.SetCell(cursorX, currentY, new Cell(cursorChar, inputBg, GetColor(ColorToken.TextStrong)));
            }
        }

        // Copy button hint
        if (Copyable && !string.IsNullOrEmpty(_value))
        {
            buffer.DrawText("📋", x + inputWidth + 1, currentY, GetColor(ColorToken.IconWeak));
        }

        currentY++;

        if (Variant == TextFieldVariant.Normal)
        {
            currentY++; // Bottom border
        }

        // Description
        if (!string.IsNullOrEmpty(Description) && string.IsNullOrEmpty(Error))
        {
            buffer.DrawText(Description, x, currentY, GetColor(ColorToken.TextWeak));
            currentY++;
        }

        // Error message
        if (hasError)
        {
            buffer.DrawText(Error!, x, currentY, GetColor(ColorToken.CriticalBase));
        }
    }
}
