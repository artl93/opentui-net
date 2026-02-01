using OpenTUI.Components.Style;
using OpenTUI.Components.Theme;
using OpenTUI.Core.Colors;
using OpenTUI.Core.Rendering;
using OpenTUI.Core.Renderables;
using BorderStyle = OpenTUI.Core.Renderables.BorderStyle;

namespace OpenTUI.Components.Components.Core;

/// <summary>
/// Base class for all high-level components.
/// Provides theming, styling, and common functionality.
/// </summary>
public abstract class Component : Renderable
{
    /// <summary>Custom style overrides.</summary>
    public Style.Style? Style { get; set; }
    
    /// <summary>Whether the component is disabled.</summary>
    public bool Disabled { get; set; }
    
    /// <summary>Whether the component is currently focused.</summary>
    public bool Focused { get; protected set; }
    
    /// <summary>Tooltip text shown on hover (if supported).</summary>
    public string? Tooltip { get; set; }
    
    /// <summary>Gets the current theme.</summary>
    protected Theme.Theme Theme => ThemeProvider.Current;
    
    /// <summary>
    /// Gets a color from the theme.
    /// </summary>
    protected RGBA GetColor(ColorToken token) => Theme[token];
    
    /// <summary>
    /// Gets a color, adjusted for disabled state.
    /// </summary>
    protected RGBA GetColor(ColorToken token, ColorToken disabledToken)
    {
        return Disabled ? Theme[disabledToken] : Theme[token];
    }
    
    /// <summary>
    /// Gets the effective style by merging base with overrides.
    /// </summary>
    protected Style.Style GetEffectiveStyle(Style.Style? baseStyle = null)
    {
        return (baseStyle ?? new Style.Style()).Merge(Style);
    }
    
    /// <summary>
    /// Called when the component receives focus.
    /// </summary>
    public override void OnFocus()
    {
        Focused = true;
        MarkDirty();
    }
    
    /// <summary>
    /// Called when the component loses focus.
    /// </summary>
    public override void OnBlur()
    {
        Focused = false;
        MarkDirty();
    }
    
    /// <summary>
    /// Draws a box with theme-aware styling.
    /// </summary>
    protected void DrawThemedBox(
        FrameBuffer buffer,
        int x, int y, int width, int height,
        RGBA? background = null,
        RGBA? borderColor = null,
        BorderStyle? borderStyle = null)
    {
        var bg = background ?? GetColor(ColorToken.SurfaceBase);
        var border = borderColor ?? GetColor(ColorToken.BorderBase);
        var style = borderStyle ?? BorderStyle.Single;
        
        // Fill background
        if (bg.A > 0)
        {
            buffer.FillRect(x, y, width, height, bg);
        }
        
        // Draw border
        if (style != BorderStyle.None)
        {
            var chars = GetBorderChars(style);
            
            // Corners
            buffer.SetCell(x, y, new Cell(chars.TopLeft, border));
            buffer.SetCell(x + width - 1, y, new Cell(chars.TopRight, border));
            buffer.SetCell(x, y + height - 1, new Cell(chars.BottomLeft, border));
            buffer.SetCell(x + width - 1, y + height - 1, new Cell(chars.BottomRight, border));
            
            // Horizontal edges
            for (int i = 1; i < width - 1; i++)
            {
                buffer.SetCell(x + i, y, new Cell(chars.Horizontal, border));
                buffer.SetCell(x + i, y + height - 1, new Cell(chars.Horizontal, border));
            }
            
            // Vertical edges
            for (int j = 1; j < height - 1; j++)
            {
                buffer.SetCell(x, y + j, new Cell(chars.Vertical, border));
                buffer.SetCell(x + width - 1, y + j, new Cell(chars.Vertical, border));
            }
        }
    }
    
    private static (string TopLeft, string TopRight, string BottomLeft, string BottomRight, string Horizontal, string Vertical) GetBorderChars(BorderStyle style)
    {
        return style switch
        {
            BorderStyle.Single => ("┌", "┐", "└", "┘", "─", "│"),
            BorderStyle.Double => ("╔", "╗", "╚", "╝", "═", "║"),
            BorderStyle.Rounded => ("╭", "╮", "╰", "╯", "─", "│"),
            BorderStyle.Bold => ("┏", "┓", "┗", "┛", "━", "┃"),
            BorderStyle.Dashed => ("┌", "┐", "└", "┘", "╌", "╎"),
            _ => ("┌", "┐", "└", "┘", "─", "│")
        };
    }
    
    /// <summary>
    /// Draws themed text.
    /// </summary>
    protected void DrawThemedText(
        FrameBuffer buffer,
        string text,
        int x, int y,
        ColorToken colorToken,
        bool bold = false,
        bool dim = false)
    {
        var color = Disabled ? GetColor(ColorToken.TextDisabled) : GetColor(colorToken);
        if (dim) color = color.WithAlpha(0.6f);
        
        buffer.DrawText(text, x, y, color);
    }
}

/// <summary>
/// Common component sizes.
/// </summary>
public enum ComponentSize
{
    Small,
    Medium,
    Large
}
