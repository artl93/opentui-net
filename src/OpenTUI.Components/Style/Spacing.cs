using OpenTUI.Core.Colors;

namespace OpenTUI.Components.Style;

/// <summary>
/// Represents spacing (padding/margin) with support for shorthand notation.
/// </summary>
public readonly struct Spacing : IEquatable<Spacing>
{
    public int Top { get; }
    public int Right { get; }
    public int Bottom { get; }
    public int Left { get; }

    /// <summary>Creates spacing with all sides equal.</summary>
    public Spacing(int all) : this(all, all, all, all) { }
    
    /// <summary>Creates spacing with vertical and horizontal values.</summary>
    public Spacing(int vertical, int horizontal) : this(vertical, horizontal, vertical, horizontal) { }
    
    /// <summary>Creates spacing with all four sides specified.</summary>
    public Spacing(int top, int right, int bottom, int left)
    {
        Top = top;
        Right = right;
        Bottom = bottom;
        Left = left;
    }

    /// <summary>No spacing.</summary>
    public static Spacing Zero => new(0);
    
    /// <summary>Horizontal spacing total.</summary>
    public int Horizontal => Left + Right;
    
    /// <summary>Vertical spacing total.</summary>
    public int Vertical => Top + Bottom;

    // Implicit conversions for ergonomic usage
    public static implicit operator Spacing(int all) => new(all);
    public static implicit operator Spacing((int vertical, int horizontal) v) => new(v.vertical, v.horizontal);
    public static implicit operator Spacing((int top, int right, int bottom, int left) v) => 
        new(v.top, v.right, v.bottom, v.left);

    public bool Equals(Spacing other) => 
        Top == other.Top && Right == other.Right && Bottom == other.Bottom && Left == other.Left;
    
    public override bool Equals(object? obj) => obj is Spacing other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Top, Right, Bottom, Left);
    public static bool operator ==(Spacing left, Spacing right) => left.Equals(right);
    public static bool operator !=(Spacing left, Spacing right) => !left.Equals(right);
    
    public override string ToString() => 
        Top == Right && Right == Bottom && Bottom == Left 
            ? $"{Top}" 
            : Top == Bottom && Left == Right 
                ? $"{Top} {Right}" 
                : $"{Top} {Right} {Bottom} {Left}";
}
