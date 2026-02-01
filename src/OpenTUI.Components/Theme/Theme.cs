using OpenTUI.Core.Colors;

namespace OpenTUI.Components.Theme;

/// <summary>
/// A theme defines the color palette for all components.
/// </summary>
public class Theme
{
    private readonly Dictionary<ColorToken, RGBA> _colors;
    
    /// <summary>Theme name for identification.</summary>
    public string Name { get; }
    
    /// <summary>Whether this is a dark theme.</summary>
    public bool IsDark { get; }

    /// <summary>
    /// Creates a new theme with the specified color mappings.
    /// </summary>
    public Theme(string name, bool isDark, Dictionary<ColorToken, RGBA> colors)
    {
        Name = name;
        IsDark = isDark;
        _colors = new Dictionary<ColorToken, RGBA>(colors);
    }

    /// <summary>
    /// Gets the color for a specific token.
    /// </summary>
    public RGBA this[ColorToken token] => _colors.TryGetValue(token, out var color) 
        ? color 
        : IsDark ? RGBA.White : RGBA.Black;

    /// <summary>
    /// Gets the color for a token, with a fallback.
    /// </summary>
    public RGBA GetColor(ColorToken token, RGBA? fallback = null)
    {
        if (_colors.TryGetValue(token, out var color))
            return color;
        return fallback ?? (IsDark ? RGBA.White : RGBA.Black);
    }

    /// <summary>
    /// Creates a new theme by overriding specific tokens.
    /// </summary>
    public Theme WithOverrides(Dictionary<ColorToken, RGBA> overrides)
    {
        var merged = new Dictionary<ColorToken, RGBA>(_colors);
        foreach (var (token, color) in overrides)
        {
            merged[token] = color;
        }
        return new Theme($"{Name} (custom)", IsDark, merged);
    }

    /// <summary>
    /// The default dark theme.
    /// </summary>
    public static Theme Dark => DarkTheme.Instance;
    
    /// <summary>
    /// The default light theme.
    /// </summary>
    public static Theme Light => LightTheme.Instance;
}
