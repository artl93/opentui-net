using OpenTUI.Components.Components.Core;
using OpenTUI.Components.Style;
using OpenTUI.Components.Theme;
using OpenTUI.Core.Colors;
using OpenTUI.Core.Rendering;
using BorderStyle = OpenTUI.Core.Renderables.BorderStyle;

namespace OpenTUI.Components.Components.Layout;

/// <summary>
/// A horizontal flex container.
/// </summary>
public class Row : Component
{
    /// <summary>Gap between children in characters.</summary>
    public int Gap { get; set; } = 1;
    
    /// <summary>Vertical alignment of children.</summary>
    public Alignment Align { get; set; } = Alignment.Start;
    
    /// <summary>Horizontal distribution of children.</summary>
    public Justify Justify { get; set; } = Justify.Start;
    
    protected override void RenderSelf(FrameBuffer buffer, int x, int y, int width, int height)
    {
        // Row itself doesn't render, children are positioned by layout
        // Background if specified in style
        if (Style?.BackgroundColor != null)
        {
            buffer.FillRect(x, y, width, height, Style.BackgroundColor.Value);
        }
    }
}

/// <summary>
/// A vertical flex container.
/// </summary>
public class Column : Component
{
    /// <summary>Gap between children in rows.</summary>
    public int Gap { get; set; } = 0;
    
    /// <summary>Horizontal alignment of children.</summary>
    public Alignment Align { get; set; } = Alignment.Stretch;
    
    /// <summary>Vertical distribution of children.</summary>
    public Justify Justify { get; set; } = Justify.Start;
    
    protected override void RenderSelf(FrameBuffer buffer, int x, int y, int width, int height)
    {
        if (Style?.BackgroundColor != null)
        {
            buffer.FillRect(x, y, width, height, Style.BackgroundColor.Value);
        }
    }
}

/// <summary>
/// A styled box container with optional border and background.
/// </summary>
public class Box : Component
{
    /// <summary>Border style.</summary>
    public BorderStyle Border { get; set; } = BorderStyle.None;
    
    /// <summary>Padding inside the box.</summary>
    public Spacing Padding { get; set; } = new Spacing(0);
    
    /// <summary>Optional title shown in the border.</summary>
    public string? Title { get; set; }
    
    protected override void RenderSelf(FrameBuffer buffer, int x, int y, int width, int height)
    {
        var bg = Style?.BackgroundColor ?? GetColor(ColorToken.SurfaceBase);
        var borderColor = Style?.BorderColor ?? GetColor(ColorToken.BorderBase);
        
        if (Border != BorderStyle.None)
        {
            DrawThemedBox(buffer, x, y, width, height, bg, borderColor, Border);
            
            // Title
            if (!string.IsNullOrEmpty(Title) && width > Title.Length + 4)
            {
                buffer.DrawText($" {Title} ", x + 2, y, GetColor(ColorToken.TextWeak));
            }
        }
        else if (bg.A > 0)
        {
            buffer.FillRect(x, y, width, height, bg);
        }
    }
}

/// <summary>
/// An elevated card container.
/// </summary>
public class Card : Component
{
    /// <summary>Optional card title.</summary>
    public string? Title { get; set; }
    
    /// <summary>Optional card description.</summary>
    public string? Description { get; set; }
    
    protected override void RenderSelf(FrameBuffer buffer, int x, int y, int width, int height)
    {
        var bg = Style?.BackgroundColor ?? GetColor(ColorToken.SurfaceElevated);
        var borderColor = GetColor(ColorToken.BorderWeak);
        
        DrawThemedBox(buffer, x, y, width, height, bg, borderColor, BorderStyle.Rounded);
        
        var contentY = y + 1;
        
        // Title
        if (!string.IsNullOrEmpty(Title))
        {
            buffer.DrawText(Title, x + 2, contentY, GetColor(ColorToken.TextStrong));
            contentY++;
        }
        
        // Description
        if (!string.IsNullOrEmpty(Description))
        {
            buffer.DrawText(Description, x + 2, contentY, GetColor(ColorToken.TextWeak));
        }
    }
}

/// <summary>
/// A visual separator line.
/// </summary>
public class Separator : Component
{
    /// <summary>Whether the separator is vertical.</summary>
    public bool Vertical { get; set; }
    
    /// <summary>Separator character.</summary>
    public string Character { get; set; } = "─";
    
    protected override void RenderSelf(FrameBuffer buffer, int x, int y, int width, int height)
    {
        var color = Style?.Color ?? GetColor(ColorToken.BorderWeak);
        
        if (Vertical)
        {
            var renderHeight = height > 0 ? height : 1;
            for (int i = 0; i < renderHeight; i++)
            {
                buffer.SetCell(x, y + i, new Cell("│", color));
            }
        }
        else
        {
            var renderWidth = width > 0 ? width : 1;
            for (int i = 0; i < renderWidth; i++)
            {
                buffer.SetCell(x + i, y, new Cell(Character, color));
            }
        }
    }
}

/// <summary>
/// A flexible spacer that expands to fill available space.
/// </summary>
public class Spacer : Component
{
    protected override void RenderSelf(FrameBuffer buffer, int x, int y, int width, int height)
    {
        // Spacer is invisible
    }
}

/// <summary>
/// Alignment options.
/// </summary>
public enum Alignment
{
    Start,
    Center,
    End,
    Stretch
}

/// <summary>
/// Justification options.
/// </summary>
public enum Justify
{
    Start,
    Center,
    End,
    SpaceBetween,
    SpaceAround,
    SpaceEvenly
}
