using OpenTUI.Core.Colors;
using OpenTUI.Core.Renderables;

namespace OpenTUI.Components.Style;

/// <summary>
/// CSS-like style properties for components.
/// </summary>
public class Style
{
    // Colors
    public RGBA? Color { get; set; }
    public RGBA? BackgroundColor { get; set; }
    public RGBA? BorderColor { get; set; }
    
    // Spacing
    public Spacing? Padding { get; set; }
    public Spacing? Margin { get; set; }
    
    // Border
    public BorderStyle? Border { get; set; }
    public int? BorderRadius { get; set; }
    
    // Dimensions
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? MinWidth { get; set; }
    public int? MinHeight { get; set; }
    public int? MaxWidth { get; set; }
    public int? MaxHeight { get; set; }
    
    // Text
    public bool? Bold { get; set; }
    public bool? Italic { get; set; }
    public bool? Underline { get; set; }
    public bool? Dim { get; set; }
    
    // Layout
    public int? Gap { get; set; }
    
    /// <summary>
    /// Creates an empty style.
    /// </summary>
    public Style() { }
    
    /// <summary>
    /// Merges this style with another, with the other taking precedence.
    /// </summary>
    public Style Merge(Style? other)
    {
        if (other == null) return this;
        
        return new Style
        {
            Color = other.Color ?? Color,
            BackgroundColor = other.BackgroundColor ?? BackgroundColor,
            BorderColor = other.BorderColor ?? BorderColor,
            Padding = other.Padding ?? Padding,
            Margin = other.Margin ?? Margin,
            Border = other.Border ?? Border,
            BorderRadius = other.BorderRadius ?? BorderRadius,
            Width = other.Width ?? Width,
            Height = other.Height ?? Height,
            MinWidth = other.MinWidth ?? MinWidth,
            MinHeight = other.MinHeight ?? MinHeight,
            MaxWidth = other.MaxWidth ?? MaxWidth,
            MaxHeight = other.MaxHeight ?? MaxHeight,
            Bold = other.Bold ?? Bold,
            Italic = other.Italic ?? Italic,
            Underline = other.Underline ?? Underline,
            Dim = other.Dim ?? Dim,
            Gap = other.Gap ?? Gap,
        };
    }
    
    /// <summary>
    /// Creates a new style builder.
    /// </summary>
    public static StyleBuilder Create() => new();
}
