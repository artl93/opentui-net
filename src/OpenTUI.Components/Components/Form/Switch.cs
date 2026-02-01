using OpenTUI.Components.Components.Core;
using OpenTUI.Components.Theme;
using OpenTUI.Core.Colors;
using OpenTUI.Core.Rendering;

namespace OpenTUI.Components.Components.Form;

/// <summary>
/// A themed on/off switch component.
/// </summary>
public class Switch : Component
{
    private bool _on;
    
    /// <summary>Optional label.</summary>
    public string? Label { get; set; }
    
    /// <summary>Text shown when on.</summary>
    public string OnText { get; set; } = "ON";
    
    /// <summary>Text shown when off.</summary>
    public string OffText { get; set; } = "OFF";
    
    /// <summary>Whether the switch is on.</summary>
    public bool On
    {
        get => _on;
        set
        {
            if (_on != value)
            {
                _on = value;
                OnChange?.Invoke(_on);
                MarkDirty();
            }
        }
    }
    
    /// <summary>Change handler.</summary>
    public Action<bool>? OnChange { get; set; }
    
    /// <summary>
    /// Toggles the switch state.
    /// </summary>
    public void Toggle()
    {
        if (!Disabled)
        {
            On = !On;
        }
    }
    
    protected override void RenderSelf(FrameBuffer buffer, int x, int y, int width, int height)
    {
        
        var trackBg = On
            ? (Disabled ? GetColor(ColorToken.PrimaryWeak) : GetColor(ColorToken.PrimaryBase))
            : (Disabled ? GetColor(ColorToken.SurfaceSunken) : GetColor(ColorToken.SurfaceOverlay));
        
        var thumbColor = Disabled
            ? GetColor(ColorToken.TextDisabled)
            : GetColor(ColorToken.TextOnPrimary);
        
        var textColor = Disabled
            ? GetColor(ColorToken.TextDisabled)
            : GetColor(ColorToken.TextBase);
        
        // Draw track ═══○ or ●═══
        var trackWidth = 4;
        
        if (On)
        {
            // ●═══  (thumb on right)
            buffer.DrawText("═══", x, y, trackBg);
            buffer.DrawText("●", x + 3, y, thumbColor);
        }
        else
        {
            // ○═══  (thumb on left)
            buffer.DrawText("○", x, y, GetColor(ColorToken.BorderBase));
            buffer.DrawText("═══", x + 1, y, trackBg);
        }
        
        // Label
        if (!string.IsNullOrEmpty(Label))
        {
            buffer.DrawText(Label, x + trackWidth + 2, y, textColor);
        }
        
        // Focus indicator
        if (Focused)
        {
            var focusColor = GetColor(ColorToken.BorderSelected);
            buffer.SetCell(x - 1, y, new Cell("▶", focusColor));
        }
    }
}
