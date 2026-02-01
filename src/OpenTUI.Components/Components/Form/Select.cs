using OpenTUI.Components.Components.Core;
using OpenTUI.Components.Theme;
using OpenTUI.Core.Colors;
using OpenTUI.Core.Rendering;
using BorderStyle = OpenTUI.Core.Renderables.BorderStyle;

namespace OpenTUI.Components.Components.Form;

/// <summary>
/// A themed dropdown select component.
/// </summary>
public class Select<T> : Component where T : notnull
{
    private int _selectedIndex = -1;
    private bool _isOpen;
    private int _highlightedIndex;
    
    /// <summary>Label displayed above the select.</summary>
    public string? Label { get; set; }
    
    /// <summary>Placeholder when no value selected.</summary>
    public string Placeholder { get; set; } = "Select...";
    
    /// <summary>Available options.</summary>
    public IReadOnlyList<T> Options { get; set; } = Array.Empty<T>();
    
    /// <summary>Function to display option as string.</summary>
    public Func<T, string>? DisplayFunc { get; set; }
    
    /// <summary>Width of the select box.</summary>
    public int Width { get; set; } = 20;
    
    /// <summary>Maximum visible options when open.</summary>
    public int MaxVisibleOptions { get; set; } = 5;
    
    /// <summary>Currently selected value.</summary>
    public T? Value
    {
        get => _selectedIndex >= 0 && _selectedIndex < Options.Count ? Options[_selectedIndex] : default;
        set
        {
            var index = value != null ? Options.ToList().IndexOf(value) : -1;
            if (_selectedIndex != index)
            {
                _selectedIndex = index;
                OnChange?.Invoke(value);
                MarkDirty();
            }
        }
    }
    
    /// <summary>Change handler.</summary>
    public Action<T?>? OnChange { get; set; }
    
    private string GetDisplayText(T item) => DisplayFunc?.Invoke(item) ?? item.ToString() ?? "";
    
    /// <summary>Opens the dropdown.</summary>
    public void Open()
    {
        if (!Disabled)
        {
            _isOpen = true;
            _highlightedIndex = Math.Max(0, _selectedIndex);
            MarkDirty();
        }
    }
    
    /// <summary>Closes the dropdown.</summary>
    public void Close()
    {
        _isOpen = false;
        MarkDirty();
    }
    
    /// <summary>Toggles the dropdown.</summary>
    public void Toggle()
    {
        if (_isOpen) Close();
        else Open();
    }
    
    /// <summary>Selects the highlighted option.</summary>
    public void SelectHighlighted()
    {
        if (_isOpen && _highlightedIndex >= 0 && _highlightedIndex < Options.Count)
        {
            _selectedIndex = _highlightedIndex;
            OnChange?.Invoke(Options[_selectedIndex]);
            Close();
        }
    }
    
    /// <summary>Handles keyboard navigation.</summary>
    public void HandleKey(ConsoleKeyInfo key)
    {
        if (Disabled) return;
        
        switch (key.Key)
        {
            case ConsoleKey.Enter:
            case ConsoleKey.Spacebar:
                if (_isOpen) SelectHighlighted();
                else Open();
                break;
                
            case ConsoleKey.Escape:
                Close();
                break;
                
            case ConsoleKey.UpArrow:
                if (_isOpen)
                {
                    _highlightedIndex = Math.Max(0, _highlightedIndex - 1);
                    MarkDirty();
                }
                break;
                
            case ConsoleKey.DownArrow:
                if (_isOpen)
                {
                    _highlightedIndex = Math.Min(Options.Count - 1, _highlightedIndex + 1);
                    MarkDirty();
                }
                else
                {
                    Open();
                }
                break;
        }
    }
    
