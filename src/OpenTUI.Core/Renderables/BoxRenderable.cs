using OpenTUI.Core.Colors;
using OpenTUI.Core.Rendering;

namespace OpenTUI.Core.Renderables;

/// <summary>
/// Border style for BoxRenderable.
/// </summary>
public enum BorderStyle
{
    None,
    Single,
    Double,
    Rounded,
    Bold,
    Dashed
}

/// <summary>
/// A container renderable with optional border and title.
/// </summary>
public class BoxRenderable : Renderable
{
    private BorderStyle _borderStyle = BorderStyle.Single;
    private RGBA? _borderColor;
    private string? _title;
    private TextAlign _titleAlign = TextAlign.Left;

    /// <summary>Border style.</summary>
    public BorderStyle BorderStyle
    {
        get => _borderStyle;
        set
        {
            if (_borderStyle != value)
            {
                _borderStyle = value;
                MarkDirty();
            }
        }
    }

    /// <summary>Border color (defaults to foreground color).</summary>
    public RGBA? BorderColor
    {
        get => _borderColor;
        set
        {
            if (_borderColor != value)
            {
                _borderColor = value;
                MarkDirty();
            }
        }
    }

    /// <summary>Optional title displayed in the top border.</summary>
    public string? Title
    {
        get => _title;
        set
        {
            if (_title != value)
            {
                _title = value;
                MarkDirty();
            }
        }
    }

    /// <summary>Title alignment within the top border.</summary>
    public TextAlign TitleAlign
    {
        get => _titleAlign;
        set
        {
            if (_titleAlign != value)
            {
                _titleAlign = value;
                MarkDirty();
            }
        }
    }

    protected override void RenderSelf(FrameBuffer buffer, int x, int y, int width, int height)
    {
        if (width <= 0 || height <= 0)
            return;

        if (_borderStyle != BorderStyle.None && width >= 2 && height >= 2)
        {
            var chars = GetBorderChars(_borderStyle);
            var fg = _borderColor ?? ForegroundColor ?? RGBA.White;
            var bg = BackgroundColor;

            // Top border
            DrawHorizontalLine(buffer, x, y, width, chars.TopLeft, chars.Horizontal, chars.TopRight, fg, bg);

            // Bottom border
            DrawHorizontalLine(buffer, x, y + height - 1, width, chars.BottomLeft, chars.Horizontal, chars.BottomRight, fg, bg);

            // Side borders
            for (int row = y + 1; row < y + height - 1; row++)
            {
                SetBorderCell(buffer, x, row, chars.Vertical, fg, bg);
                SetBorderCell(buffer, x + width - 1, row, chars.Vertical, fg, bg);
            }

            // Draw title if present
            if (!string.IsNullOrEmpty(_title) && width > 4)
            {
                var maxTitleLen = width - 4; // Leave room for corners and padding
                var title = _title.Length <= maxTitleLen ? _title : _title[..maxTitleLen];
                var titleX = _titleAlign switch
                {
                    TextAlign.Center => x + (width - title.Length - 2) / 2,
                    TextAlign.Right => x + width - title.Length - 3,
                    _ => x + 2
                };

                // Draw title with surrounding spaces
                SetBorderCell(buffer, titleX, y, " ", fg, bg);
                for (int i = 0; i < title.Length; i++)
                {
                    SetBorderCell(buffer, titleX + 1 + i, y, title[i].ToString(), fg, bg);
                }
                SetBorderCell(buffer, titleX + title.Length + 1, y, " ", fg, bg);
            }
        }
    }

    private void DrawHorizontalLine(FrameBuffer buffer, int x, int y, int width, 
        string left, string horizontal, string right, RGBA fg, RGBA? bg)
    {
        SetBorderCell(buffer, x, y, left, fg, bg);
        for (int col = x + 1; col < x + width - 1; col++)
        {
            SetBorderCell(buffer, col, y, horizontal, fg, bg);
        }
        SetBorderCell(buffer, x + width - 1, y, right, fg, bg);
    }

    private void SetBorderCell(FrameBuffer buffer, int col, int row, string ch, RGBA fg, RGBA? bg)
    {
        var existingBg = bg ?? buffer.GetCell(row, col).Background;
        buffer.SetCell(col, row, new Cell(ch, fg, existingBg));
    }

    private static BorderChars GetBorderChars(BorderStyle style)
    {
        return style switch
        {
            BorderStyle.Double => new BorderChars("╔", "╗", "╚", "╝", "═", "║"),
            BorderStyle.Rounded => new BorderChars("╭", "╮", "╰", "╯", "─", "│"),
            BorderStyle.Bold => new BorderChars("┏", "┓", "┗", "┛", "━", "┃"),
            BorderStyle.Dashed => new BorderChars("┌", "┐", "└", "┘", "╌", "╎"),
            _ => new BorderChars("┌", "┐", "└", "┘", "─", "│") // Single
        };
    }

    private readonly record struct BorderChars(
        string TopLeft, string TopRight, 
        string BottomLeft, string BottomRight,
        string Horizontal, string Vertical);
}
