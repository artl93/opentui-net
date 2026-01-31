using System.Text;
using OpenTUI.Core.Colors;

namespace OpenTUI.Core.Rendering;

/// <summary>
/// A 2D buffer of cells for rendering terminal output.
/// Optimized for performance with dirty region tracking.
/// </summary>
public class FrameBuffer
{
    private readonly Cell[,] _cells;
    private readonly bool[,] _dirty;
    
    /// <summary>Width of the buffer in columns.</summary>
    public int Width { get; }
    
    /// <summary>Height of the buffer in rows.</summary>
    public int Height { get; }

    /// <summary>
    /// Creates a new frame buffer with the specified dimensions.
    /// </summary>
    public FrameBuffer(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        
        Width = width;
        Height = height;
        _cells = new Cell[height, width];
        _dirty = new bool[height, width];
        
        Clear();
    }

    /// <summary>
    /// Gets or sets a cell at the specified position.
    /// </summary>
    public ref Cell this[int row, int col] => ref _cells[row, col];

    /// <summary>
    /// Gets a cell at the specified position (bounds-checked).
    /// </summary>
    public Cell GetCell(int row, int col)
    {
        if (row < 0 || row >= Height || col < 0 || col >= Width)
            return new Cell();
        return _cells[row, col];
    }

    /// <summary>
    /// Sets a cell at the specified position.
    /// </summary>
    public void SetCell(int col, int row, Cell cell)
    {
        if (row < 0 || row >= Height || col < 0 || col >= Width)
            return;
            
        if (_cells[row, col] != cell)
        {
            _cells[row, col] = cell;
            _dirty[row, col] = true;
        }
    }

    /// <summary>
    /// Sets a cell with alpha blending against the existing background.
    /// </summary>
    public void SetCellWithAlphaBlending(int col, int row, Cell cell)
    {
        if (row < 0 || row >= Height || col < 0 || col >= Width)
            return;

        var existing = _cells[row, col];
        
        // Blend foreground if semi-transparent
        if (cell.Foreground.A < 1f && cell.Foreground.A > 0f)
            cell.Foreground = cell.Foreground.BlendOver(existing.Foreground);
            
        // Blend background if semi-transparent
        if (cell.Background.A < 1f)
            cell.Background = cell.Background.BlendOver(existing.Background);

        SetCell(col, row, cell);
    }

    /// <summary>
    /// Draws text at the specified position.
    /// </summary>
    public void DrawText(string text, int col, int row, RGBA? foreground = null, RGBA? background = null, TextAttributes attributes = TextAttributes.None)
    {
        if (row < 0 || row >= Height || string.IsNullOrEmpty(text))
            return;

        var fg = foreground ?? RGBA.White;
        var bg = background ?? RGBA.Transparent;
        
        var currentCol = col;
        foreach (var c in text)
        {
            if (currentCol >= Width)
                break;
            if (currentCol >= 0)
            {
                SetCell(currentCol, row, new Cell(c.ToString(), fg, bg, attributes));
            }
            currentCol++;
        }
    }

    /// <summary>
    /// Fills a rectangular region with a color.
    /// </summary>
    public void FillRect(int col, int row, int width, int height, RGBA color)
    {
        var cell = new Cell(" ", RGBA.White, color);
        
        for (var r = row; r < row + height && r < Height; r++)
        {
            if (r < 0) continue;
            for (var c = col; c < col + width && c < Width; c++)
            {
                if (c < 0) continue;
                SetCell(c, r, cell);
            }
        }
    }

    /// <summary>
    /// Draws a border around a rectangular region.
    /// </summary>
    public void DrawBorder(int col, int row, int width, int height, BorderStyle style, RGBA? foreground = null, RGBA? background = null)
    {
        var chars = GetBorderChars(style);
        var fg = foreground ?? RGBA.White;
        var bg = background ?? RGBA.Transparent;

        // Corners
        SetCell(col, row, new Cell(chars.TopLeft, fg, bg));
        SetCell(col + width - 1, row, new Cell(chars.TopRight, fg, bg));
        SetCell(col, row + height - 1, new Cell(chars.BottomLeft, fg, bg));
        SetCell(col + width - 1, row + height - 1, new Cell(chars.BottomRight, fg, bg));

        // Top and bottom edges
        for (var c = col + 1; c < col + width - 1; c++)
        {
            SetCell(c, row, new Cell(chars.Horizontal, fg, bg));
            SetCell(c, row + height - 1, new Cell(chars.Horizontal, fg, bg));
        }

        // Left and right edges
        for (var r = row + 1; r < row + height - 1; r++)
        {
            SetCell(col, r, new Cell(chars.Vertical, fg, bg));
            SetCell(col + width - 1, r, new Cell(chars.Vertical, fg, bg));
        }
    }

