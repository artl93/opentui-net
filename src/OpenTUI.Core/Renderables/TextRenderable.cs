using OpenTUI.Core.Colors;
using OpenTUI.Core.Rendering;

namespace OpenTUI.Core.Renderables;

/// <summary>
/// Text wrapping mode.
/// </summary>
public enum TextWrap
{
    /// <summary>No wrapping - text is clipped at boundaries.</summary>
    None,
    /// <summary>Wrap at word boundaries.</summary>
    Word,
    /// <summary>Wrap at character boundaries.</summary>
    Character
}

/// <summary>
/// Text alignment within the renderable.
/// </summary>
public enum TextAlign
{
    Left,
    Center,
    Right
}

/// <summary>
/// A renderable that displays styled text.
/// </summary>
public class TextRenderable : Renderable
{
    private string _text = string.Empty;
    private TextWrap _wrap = TextWrap.None;
    private TextAlign _align = TextAlign.Left;
    private TextAttributes _attributes = TextAttributes.None;

    /// <summary>The text content to display.</summary>
    public string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value ?? string.Empty;
                MarkDirty();
            }
        }
    }

    /// <summary>Text wrapping mode.</summary>
    public TextWrap Wrap
    {
        get => _wrap;
        set
        {
            if (_wrap != value)
            {
                _wrap = value;
                MarkDirty();
            }
        }
    }

    /// <summary>Text alignment.</summary>
    public TextAlign Align
    {
        get => _align;
        set
        {
            if (_align != value)
            {
                _align = value;
                MarkDirty();
            }
        }
    }

    /// <summary>Text attributes (bold, italic, etc.).</summary>
    public TextAttributes Attributes
    {
        get => _attributes;
        set
        {
            if (_attributes != value)
            {
                _attributes = value;
                MarkDirty();
            }
        }
    }

    public TextRenderable() { }

    public TextRenderable(string text)
    {
        _text = text ?? string.Empty;
    }

    protected override void RenderSelf(FrameBuffer buffer, int x, int y, int width, int height)
    {
        if (string.IsNullOrEmpty(_text) || width <= 0 || height <= 0)
            return;

        var fg = ForegroundColor ?? RGBA.White;
        var bg = BackgroundColor;
        var lines = WrapText(_text, width);

        for (int i = 0; i < lines.Count && i < height; i++)
        {
            var line = lines[i];
            var lineX = GetAlignedX(line, x, width);
            
            for (int j = 0; j < line.Length && lineX + j < x + width; j++)
            {
                var cell = new Cell(
                    line[j].ToString(),
                    fg,
                    bg ?? buffer.GetCell(y + i, lineX + j).Background,
                    _attributes
                );
                buffer.SetCell(lineX + j, y + i, cell);
            }
        }
    }

    private int GetAlignedX(string line, int x, int width)
    {
        return _align switch
        {
            TextAlign.Center => x + Math.Max(0, (width - line.Length) / 2),
            TextAlign.Right => x + Math.Max(0, width - line.Length),
            _ => x
        };
    }

    private List<string> WrapText(string text, int maxWidth)
    {
        if (maxWidth <= 0)
            return new List<string>();

        if (_wrap == TextWrap.None)
        {
            // Split by newlines only, truncate each line
            return text.Split('\n')
                .Select(line => line.Length <= maxWidth ? line : line[..maxWidth])
                .ToList();
        }

        var lines = new List<string>();
        var paragraphs = text.Split('\n');

        foreach (var paragraph in paragraphs)
        {
            if (string.IsNullOrEmpty(paragraph))
            {
                lines.Add(string.Empty);
                continue;
            }

            if (_wrap == TextWrap.Character)
            {
                // Wrap at character boundaries
                for (int i = 0; i < paragraph.Length; i += maxWidth)
                {
                    lines.Add(paragraph.Substring(i, Math.Min(maxWidth, paragraph.Length - i)));
                }
            }
            else // TextWrap.Word
            {
                var words = paragraph.Split(' ');
                var currentLine = string.Empty;

                foreach (var word in words)
                {
                    if (string.IsNullOrEmpty(currentLine))
                    {
                        currentLine = word;
                    }
                    else if (currentLine.Length + 1 + word.Length <= maxWidth)
                    {
                        currentLine += " " + word;
                    }
                    else
                    {
                        lines.Add(currentLine);
                        currentLine = word;
                    }

                    // Handle words longer than maxWidth
                    while (currentLine.Length > maxWidth)
                    {
                        lines.Add(currentLine[..maxWidth]);
                        currentLine = currentLine[maxWidth..];
                    }
                }

                if (!string.IsNullOrEmpty(currentLine))
                {
                    lines.Add(currentLine);
                }
            }
        }

        return lines;
    }
}
