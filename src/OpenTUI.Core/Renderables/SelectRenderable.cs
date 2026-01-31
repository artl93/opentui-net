using OpenTUI.Core.Colors;
using OpenTUI.Core.Rendering;

namespace OpenTUI.Core.Renderables;

/// <summary>
/// A list selection component.
/// </summary>
public class SelectRenderable : Renderable
{
    private readonly List<string> _items = new();
    private int _selectedIndex;
    private int _scrollOffset;
    private string _emptyText = "(No items)";

    /// <summary>The list of selectable items.</summary>
    public IReadOnlyList<string> Items => _items;

    /// <summary>Currently selected index (-1 if no selection).</summary>
    public int SelectedIndex
    {
        get => _items.Count > 0 ? _selectedIndex : -1;
        set
        {
            var newIndex = _items.Count > 0 ? Math.Clamp(value, 0, _items.Count - 1) : -1;
            if (_selectedIndex != newIndex)
            {
                _selectedIndex = newIndex;
                EnsureSelectedVisible();
                MarkDirty();
                SelectionChanged?.Invoke(this, SelectedItem);
            }
        }
    }

    /// <summary>Currently selected item (null if no selection).</summary>
    public string? SelectedItem => _selectedIndex >= 0 && _selectedIndex < _items.Count 
        ? _items[_selectedIndex] 
        : null;

    /// <summary>Text to display when the list is empty.</summary>
    public string EmptyText
    {
        get => _emptyText;
        set
        {
            if (_emptyText != value)
            {
                _emptyText = value ?? string.Empty;
                MarkDirty();
            }
        }
    }

    /// <summary>Color for the selected item background.</summary>
    public RGBA SelectedBackground { get; set; } = RGBA.FromValues(0.2f, 0.4f, 0.8f);

    /// <summary>Color for the selected item foreground.</summary>
    public RGBA SelectedForeground { get; set; } = RGBA.White;

    /// <summary>Prefix shown before the selected item.</summary>
    public string SelectionPrefix { get; set; } = "› ";

    /// <summary>Prefix shown before unselected items.</summary>
    public string ItemPrefix { get; set; } = "  ";

    /// <summary>Event raised when selection changes.</summary>
    public event EventHandler<string?>? SelectionChanged;

    /// <summary>Event raised when an item is activated (Enter pressed).</summary>
    public event EventHandler<string?>? ItemActivated;

    public SelectRenderable()
    {
        Focusable = true;
    }

    /// <summary>Adds an item to the list.</summary>
    public void AddItem(string item)
    {
        _items.Add(item);
        if (_items.Count == 1)
            _selectedIndex = 0;
        MarkDirty();
    }

    /// <summary>Adds multiple items to the list.</summary>
    public void AddItems(IEnumerable<string> items)
    {
        var hadItems = _items.Count > 0;
        _items.AddRange(items);
        if (!hadItems && _items.Count > 0)
            _selectedIndex = 0;
        MarkDirty();
    }

    /// <summary>Removes an item from the list.</summary>
    public bool RemoveItem(string item)
    {
        var index = _items.IndexOf(item);
        if (index >= 0)
        {
            _items.RemoveAt(index);
            if (_selectedIndex >= _items.Count)
                _selectedIndex = _items.Count - 1;
            MarkDirty();
            return true;
        }
        return false;
    }

    /// <summary>Clears all items.</summary>
    public void ClearItems()
    {
        _items.Clear();
        _selectedIndex = -1;
        _scrollOffset = 0;
        MarkDirty();
    }

    /// <summary>Sets the items, replacing any existing items.</summary>
    public void SetItems(IEnumerable<string> items)
    {
        _items.Clear();
        _items.AddRange(items);
        _selectedIndex = _items.Count > 0 ? 0 : -1;
        _scrollOffset = 0;
        MarkDirty();
    }

    /// <summary>Selects the previous item.</summary>
    public void SelectPrevious()
    {
        if (_items.Count > 0 && _selectedIndex > 0)
        {
            _selectedIndex--;
            EnsureSelectedVisible();
            MarkDirty();
            SelectionChanged?.Invoke(this, SelectedItem);
        }
    }

    /// <summary>Selects the next item.</summary>
    public void SelectNext()
    {
        if (_items.Count > 0 && _selectedIndex < _items.Count - 1)
        {
            _selectedIndex++;
            EnsureSelectedVisible();
            MarkDirty();
            SelectionChanged?.Invoke(this, SelectedItem);
        }
    }

    /// <summary>Selects the first item.</summary>
    public void SelectFirst()
    {
        if (_items.Count > 0 && _selectedIndex != 0)
        {
            _selectedIndex = 0;
            _scrollOffset = 0;
            MarkDirty();
            SelectionChanged?.Invoke(this, SelectedItem);
        }
    }

    /// <summary>Selects the last item.</summary>
    public void SelectLast()
    {
        if (_items.Count > 0 && _selectedIndex != _items.Count - 1)
        {
            _selectedIndex = _items.Count - 1;
            EnsureSelectedVisible();
            MarkDirty();
            SelectionChanged?.Invoke(this, SelectedItem);
        }
    }

    /// <summary>Activates the currently selected item.</summary>
    public void Activate()
    {
        ItemActivated?.Invoke(this, SelectedItem);
    }

    protected override void RenderSelf(FrameBuffer buffer, int x, int y, int width, int height)
    {
        if (width <= 0 || height <= 0) return;

        var fg = ForegroundColor ?? RGBA.White;
        var bg = BackgroundColor;

        if (_items.Count == 0)
        {
            // Show empty text
            var text = _emptyText.Length <= width ? _emptyText : _emptyText[..width];
            for (int i = 0; i < width; i++)
            {
                var ch = i < text.Length ? text[i].ToString() : " ";
                buffer.SetCell(x + i, y, new Cell(ch, RGBA.FromValues(0.5f, 0.5f, 0.5f), bg ?? buffer.GetCell(y, x).Background));
            }
            return;
        }

        var prefixLen = Math.Max(SelectionPrefix.Length, ItemPrefix.Length);
        var maxTextWidth = width - prefixLen;

        for (int row = 0; row < height && _scrollOffset + row < _items.Count; row++)
        {
            var itemIndex = _scrollOffset + row;
            var item = _items[itemIndex];
            var isSelected = itemIndex == _selectedIndex;
            var prefix = isSelected ? SelectionPrefix : ItemPrefix;

            var itemFg = isSelected && IsFocused ? SelectedForeground : fg;
            var itemBg = isSelected && IsFocused ? SelectedBackground : (bg ?? buffer.GetCell(y + row, x).Background);

            // Draw prefix
            for (int i = 0; i < prefixLen; i++)
            {
                var ch = i < prefix.Length ? prefix[i].ToString() : " ";
                buffer.SetCell(x + i, y + row, new Cell(ch, itemFg, itemBg));
            }

            // Draw item text
            var text = item.Length <= maxTextWidth ? item : item[..(maxTextWidth - 1)] + "…";
            for (int i = 0; i < maxTextWidth; i++)
            {
                var ch = i < text.Length ? text[i].ToString() : " ";
                buffer.SetCell(x + prefixLen + i, y + row, new Cell(ch, itemFg, itemBg));
            }
        }
    }

    private void EnsureSelectedVisible()
    {
        var height = (int)Layout.Layout.Height;
        if (height <= 0) return;

        if (_selectedIndex < _scrollOffset)
        {
            _scrollOffset = _selectedIndex;
        }
        else if (_selectedIndex >= _scrollOffset + height)
        {
            _scrollOffset = _selectedIndex - height + 1;
        }
    }
}
