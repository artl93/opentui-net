using OpenTUI.Core.Colors;

namespace OpenTUI.Core.Rendering;

/// <summary>
/// Represents a single cell in the frame buffer.
/// </summary>
public struct Cell : IEquatable<Cell>
{
    /// <summary>The character to display (can be a grapheme cluster).</summary>
    public string Character { get; set; }

    /// <summary>Foreground color.</summary>
    public RGBA Foreground { get; set; }

    /// <summary>Background color.</summary>
    public RGBA Background { get; set; }

    /// <summary>Text attributes (bold, italic, etc.).</summary>
    public TextAttributes Attributes { get; set; }

    /// <summary>
    /// If greater than 1, this cell is a placeholder for a wide character.
    /// The actual character is in the cell to the left.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Creates a new cell with default values.
    /// </summary>
    public Cell()
    {
        Character = " ";
        Foreground = RGBA.White;
        Background = RGBA.Transparent;
        Attributes = TextAttributes.None;
        Width = 1;
    }

    /// <summary>
    /// Creates a new cell with the specified character and colors.
    /// </summary>
    public Cell(string character, RGBA? foreground = null, RGBA? background = null, TextAttributes attributes = TextAttributes.None)
    {
        Character = character;
        Foreground = foreground ?? RGBA.White;
        Background = background ?? RGBA.Transparent;
        Attributes = attributes;
        Width = 1;
    }

    /// <summary>
    /// Creates a placeholder cell for wide characters.
    /// </summary>
    public static Cell WidePlaceholder => new()
    {
        Character = string.Empty,
        Width = 0
    };

    /// <summary>
    /// Clears the cell to default state (space, white on transparent).
    /// </summary>
    public void Clear()
    {
        Character = " ";
        Foreground = RGBA.White;
        Background = RGBA.Transparent;
        Attributes = TextAttributes.None;
        Width = 1;
    }

    /// <summary>
    /// Clears the cell with a specific background color.
    /// </summary>
    public void Clear(RGBA background)
    {
        Character = " ";
        Foreground = RGBA.White;
        Background = background;
        Attributes = TextAttributes.None;
        Width = 1;
    }

    public bool Equals(Cell other)
        => Character == other.Character
           && Foreground == other.Foreground
           && Background == other.Background
           && Attributes == other.Attributes
           && Width == other.Width;

    public override bool Equals(object? obj) => obj is Cell other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Character, Foreground, Background, Attributes, Width);

    public static bool operator ==(Cell left, Cell right) => left.Equals(right);
    public static bool operator !=(Cell left, Cell right) => !left.Equals(right);
}
