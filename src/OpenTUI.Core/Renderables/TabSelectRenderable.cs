using OpenTUI.Core.Colors;
using OpenTUI.Core.Rendering;

namespace OpenTUI.Core.Renderables;

/// <summary>
/// A horizontal tab selection component.
/// </summary>
public class TabSelectRenderable : Renderable
{
    private readonly List<string> _tabs = new();
    private int _selectedIndex;

    /// <summary>The list of tab labels.</summary>
    public IReadOnlyList<string> Tabs => _tabs;

    /// <summary>Currently selected tab index.</summary>
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            var newIndex = _tabs.Count > 0 ? Math.Clamp(value, 0, _tabs.Count - 1) : -1;
            if (_selectedIndex != newIndex)
            {
                _selectedIndex = newIndex;
                MarkDirty();
                SelectionChanged?.Invoke(this, SelectedTab);
            }
        }
    }

    /// <summary>Currently selected tab label.</summary>
    public string? SelectedTab => _selectedIndex >= 0 && _selectedIndex < _tabs.Count
        ? _tabs[_selectedIndex]
        : null;

    /// <summary>Color for selected tab background.</summary>
    public RGBA SelectedBackground { get; set; } = RGBA.FromValues(0.2f, 0.4f, 0.8f);

    /// <summary>Color for selected tab foreground.</summary>
    public RGBA SelectedForeground { get; set; } = RGBA.White;

    /// <summary>Separator between tabs.</summary>
    public string Separator { get; set; } = " │ ";

    /// <summary>Padding around tab labels.</summary>
    public int TabPadding { get; set; } = 1;

    /// <summary>Event raised when selection changes.</summary>
    public event EventHandler<string?>? SelectionChanged;

    public TabSelectRenderable()
    {
        Focusable = true;
    }

    /// <summary>Adds a tab.</summary>
    public void AddTab(string label)
    {
        _tabs.Add(label);
        if (_tabs.Count == 1)
            _selectedIndex = 0;
        MarkDirty();
    }

    /// <summary>Adds multiple tabs.</summary>
    public void AddTabs(IEnumerable<string> labels)
    {
        var hadTabs = _tabs.Count > 0;
        _tabs.AddRange(labels);
        if (!hadTabs && _tabs.Count > 0)
            _selectedIndex = 0;
        MarkDirty();
    }

    /// <summary>Sets tabs, replacing existing ones.</summary>
    public void SetTabs(IEnumerable<string> labels)
    {
        _tabs.Clear();
        _tabs.AddRange(labels);
        _selectedIndex = _tabs.Count > 0 ? 0 : -1;
        MarkDirty();
    }

    /// <summary>Clears all tabs.</summary>
    public void ClearTabs()
    {
        _tabs.Clear();
        _selectedIndex = -1;
        MarkDirty();
    }

    /// <summary>Selects the previous tab.</summary>
    public void SelectPrevious()
    {
        if (_tabs.Count > 0 && _selectedIndex > 0)
        {
            _selectedIndex--;
            MarkDirty();
            SelectionChanged?.Invoke(this, SelectedTab);
        }
    }

    /// <summary>Selects the next tab.</summary>
    public void SelectNext()
    {
        if (_tabs.Count > 0 && _selectedIndex < _tabs.Count - 1)
        {
            _selectedIndex++;
            MarkDirty();
            SelectionChanged?.Invoke(this, SelectedTab);
        }
    }

    protected override void RenderSelf(FrameBuffer buffer, int x, int y, int width, int height)
    {
        if (width <= 0 || height <= 0 || _tabs.Count == 0) return;

        var fg = ForegroundColor ?? RGBA.White;
        var bg = BackgroundColor ?? buffer.GetCell(y, x).Background;

        int currentX = x;
        for (int i = 0; i < _tabs.Count && currentX < x + width; i++)
        {
            var tab = _tabs[i];
            var isSelected = i == _selectedIndex;
            var tabFg = isSelected && IsFocused ? SelectedForeground : fg;
            var tabBg = isSelected && IsFocused ? SelectedBackground : bg;

            // Draw padding before
            for (int p = 0; p < TabPadding && currentX < x + width; p++)
            {
                buffer.SetCell(currentX++, y, new Cell(" ", tabFg, tabBg));
            }

            // Draw tab label
            foreach (var ch in tab)
            {
                if (currentX >= x + width) break;
                buffer.SetCell(currentX++, y, new Cell(ch.ToString(), tabFg, tabBg));
            }

            // Draw padding after
            for (int p = 0; p < TabPadding && currentX < x + width; p++)
            {
                buffer.SetCell(currentX++, y, new Cell(" ", tabFg, tabBg));
            }

            // Draw separator (except after last tab)
            if (i < _tabs.Count - 1)
            {
                foreach (var ch in Separator)
                {
                    if (currentX >= x + width) break;
                    buffer.SetCell(currentX++, y, new Cell(ch.ToString(), fg, bg));
                }
            }
        }

        // Fill remaining width
        while (currentX < x + width)
        {
            buffer.SetCell(currentX++, y, new Cell(" ", fg, bg));
        }
    }
}
