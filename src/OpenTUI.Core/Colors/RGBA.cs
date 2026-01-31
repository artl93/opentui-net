namespace OpenTUI.Core.Colors;

/// <summary>
/// Represents a color with red, green, blue, and alpha components.
/// Values are stored as normalized floats (0.0-1.0) for efficient processing.
/// </summary>
public readonly struct RGBA : IEquatable<RGBA>
{
    /// <summary>Red component (0.0-1.0)</summary>
    public float R { get; }
    
    /// <summary>Green component (0.0-1.0)</summary>
    public float G { get; }
    
    /// <summary>Blue component (0.0-1.0)</summary>
    public float B { get; }
    
    /// <summary>Alpha component (0.0-1.0), where 1.0 is fully opaque</summary>
    public float A { get; }

    private RGBA(float r, float g, float b, float a)
    {
        R = Math.Clamp(r, 0f, 1f);
        G = Math.Clamp(g, 0f, 1f);
        B = Math.Clamp(b, 0f, 1f);
        A = Math.Clamp(a, 0f, 1f);
    }

    /// <summary>
    /// Creates an RGBA color from normalized float values (0.0-1.0).
    /// </summary>
    public static RGBA FromValues(float r, float g, float b, float a = 1f)
        => new(r, g, b, a);

    /// <summary>
    /// Creates an RGBA color from integer values (0-255).
    /// </summary>
    public static RGBA FromInts(int r, int g, int b, int a = 255)
        => new(r / 255f, g / 255f, b / 255f, a / 255f);

    /// <summary>
    /// Creates an RGBA color from a hex string.
    /// Supports formats: #RGB, #RGBA, #RRGGBB, #RRGGBBAA (with or without #)
    /// </summary>
    public static RGBA FromHex(string hex)
    {
        ArgumentNullException.ThrowIfNull(hex);
        
        var span = hex.AsSpan();
        if (span.Length > 0 && span[0] == '#')
            span = span[1..];

        return span.Length switch
        {
            3 => ParseShortHex(span, 255),
            4 => ParseShortHexWithAlpha(span),
            6 => ParseLongHex(span, 255),
            8 => ParseLongHexWithAlpha(span),
            _ => throw new FormatException($"Invalid hex color format: {hex}")
        };
    }

    /// <summary>
    /// Attempts to parse a hex string into an RGBA color.
    /// </summary>
    public static bool TryFromHex(string? hex, out RGBA result)
    {
        result = default;
        if (string.IsNullOrEmpty(hex))
            return false;

        try
        {
            result = FromHex(hex);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Parses a color from various formats: hex strings, CSS color names, or "transparent".
    /// </summary>
    public static RGBA Parse(string color)
    {
        ArgumentNullException.ThrowIfNull(color);
        
        var trimmed = color.Trim().ToLowerInvariant();
        
        if (trimmed == "transparent")
            return Transparent;

        if (trimmed.StartsWith('#') || IsHexString(trimmed))
            return FromHex(trimmed);

        if (CssColors.TryGetValue(trimmed, out var cssColor))
            return cssColor;

        throw new FormatException($"Unknown color format: {color}");
    }

    /// <summary>
    /// Attempts to parse a color string.
    /// </summary>
    public static bool TryParse(string? color, out RGBA result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(color))
            return false;

        try
        {
            result = Parse(color);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Blends this color over another using alpha compositing.
    /// </summary>
    public RGBA BlendOver(RGBA background)
    {
        if (A >= 1f) return this;
        if (A <= 0f) return background;

        float outA = A + background.A * (1 - A);
        if (outA <= 0f) return Transparent;

        float outR = (R * A + background.R * background.A * (1 - A)) / outA;
        float outG = (G * A + background.G * background.A * (1 - A)) / outA;
        float outB = (B * A + background.B * background.A * (1 - A)) / outA;

        return new RGBA(outR, outG, outB, outA);
    }

    /// <summary>
    /// Returns the color with a new alpha value.
    /// </summary>
    public RGBA WithAlpha(float alpha) => new(R, G, B, alpha);

    /// <summary>
    /// Converts to integer RGB values (0-255).
    /// </summary>
    public (int R, int G, int B, int A) ToInts()
        => ((int)(R * 255), (int)(G * 255), (int)(B * 255), (int)(A * 255));

    /// <summary>
    /// Converts to a hex string in #RRGGBB or #RRGGBBAA format.
    /// </summary>
    public string ToHex(bool includeAlpha = false)
    {
        var (r, g, b, a) = ToInts();
        return includeAlpha
            ? $"#{r:X2}{g:X2}{b:X2}{a:X2}"
            : $"#{r:X2}{g:X2}{b:X2}";
    }

    // Predefined colors
    public static RGBA Transparent => new(0, 0, 0, 0);
    public static RGBA Black => new(0, 0, 0, 1);
    public static RGBA White => new(1, 1, 1, 1);
    public static RGBA Red => new(1, 0, 0, 1);
    public static RGBA Green => new(0, 1, 0, 1);
    public static RGBA Blue => new(0, 0, 1, 1);
    public static RGBA Yellow => new(1, 1, 0, 1);
    public static RGBA Cyan => new(0, 1, 1, 1);
    public static RGBA Magenta => new(1, 0, 1, 1);

    // IEquatable implementation
    public bool Equals(RGBA other)
        => R == other.R && G == other.G && B == other.B && A == other.A;

    public override bool Equals(object? obj)
        => obj is RGBA other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(R, G, B, A);

    public static bool operator ==(RGBA left, RGBA right) => left.Equals(right);
    public static bool operator !=(RGBA left, RGBA right) => !left.Equals(right);

    public override string ToString() => ToHex(A < 1f);

    // Private helpers
    private static bool IsHexString(string s)
        => s.Length is 3 or 4 or 6 or 8 && s.All(c => char.IsAsciiHexDigit(c));

    private static RGBA ParseShortHex(ReadOnlySpan<char> span, int alpha)
    {
        int r = ParseHexDigit(span[0]) * 17;
        int g = ParseHexDigit(span[1]) * 17;
        int b = ParseHexDigit(span[2]) * 17;
        return FromInts(r, g, b, alpha);
    }

    private static RGBA ParseShortHexWithAlpha(ReadOnlySpan<char> span)
    {
        int r = ParseHexDigit(span[0]) * 17;
        int g = ParseHexDigit(span[1]) * 17;
        int b = ParseHexDigit(span[2]) * 17;
        int a = ParseHexDigit(span[3]) * 17;
        return FromInts(r, g, b, a);
    }

    private static RGBA ParseLongHex(ReadOnlySpan<char> span, int alpha)
    {
        int r = ParseHexByte(span[0..2]);
        int g = ParseHexByte(span[2..4]);
        int b = ParseHexByte(span[4..6]);
        return FromInts(r, g, b, alpha);
    }

    private static RGBA ParseLongHexWithAlpha(ReadOnlySpan<char> span)
    {
        int r = ParseHexByte(span[0..2]);
        int g = ParseHexByte(span[2..4]);
        int b = ParseHexByte(span[4..6]);
        int a = ParseHexByte(span[6..8]);
        return FromInts(r, g, b, a);
    }

    private static int ParseHexDigit(char c)
        => c switch
        {
            >= '0' and <= '9' => c - '0',
            >= 'a' and <= 'f' => c - 'a' + 10,
            >= 'A' and <= 'F' => c - 'A' + 10,
            _ => throw new FormatException($"Invalid hex digit: {c}")
        };

    private static int ParseHexByte(ReadOnlySpan<char> span)
        => ParseHexDigit(span[0]) * 16 + ParseHexDigit(span[1]);

    // CSS named colors (subset of most common)
    private static readonly Dictionary<string, RGBA> CssColors = new()
    {
        ["black"] = FromInts(0, 0, 0),
        ["white"] = FromInts(255, 255, 255),
        ["red"] = FromInts(255, 0, 0),
        ["green"] = FromInts(0, 128, 0),
        ["blue"] = FromInts(0, 0, 255),
        ["yellow"] = FromInts(255, 255, 0),
        ["cyan"] = FromInts(0, 255, 255),
        ["magenta"] = FromInts(255, 0, 255),
        ["gray"] = FromInts(128, 128, 128),
        ["grey"] = FromInts(128, 128, 128),
        ["silver"] = FromInts(192, 192, 192),
        ["maroon"] = FromInts(128, 0, 0),
        ["olive"] = FromInts(128, 128, 0),
        ["lime"] = FromInts(0, 255, 0),
        ["aqua"] = FromInts(0, 255, 255),
        ["teal"] = FromInts(0, 128, 128),
        ["navy"] = FromInts(0, 0, 128),
        ["fuchsia"] = FromInts(255, 0, 255),
        ["purple"] = FromInts(128, 0, 128),
        ["orange"] = FromInts(255, 165, 0),
        ["pink"] = FromInts(255, 192, 203),
        ["brown"] = FromInts(165, 42, 42),
        ["coral"] = FromInts(255, 127, 80),
        ["crimson"] = FromInts(220, 20, 60),
        ["gold"] = FromInts(255, 215, 0),
        ["indigo"] = FromInts(75, 0, 130),
        ["ivory"] = FromInts(255, 255, 240),
        ["khaki"] = FromInts(240, 230, 140),
        ["lavender"] = FromInts(230, 230, 250),
        ["salmon"] = FromInts(250, 128, 114),
        ["skyblue"] = FromInts(135, 206, 235),
        ["slategray"] = FromInts(112, 128, 144),
        ["tan"] = FromInts(210, 180, 140),
        ["tomato"] = FromInts(255, 99, 71),
        ["turquoise"] = FromInts(64, 224, 208),
        ["violet"] = FromInts(238, 130, 238),
        ["wheat"] = FromInts(245, 222, 179),
    };
}
