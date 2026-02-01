using OpenTUI.Core.Colors;
using OpenTUI.Core.Rendering;

namespace OpenTUI.Core.Renderables;

/// <summary>
/// A scrollable container for content larger than the viewport.
/// </summary>
public class ScrollBoxRenderable : Renderable
{
    private int _scrollX;
    private int _scrollY;
    private bool _showHorizontalScrollbar = true;
    private bool _showVerticalScrollbar = true;

    /// <summary>Horizontal scroll offset.</summary>
    public int ScrollX
    {
        get => _scrollX;
        set
        {
            var newValue = Math.Max(0, value);
            if (_scrollX != newValue)
            {
                _scrollX = newValue;
                MarkDirty();
                ScrollChanged?.Invoke(this, (_scrollX, _scrollY));
            }
        }
    }

    /// <summary>Vertical scroll offset.</summary>
    public int ScrollY
    {
        get => _scrollY;
        set
        {
            var newValue = Math.Max(0, value);
            if (_scrollY != newValue)
            {
                _scrollY = newValue;
                MarkDirty();
                ScrollChanged?.Invoke(this, (_scrollX, _scrollY));
            }
        }
    }

    /// <summary>Whether to show horizontal scrollbar.</summary>
    public bool ShowHorizontalScrollbar
    {
        get => _showHorizontalScrollbar;
        set
        {
            if (_showHorizontalScrollbar != value)
            {
                _showHorizontalScrollbar = value;
                MarkDirty();
            }
        }
    }

    /// <summary>Whether to show vertical scrollbar.</summary>
    public bool ShowVerticalScrollbar
    {
        get => _showVerticalScrollbar;
        set
        {
            if (_showVerticalScrollbar != value)
            {
                _showVerticalScrollbar = value;
                MarkDirty();
            }
        }
    }

    /// <summary>Scrollbar track color.</summary>
    public RGBA ScrollbarTrackColor { get; set; } = RGBA.FromValues(0.2f, 0.2f, 0.2f);

    /// <summary>Scrollbar thumb color.</summary>
    public RGBA ScrollbarThumbColor { get; set; } = RGBA.FromValues(0.5f, 0.5f, 0.5f);

    /// <summary>Event raised when scroll position changes.</summary>
    public event EventHandler<(int X, int Y)>? ScrollChanged;

    public ScrollBoxRenderable()
    {
        Focusable = true;
    }

    /// <summary>Scrolls up by the specified amount.</summary>
    public void ScrollUp(int amount = 1)
    {
        ScrollY = Math.Max(0, _scrollY - amount);
    }

    /// <summary>Scrolls down by the specified amount.</summary>
    public void ScrollDown(int amount = 1)
    {
        ScrollY += amount;
    }

    /// <summary>Scrolls left by the specified amount.</summary>
    public void ScrollLeft(int amount = 1)
    {
        ScrollX = Math.Max(0, _scrollX - amount);
    }

    /// <summary>Scrolls right by the specified amount.</summary>
    public void ScrollRight(int amount = 1)
    {
        ScrollX += amount;
    }

    /// <summary>Scrolls to the top.</summary>
    public void ScrollToTop()
    {
        ScrollY = 0;
    }

    /// <summary>Scrolls to show a specific position.</summary>
    public void ScrollTo(int x, int y)
    {
        _scrollX = Math.Max(0, x);
        _scrollY = Math.Max(0, y);
        MarkDirty();
        ScrollChanged?.Invoke(this, (_scrollX, _scrollY));
    }

    /// <summary>Gets the content size from children.</summary>
    private (int Width, int Height) GetContentSize()
    {
        int maxWidth = 0;
        int maxHeight = 0;

        foreach (var child in Children)
        {
            var childRight = (int)(child.Layout.Layout.X + child.Layout.Layout.Width);
            var childBottom = (int)(child.Layout.Layout.Y + child.Layout.Layout.Height);
            maxWidth = Math.Max(maxWidth, childRight);
            maxHeight = Math.Max(maxHeight, childBottom);
        }

        return (maxWidth, maxHeight);
    }

