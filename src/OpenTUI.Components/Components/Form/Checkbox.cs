using OpenTUI.Components.Components.Core;
using OpenTUI.Components.Theme;
using OpenTUI.Core.Colors;
using OpenTUI.Core.Rendering;

namespace OpenTUI.Components.Components.Form;

/// <summary>
/// A themed checkbox component.
/// </summary>
public class Checkbox : Component
{
    private bool _checked;

    /// <summary>Checkbox label.</summary>
    public string Label { get; set; } = "";

    /// <summary>Whether the checkbox is checked.</summary>
    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked != value)
            {
                _checked = value;
                OnChange?.Invoke(_checked);
                MarkDirty();
            }
        }
    }

    /// <summary>Whether to show an indeterminate state.</summary>
    public bool Indeterminate { get; set; }

    /// <summary>Change handler.</summary>
    public Action<bool>? OnChange { get; set; }

    /// <summary>
    /// Toggles the checkbox state.
    /// </summary>
    public void Toggle()
    {
        if (!Disabled)
        {
            Checked = !Checked;
        }
    }

    protected override void RenderSelf(FrameBuffer buffer, int x, int y, int width, int height)
    {

        var boxColor = Disabled
            ? GetColor(ColorToken.BorderWeak)
            : Focused
                ? GetColor(ColorToken.BorderSelected)
                : GetColor(ColorToken.BorderBase);

        var checkColor = Disabled
            ? GetColor(ColorToken.TextDisabled)
            : GetColor(ColorToken.PrimaryBase);

        var textColor = Disabled
            ? GetColor(ColorToken.TextDisabled)
            : GetColor(ColorToken.TextBase);

        // Draw checkbox box
        // Box brackets
        buffer.DrawText("[", x, y, boxColor);
        buffer.DrawText("]", x + 2, y, boxColor);

        // Check mark
        if (Indeterminate)
        {
            buffer.DrawText("─", x + 1, y, checkColor);
        }
        else if (Checked)
        {
            buffer.DrawText("✓", x + 1, y, checkColor);
        }
        else
        {
            buffer.DrawText(" ", x + 1, y, RGBA.Transparent);
        }

        // Label
        if (!string.IsNullOrEmpty(Label))
        {
            buffer.DrawText(Label, x + 4, y, textColor);
        }

        // Focus indicator
        if (Focused)
        {
            buffer.SetCell(x - 1, y, new Cell("▶", GetColor(ColorToken.PrimaryBase)));
        }
    }
}
