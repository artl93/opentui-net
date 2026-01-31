namespace OpenTUI.Core.Rendering;

/// <summary>
/// Text attributes for styling (can be combined with bitwise OR).
/// </summary>
[Flags]
public enum TextAttributes
{
    None = 0,
    Bold = 1 << 0,
    Dim = 1 << 1,
    Italic = 1 << 2,
    Underline = 1 << 3,
    Blink = 1 << 4,
    Reverse = 1 << 5,
    Hidden = 1 << 6,
    Strikethrough = 1 << 7,
}