    protected override void RenderSelf(FrameBuffer buffer, int x, int y, int width, int height)
    {
        if (width <= 0 || height <= 0) return;

        var (contentWidth, contentHeight) = GetContentSize();

        // Calculate viewport dimensions (accounting for scrollbars)
        var viewportWidth = _showVerticalScrollbar && contentHeight > height ? width - 1 : width;
        var viewportHeight = _showHorizontalScrollbar && contentWidth > viewportWidth ? height - 1 : height;

        // Clamp scroll positions
        var maxScrollX = Math.Max(0, contentWidth - viewportWidth);
        var maxScrollY = Math.Max(0, contentHeight - viewportHeight);
        _scrollX = Math.Min(_scrollX, maxScrollX);
        _scrollY = Math.Min(_scrollY, maxScrollY);

        // Create a clipped buffer for children to render into
        // For simplicity, we'll render children with offset and let them clip naturally
        // A full implementation would use a separate buffer and copy

        // Draw vertical scrollbar
        if (_showVerticalScrollbar && contentHeight > viewportHeight)
        {
            DrawVerticalScrollbar(buffer, x + viewportWidth, y, viewportHeight, contentHeight);
        }

        // Draw horizontal scrollbar
        if (_showHorizontalScrollbar && contentWidth > viewportWidth)
        {
            DrawHorizontalScrollbar(buffer, x, y + viewportHeight, viewportWidth, contentWidth);
        }
    }

    private void DrawVerticalScrollbar(FrameBuffer buffer, int x, int y, int height, int contentHeight)
    {
        if (height <= 0 || contentHeight <= 0) return;

        // Calculate thumb size and position
        var thumbSize = Math.Max(1, height * height / contentHeight);
        var maxScroll = contentHeight - height;
        var thumbPos = maxScroll > 0 ? _scrollY * (height - thumbSize) / maxScroll : 0;

        for (int row = 0; row < height; row++)
        {
            var isThumb = row >= thumbPos && row < thumbPos + thumbSize;
            var color = isThumb ? ScrollbarThumbColor : ScrollbarTrackColor;
            buffer.SetCell(x, y + row, new Cell("│", color, color));
        }
    }

    private void DrawHorizontalScrollbar(FrameBuffer buffer, int x, int y, int width, int contentWidth)
    {
        if (width <= 0 || contentWidth <= 0) return;

        // Calculate thumb size and position
        var thumbSize = Math.Max(1, width * width / contentWidth);
        var maxScroll = contentWidth - width;
        var thumbPos = maxScroll > 0 ? _scrollX * (width - thumbSize) / maxScroll : 0;

        for (int col = 0; col < width; col++)
        {
            var isThumb = col >= thumbPos && col < thumbPos + thumbSize;
            var color = isThumb ? ScrollbarThumbColor : ScrollbarTrackColor;
            buffer.SetCell(x + col, y, new Cell("─", color, color));
        }
    }

    public override void Render(FrameBuffer buffer, int offsetX, int offsetY)
    {
        if (!Visible) return;

        var x = offsetX + (int)Layout.Layout.X;
        var y = offsetY + (int)Layout.Layout.Y;
        var width = (int)Layout.Layout.Width;
        var height = (int)Layout.Layout.Height;

        // Render background
        if (BackgroundColor.HasValue)
        {
            buffer.FillRect(x, y, width, height, BackgroundColor.Value);
        }

        // Render self (scrollbars)
        RenderSelf(buffer, x, y, width, height);

        // Render children with scroll offset
        // Note: This is a simplified implementation - children that extend beyond
        // the viewport will still render (they should be clipped in a full implementation)
        foreach (var child in Children)
        {
            child.Render(buffer, x - _scrollX, y - _scrollY);
        }
    }
}