    protected override void RenderSelf(FrameBuffer buffer, int x, int y, int renderWidth, int renderHeight)
    {
        var currentY = y;
        
        var borderColor = Focused && !_isOpen
            ? GetColor(ColorToken.BorderSelected)
            : GetColor(ColorToken.BorderBase);
        
        // Label
        if (!string.IsNullOrEmpty(Label))
        {
            buffer.DrawText(Label, x, currentY, GetColor(ColorToken.TextWeak));
            currentY++;
        }
        
        // Select box
        var boxWidth = Width + 4; // +2 for arrow, +2 for padding
        
        // Background
        buffer.FillRect(x, currentY, boxWidth, 1, GetColor(ColorToken.InputBase));
        
        // Border
        DrawThemedBox(buffer, x, currentY, boxWidth, 3,
            background: GetColor(ColorToken.InputBase),
            borderColor: _isOpen ? GetColor(ColorToken.BorderSelected) : borderColor,
            borderStyle: BorderStyle.Rounded);
        
        currentY++; // Inside border
        
        // Selected value or placeholder
        var displayText = _selectedIndex >= 0 
            ? GetDisplayText(Options[_selectedIndex])
            : Placeholder;
        var textColor = _selectedIndex >= 0
            ? GetColor(ColorToken.TextStrong)
            : GetColor(ColorToken.TextWeak);
        
        if (Disabled) textColor = GetColor(ColorToken.TextDisabled);
        
        var truncated = displayText.Length > Width 
            ? displayText.Substring(0, Width - 1) + "…"
            : displayText.PadRight(Width);
        
        buffer.DrawText(truncated, x + 1, currentY, textColor);
        
        // Dropdown arrow
        var arrowColor = Disabled ? GetColor(ColorToken.IconDisabled) : GetColor(ColorToken.IconBase);
        buffer.DrawText(_isOpen ? "▲" : "▼", x + Width + 2, currentY, arrowColor);
        
        currentY += 2; // Move past border
        
        // Dropdown options
        if (_isOpen && Options.Count > 0)
        {
            var visibleCount = Math.Min(MaxVisibleOptions, Options.Count);
            var startIndex = Math.Max(0, _highlightedIndex - visibleCount + 1);
            startIndex = Math.Min(startIndex, Options.Count - visibleCount);
            
            // Dropdown background
            buffer.FillRect(x, currentY, boxWidth, visibleCount, GetColor(ColorToken.SurfaceOverlay));
            
            // Draw border around dropdown
            for (int i = 0; i < visibleCount; i++)
            {
                buffer.SetCell(x, currentY + i, new Cell("│", GetColor(ColorToken.BorderBase)));
                buffer.SetCell(x + boxWidth - 1, currentY + i, new Cell("│", GetColor(ColorToken.BorderBase)));
            }
            
            // Bottom border
            buffer.SetCell(x, currentY + visibleCount, new Cell("╰", GetColor(ColorToken.BorderBase)));
            buffer.SetCell(x + boxWidth - 1, currentY + visibleCount, new Cell("╯", GetColor(ColorToken.BorderBase)));
            for (int i = 1; i < boxWidth - 1; i++)
            {
                buffer.SetCell(x + i, currentY + visibleCount, new Cell("─", GetColor(ColorToken.BorderBase)));
            }
            
            // Options
            for (int i = 0; i < visibleCount; i++)
            {
                var optionIndex = startIndex + i;
                var isHighlighted = optionIndex == _highlightedIndex;
                var isSelected = optionIndex == _selectedIndex;
                
                var optionBg = isHighlighted ? GetColor(ColorToken.GhostHover) : RGBA.Transparent;
                var optionFg = isSelected 
                    ? GetColor(ColorToken.PrimaryBase) 
                    : GetColor(ColorToken.TextBase);
                
                if (isHighlighted)
                {
                    buffer.FillRect(x + 1, currentY + i, boxWidth - 2, 1, optionBg);
                }
                
                var optionText = GetDisplayText(Options[optionIndex]);
                var truncatedOption = optionText.Length > Width
                    ? optionText.Substring(0, Width - 1) + "…"
                    : optionText.PadRight(Width);
                
                buffer.DrawText(truncatedOption, x + 1, currentY + i, optionFg);
                
                if (isSelected)
                {
                    buffer.DrawText("✓", x + Width + 1, currentY + i, GetColor(ColorToken.PrimaryBase));
                }
            }
        }
    }
}
