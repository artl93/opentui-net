using OpenTUI.Components.Components.Core;
using OpenTUI.Components.Style;
using OpenTUI.Components.Theme;
using OpenTUI.Core.Colors;
using OpenTUI.Core.Rendering;
using BorderStyle = OpenTUI.Core.Renderables.BorderStyle;

namespace OpenTUI.Components.Components.Form;

/// <summary>
/// Button variants matching OpenCode's design system.
/// </summary>
public enum ButtonVariant
{
    Primary,
    Secondary,
    Ghost,
    Danger
}

/// <summary>
/// A themed button component with variants, icons, and loading state.
/// </summary>
public class Button : Component
{
    private bool _isPressed;
    private bool _isHovered;
    
    /// <summary>Button label text.</summary>
    public string Label { get; set; } = "";
    
    /// <summary>Button variant (Primary, Secondary, Ghost, Danger).</summary>
    public ButtonVariant Variant { get; set; } = ButtonVariant.Secondary;
    
    /// <summary>Button size.</summary>
    public ComponentSize Size { get; set; } = ComponentSize.Medium;
    
    /// <summary>Optional icon (Unicode character) shown before label.</summary>
    public string? Icon { get; set; }
    
    /// <summary>Whether the button is in loading state.</summary>
    public bool Loading { get; set; }
    
    /// <summary>Keyboard shortcut hint (e.g., "Ctrl+S").</summary>
    public string? Shortcut { get; set; }
    
    /// <summary>Click handler.</summary>
    public Action? OnClick { get; set; }
    
    /// <summary>
    /// Triggers the button click.
    /// </summary>
    public void Click()
    {
        if (!Disabled && !Loading)
        {
            OnClick?.Invoke();
        }
    }
    
    protected override void RenderSelf(FrameBuffer buffer, int x, int y, int width, int height)
    {
        var (bg, fg, border) = GetColors();
        var (paddingX, paddingY, renderHeight) = GetSizing();
        
        // Calculate content
        var iconPart = Icon != null ? $"{Icon} " : "";
        var loadingPart = Loading ? "◐ " : "";
        var content = $"{loadingPart}{iconPart}{Label}";
        var shortcutPart = Shortcut != null ? $" [{Shortcut}]" : "";
        var fullContent = content + shortcutPart;
        
        var buttonWidth = fullContent.Length + (paddingX * 2);
        
        // Background
        buffer.FillRect(x, y, buttonWidth, renderHeight, bg);
        
        // Border for secondary variant
        if (Variant == ButtonVariant.Secondary && border.A > 0)
        {
            DrawThemedBox(buffer, x, y, buttonWidth, renderHeight, 
                background: null, 
                borderColor: Focused ? GetColor(ColorToken.BorderSelected) : border,
                borderStyle: BorderStyle.Rounded);
        }
        else if (Variant != ButtonVariant.Ghost)
        {
            // Rounded corners hint (just color the background appropriately)
            buffer.FillRect(x, y, buttonWidth, renderHeight, bg);
        }
        
        // Text
        var textX = x + paddingX;
        var textY = y + (renderHeight - 1) / 2;
        
        // Content
        buffer.DrawText(content, textX, textY, fg);
        
        // Shortcut hint (dimmed)
        if (Shortcut != null)
        {
            var shortcutColor = fg.WithAlpha(0.5f);
            buffer.DrawText(shortcutPart, textX + content.Length, textY, 
                Variant == ButtonVariant.Ghost ? GetColor(ColorToken.TextWeak) : shortcutColor);
        }
        
        // Focus indicator
        if (Focused)
        {
            var focusColor = GetColor(ColorToken.BorderSelected);
            buffer.SetCell(x - 1, textY, new Cell("▶", focusColor));
        }
    }
    
    private (RGBA bg, RGBA fg, RGBA border) GetColors()
    {
        if (Disabled)
        {
            return (
                GetColor(ColorToken.SurfaceSunken),
                GetColor(ColorToken.TextDisabled),
                GetColor(ColorToken.BorderWeak)
            );
        }
        
        return Variant switch
        {
            ButtonVariant.Primary => (
                _isPressed ? GetColor(ColorToken.PrimaryActive) :
                _isHovered ? GetColor(ColorToken.PrimaryHover) :
                GetColor(ColorToken.PrimaryBase),
                GetColor(ColorToken.TextOnPrimary),
                RGBA.Transparent
            ),
            ButtonVariant.Secondary => (
                _isPressed ? GetColor(ColorToken.SecondaryActive) :
                _isHovered ? GetColor(ColorToken.SecondaryHover) :
                GetColor(ColorToken.SurfaceElevated),
                GetColor(ColorToken.TextStrong),
                GetColor(ColorToken.BorderBase)
            ),
            ButtonVariant.Ghost => (
                _isPressed ? GetColor(ColorToken.GhostActive) :
                _isHovered ? GetColor(ColorToken.GhostHover) :
                RGBA.Transparent,
                GetColor(ColorToken.TextBase),
                RGBA.Transparent
            ),
            ButtonVariant.Danger => (
                _isPressed ? GetColor(ColorToken.CriticalBase).WithAlpha(0.8f) :
                _isHovered ? GetColor(ColorToken.CriticalBase).WithAlpha(0.9f) :
                GetColor(ColorToken.CriticalBase),
                GetColor(ColorToken.TextOnCritical),
                RGBA.Transparent
            ),
            _ => (GetColor(ColorToken.SurfaceElevated), GetColor(ColorToken.TextBase), GetColor(ColorToken.BorderBase))
        };
    }
    
    private (int paddingX, int paddingY, int height) GetSizing()
    {
        return Size switch
        {
            ComponentSize.Small => (1, 0, 1),
            ComponentSize.Medium => (2, 0, 1),
            ComponentSize.Large => (3, 1, 3),
            _ => (2, 0, 1)
        };
    }
}
