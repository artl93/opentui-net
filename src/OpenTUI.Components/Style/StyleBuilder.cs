using OpenTUI.Core.Colors;
using OpenTUI.Core.Renderables;

namespace OpenTUI.Components.Style;

/// <summary>
/// Fluent builder for creating styles.
/// </summary>
public class StyleBuilder
{
    private readonly Style _style = new();

    public StyleBuilder Color(RGBA color) { _style.Color = color; return this; }
    public StyleBuilder Color(string hex) { _style.Color = RGBA.FromHex(hex); return this; }
    
    public StyleBuilder BackgroundColor(RGBA color) { _style.BackgroundColor = color; return this; }
    public StyleBuilder BackgroundColor(string hex) { _style.BackgroundColor = RGBA.FromHex(hex); return this; }
    public StyleBuilder Bg(RGBA color) => BackgroundColor(color);
    public StyleBuilder Bg(string hex) => BackgroundColor(hex);
    
    public StyleBuilder BorderColor(RGBA color) { _style.BorderColor = color; return this; }
    public StyleBuilder BorderColor(string hex) { _style.BorderColor = RGBA.FromHex(hex); return this; }
    
    public StyleBuilder Padding(int all) { _style.Padding = new Spacing(all); return this; }
    public StyleBuilder Padding(int vertical, int horizontal) { _style.Padding = new Spacing(vertical, horizontal); return this; }
    public StyleBuilder Padding(int top, int right, int bottom, int left) { _style.Padding = new Spacing(top, right, bottom, left); return this; }
    public StyleBuilder P(int all) => Padding(all);
    public StyleBuilder Px(int horizontal) { _style.Padding = new Spacing(0, horizontal); return this; }
    public StyleBuilder Py(int vertical) { _style.Padding = new Spacing(vertical, 0); return this; }
    
    public StyleBuilder Margin(int all) { _style.Margin = new Spacing(all); return this; }
    public StyleBuilder Margin(int vertical, int horizontal) { _style.Margin = new Spacing(vertical, horizontal); return this; }
    public StyleBuilder Margin(int top, int right, int bottom, int left) { _style.Margin = new Spacing(top, right, bottom, left); return this; }
    public StyleBuilder M(int all) => Margin(all);
    public StyleBuilder Mx(int horizontal) { _style.Margin = new Spacing(0, horizontal); return this; }
    public StyleBuilder My(int vertical) { _style.Margin = new Spacing(vertical, 0); return this; }
    
    public StyleBuilder Border(BorderStyle style) { _style.Border = style; return this; }
    public StyleBuilder BorderRadius(int radius) { _style.BorderRadius = radius; return this; }
    public StyleBuilder Rounded() { _style.Border = BorderStyle.Rounded; return this; }
    
    public StyleBuilder Width(int w) { _style.Width = w; return this; }
    public StyleBuilder Height(int h) { _style.Height = h; return this; }
    public StyleBuilder Size(int w, int h) { _style.Width = w; _style.Height = h; return this; }
    public StyleBuilder MinWidth(int w) { _style.MinWidth = w; return this; }
    public StyleBuilder MinHeight(int h) { _style.MinHeight = h; return this; }
    public StyleBuilder MaxWidth(int w) { _style.MaxWidth = w; return this; }
    public StyleBuilder MaxHeight(int h) { _style.MaxHeight = h; return this; }
    
    public StyleBuilder Bold(bool b = true) { _style.Bold = b; return this; }
    public StyleBuilder Italic(bool i = true) { _style.Italic = i; return this; }
    public StyleBuilder Underline(bool u = true) { _style.Underline = u; return this; }
    public StyleBuilder Dim(bool d = true) { _style.Dim = d; return this; }
    
    public StyleBuilder Gap(int g) { _style.Gap = g; return this; }
    
    public Style Build() => _style;
    
    public static implicit operator Style(StyleBuilder builder) => builder.Build();
}