    /// <summary>
    /// Copies content from another frame buffer.
    /// </summary>
    public void DrawFrameBuffer(FrameBuffer source, int destCol, int destRow, int srcCol = 0, int srcRow = 0, int? width = null, int? height = null)
    {
        var w = Math.Min(width ?? source.Width, source.Width - srcCol);
        var h = Math.Min(height ?? source.Height, source.Height - srcRow);

        for (var r = 0; r < h; r++)
        {
            var dr = destRow + r;
            var sr = srcRow + r;
            if (dr < 0 || dr >= Height || sr < 0 || sr >= source.Height)
                continue;

            for (var c = 0; c < w; c++)
            {
                var dc = destCol + c;
                var sc = srcCol + c;
                if (dc < 0 || dc >= Width || sc < 0 || sc >= source.Width)
                    continue;

                SetCellWithAlphaBlending(dc, dr, source._cells[sr, sc]);
            }
        }
    }

    /// <summary>
    /// Clears the entire buffer.
    /// </summary>
    public void Clear(RGBA? background = null)
    {
        var bg = background ?? RGBA.Transparent;
        for (var r = 0; r < Height; r++)
        {
            for (var c = 0; c < Width; c++)
            {
                _cells[r, c] = new Cell(" ", RGBA.White, bg);
                _dirty[r, c] = true;
            }
        }
    }

    /// <summary>
    /// Checks if a cell is marked dirty.
    /// </summary>
    public bool IsDirty(int row, int col) => _dirty[row, col];

    /// <summary>
    /// Marks all cells as clean.
    /// </summary>
    public void ClearDirty()
    {
        Array.Clear(_dirty);
    }

    /// <summary>
    /// Marks all cells as dirty (forces full redraw).
    /// </summary>
    public void MarkAllDirty()
    {
        for (var r = 0; r < Height; r++)
            for (var c = 0; c < Width; c++)
                _dirty[r, c] = true;
    }

    /// <summary>
    /// Checks if any cell is dirty.
    /// </summary>
    public bool HasDirtyCells()
    {
        for (var r = 0; r < Height; r++)
            for (var c = 0; c < Width; c++)
                if (_dirty[r, c])
                    return true;
        return false;
    }

    /// <summary>
    /// Generates ANSI output for the entire buffer.
    /// </summary>
    public string ToAnsiString()
    {
        var sb = new StringBuilder();
        sb.Append(Ansi.MoveCursor(1, 1));

        RGBA? lastFg = null;
        RGBA? lastBg = null;
        TextAttributes lastAttrs = TextAttributes.None;

        for (var r = 0; r < Height; r++)
        {
            sb.Append(Ansi.MoveCursor(r + 1, 1));
            
            for (var c = 0; c < Width; c++)
            {
                var cell = _cells[r, c];
                if (cell.Width == 0) continue; // Skip wide char placeholders

                // Only emit style changes when needed
                if (lastFg != cell.Foreground || lastBg != cell.Background || lastAttrs != cell.Attributes)
                {
                    sb.Append(Ansi.SetStyle(cell.Foreground, cell.Background, cell.Attributes));
                    lastFg = cell.Foreground;
                    lastBg = cell.Background;
                    lastAttrs = cell.Attributes;
                }

                sb.Append(cell.Character);
            }
        }

        sb.Append(Ansi.Reset);
        return sb.ToString();
    }

    /// <summary>
    /// Generates ANSI output for only dirty cells (differential update).
    /// </summary>
    public string ToDifferentialAnsiString()
    {
        var sb = new StringBuilder();
        int lastRow = -1, lastCol = -1;

        for (var r = 0; r < Height; r++)
        {
            for (var c = 0; c < Width; c++)
            {
                if (!_dirty[r, c]) continue;

                var cell = _cells[r, c];
                if (cell.Width == 0) continue;

                // Move cursor if not contiguous
                if (r != lastRow || c != lastCol + 1)
                {
                    sb.Append(Ansi.MoveCursor(r + 1, c + 1));
                }

                sb.Append(Ansi.SetStyle(cell.Foreground, cell.Background, cell.Attributes));
                sb.Append(cell.Character);

                lastRow = r;
                lastCol = c;
            }
        }

        sb.Append(Ansi.Reset);
        return sb.ToString();
    }

    private static (string TopLeft, string TopRight, string BottomLeft, string BottomRight, string Horizontal, string Vertical) GetBorderChars(BorderStyle style)
    {
        return style switch
        {
            BorderStyle.Single => ("┌", "┐", "└", "┘", "─", "│"),
            BorderStyle.Double => ("╔", "╗", "╚", "╝", "═", "║"),
            BorderStyle.Rounded => ("╭", "╮", "╰", "╯", "─", "│"),
            BorderStyle.Bold => ("┏", "┓", "┗", "┛", "━", "┃"),
            BorderStyle.Ascii => ("+", "+", "+", "+", "-", "|"),
            _ => ("┌", "┐", "└", "┘", "─", "│")
        };
    }
}

/// <summary>
/// Border drawing styles.
/// </summary>
public enum BorderStyle
{
    Single,
    Double,
    Rounded,
    Bold,
    Ascii,
    None
}
