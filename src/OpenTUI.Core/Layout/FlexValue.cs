namespace OpenTUI.Core.Layout;

/// <summary>
/// Represents a dimension value that can be a fixed number, percentage, or auto.
/// </summary>
public readonly struct FlexValue : IEquatable<FlexValue>
{
    public float Value { get; }
    public FlexUnit Unit { get; }

    private FlexValue(float value, FlexUnit unit)
    {
        Value = value;
        Unit = unit;
    }

    /// <summary>Auto sizing.</summary>
    public static FlexValue Auto => new(0, FlexUnit.Auto);

    /// <summary>Undefined/unset value.</summary>
    public static FlexValue Undefined => new(float.NaN, FlexUnit.Undefined);

    /// <summary>Creates a point/pixel value.</summary>
    public static FlexValue Points(float value) => new(value, FlexUnit.Point);

    /// <summary>Creates a percentage value.</summary>
    public static FlexValue Percent(float value) => new(value, FlexUnit.Percent);

    /// <summary>Checks if this value is defined.</summary>
    public bool IsDefined => Unit != FlexUnit.Undefined && !float.IsNaN(Value);

    /// <summary>Checks if this is an auto value.</summary>
    public bool IsAuto => Unit == FlexUnit.Auto;

    /// <summary>Checks if this is a percentage value.</summary>
    public bool IsPercent => Unit == FlexUnit.Percent;

    /// <summary>Checks if this is a point/pixel value.</summary>
    public bool IsPoint => Unit == FlexUnit.Point;

    /// <summary>
    /// Resolves this value to a concrete number given a parent size.
    /// </summary>
    public float Resolve(float parentSize)
    {
        return Unit switch
        {
            FlexUnit.Point => Value,
            FlexUnit.Percent => parentSize * Value / 100f,
            _ => float.NaN
        };
    }

    /// <summary>
    /// Resolves this value or returns a default if undefined/auto.
    /// </summary>
    public float ResolveOrDefault(float parentSize, float defaultValue)
    {
        if (!IsDefined || IsAuto)
            return defaultValue;
        return Resolve(parentSize);
    }

    public bool Equals(FlexValue other)
        => Value.Equals(other.Value) && Unit == other.Unit;

    public override bool Equals(object? obj)
        => obj is FlexValue other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(Value, Unit);

    public static bool operator ==(FlexValue left, FlexValue right) => left.Equals(right);
    public static bool operator !=(FlexValue left, FlexValue right) => !left.Equals(right);

    public override string ToString()
    {
        return Unit switch
        {
            FlexUnit.Auto => "auto",
            FlexUnit.Undefined => "undefined",
            FlexUnit.Percent => $"{Value}%",
            FlexUnit.Point => $"{Value}",
            _ => $"{Value}"
        };
    }

    // Implicit conversions
    public static implicit operator FlexValue(float value) => Points(value);
    public static implicit operator FlexValue(int value) => Points(value);
}

/// <summary>
/// Unit types for flex values.
/// </summary>
public enum FlexUnit
{
    Undefined,
    Auto,
    Point,
    Percent
}

/// <summary>
/// Represents spacing values for margin, padding, and border.
/// </summary>
public struct Edges
{
    public FlexValue Top { get; set; }
    public FlexValue Right { get; set; }
    public FlexValue Bottom { get; set; }
    public FlexValue Left { get; set; }

    public Edges(FlexValue all)
    {
        Top = Right = Bottom = Left = all;
    }

    public Edges(FlexValue vertical, FlexValue horizontal)
    {
        Top = Bottom = vertical;
        Left = Right = horizontal;
    }

    public Edges(FlexValue top, FlexValue right, FlexValue bottom, FlexValue left)
    {
        Top = top;
        Right = right;
        Bottom = bottom;
        Left = left;
    }

    public static Edges Zero => new(FlexValue.Points(0));
    public static Edges Undefined => new(FlexValue.Undefined);

    /// <summary>Gets total horizontal spacing.</summary>
    public float GetHorizontal(float parentWidth)
        => Left.ResolveOrDefault(parentWidth, 0) + Right.ResolveOrDefault(parentWidth, 0);

    /// <summary>Gets total vertical spacing.</summary>
    public float GetVertical(float parentHeight)
        => Top.ResolveOrDefault(parentHeight, 0) + Bottom.ResolveOrDefault(parentHeight, 0);
}
