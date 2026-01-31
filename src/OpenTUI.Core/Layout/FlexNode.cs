namespace OpenTUI.Core.Layout;

/// <summary>
/// Computed layout rectangle for a node.
/// </summary>
public struct LayoutRect
{
    /// <summary>X position relative to parent.</summary>
    public float X { get; set; }
    
    /// <summary>Y position relative to parent.</summary>
    public float Y { get; set; }
    
    /// <summary>Computed width.</summary>
    public float Width { get; set; }
    
    /// <summary>Computed height.</summary>
    public float Height { get; set; }

    public LayoutRect(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>Right edge (X + Width).</summary>
    public float Right => X + Width;
    
    /// <summary>Bottom edge (Y + Height).</summary>
    public float Bottom => Y + Height;

    public static LayoutRect Zero => new(0, 0, 0, 0);

    public override string ToString() => $"({X}, {Y}, {Width}x{Height})";
}

/// <summary>
/// A node in the flex layout tree.
/// </summary>
public class FlexNode
{
    private readonly List<FlexNode> _children = new();
    private FlexNode? _parent;
    private bool _isDirty = true;
    private LayoutRect _layout;

    /// <summary>Unique identifier for this node.</summary>
    public string? Id { get; set; }

    // Layout style properties
    
    /// <summary>Display type.</summary>
    public Display Display { get; set; } = Display.Flex;
    
    /// <summary>Position type (relative or absolute).</summary>
    public PositionType PositionType { get; set; } = PositionType.Relative;
    
    /// <summary>Flex direction.</summary>
    public FlexDirection FlexDirection { get; set; } = FlexDirection.Row;
    
    /// <summary>Flex wrap behavior.</summary>
    public FlexWrap FlexWrap { get; set; } = FlexWrap.NoWrap;
    
    /// <summary>Justify content (main axis).</summary>
    public JustifyContent JustifyContent { get; set; } = JustifyContent.FlexStart;
    
    /// <summary>Align items (cross axis).</summary>
    public AlignItems AlignItems { get; set; } = AlignItems.Stretch;
    
    /// <summary>Align self override.</summary>
    public AlignSelf AlignSelf { get; set; } = AlignSelf.Auto;
    
    /// <summary>Align content (multi-line).</summary>
    public AlignContent AlignContent { get; set; } = AlignContent.FlexStart;

    // Flex item properties
    
    /// <summary>Flex grow factor.</summary>
    public float FlexGrow { get; set; } = 0;
    
    /// <summary>Flex shrink factor.</summary>
    public float FlexShrink { get; set; } = 1;
    
    /// <summary>Flex basis.</summary>
    public FlexValue FlexBasis { get; set; } = FlexValue.Auto;

    // Size constraints
    
    /// <summary>Width.</summary>
    public FlexValue Width { get; set; } = FlexValue.Auto;
    
    /// <summary>Height.</summary>
    public FlexValue Height { get; set; } = FlexValue.Auto;
    
    /// <summary>Minimum width.</summary>
    public FlexValue MinWidth { get; set; } = FlexValue.Undefined;
    
    /// <summary>Minimum height.</summary>
    public FlexValue MinHeight { get; set; } = FlexValue.Undefined;
    
    /// <summary>Maximum width.</summary>
    public FlexValue MaxWidth { get; set; } = FlexValue.Undefined;
    
    /// <summary>Maximum height.</summary>
    public FlexValue MaxHeight { get; set; } = FlexValue.Undefined;

    // Spacing
    
    /// <summary>Margin around the node.</summary>
    public Edges Margin { get; set; } = Edges.Zero;
    
    /// <summary>Padding inside the node.</summary>
    public Edges Padding { get; set; } = Edges.Zero;
    
    /// <summary>Border width (affects padding calculation).</summary>
    public Edges Border { get; set; } = Edges.Zero;

    // Position offsets (for absolute positioning)
    
    /// <summary>Top position offset.</summary>
    public FlexValue Top { get; set; } = FlexValue.Undefined;
    
    /// <summary>Right position offset.</summary>
    public FlexValue Right { get; set; } = FlexValue.Undefined;
    
    /// <summary>Bottom position offset.</summary>
    public FlexValue Bottom { get; set; } = FlexValue.Undefined;
    
    /// <summary>Left position offset.</summary>
    public FlexValue Left { get; set; } = FlexValue.Undefined;

    // Gap (spacing between flex items)
    
    /// <summary>Gap between items.</summary>
    public float Gap { get; set; } = 0;
    
    /// <summary>Row gap (for wrapped flex).</summary>
    public float RowGap { get; set; } = 0;
    
    /// <summary>Column gap.</summary>
    public float ColumnGap { get; set; } = 0;

    // Overflow
    
    /// <summary>Overflow behavior.</summary>
    public Overflow Overflow { get; set; } = Overflow.Visible;

    // Computed layout
    
    /// <summary>The computed layout rectangle.</summary>
    public LayoutRect Layout => _layout;

    /// <summary>Parent node.</summary>
    public FlexNode? Parent => _parent;

    /// <summary>Child nodes.</summary>
    public IReadOnlyList<FlexNode> Children => _children;

    /// <summary>Whether this node needs layout recalculation.</summary>
    public bool IsDirty => _isDirty;

    /// <summary>
    /// Adds a child node.
    /// </summary>
    public void AddChild(FlexNode child)
    {
        child._parent?.RemoveChild(child);
        child._parent = this;
        _children.Add(child);
        MarkDirty();
    }

    /// <summary>
    /// Inserts a child at a specific index.
    /// </summary>
    public void InsertChild(int index, FlexNode child)
    {
        child._parent?.RemoveChild(child);
        child._parent = this;
        _children.Insert(index, child);
        MarkDirty();
    }

    /// <summary>
    /// Removes a child node.
    /// </summary>
    public void RemoveChild(FlexNode child)
    {
        if (_children.Remove(child))
        {
            child._parent = null;
            MarkDirty();
        }
    }

    /// <summary>
    /// Removes all children.
    /// </summary>
    public void ClearChildren()
    {
        foreach (var child in _children)
            child._parent = null;
        _children.Clear();
        MarkDirty();
    }

    /// <summary>
    /// Marks this node and ancestors as needing layout recalculation.
    /// </summary>
    public void MarkDirty()
    {
        _isDirty = true;
        _parent?.MarkDirty();
    }

    /// <summary>
    /// Calculates layout with the given available size.
    /// </summary>
    public void CalculateLayout(float availableWidth, float availableHeight)
    {
        FlexLayoutEngine.Calculate(this, availableWidth, availableHeight);
        ClearDirtyRecursive();
    }

    internal void SetLayout(LayoutRect layout)
    {
        _layout = layout;
    }

    private void ClearDirtyRecursive()
    {
        _isDirty = false;
        foreach (var child in _children)
            child.ClearDirtyRecursive();
    }

    /// <summary>Whether this is a row direction.</summary>
    public bool IsRow => FlexDirection is FlexDirection.Row or FlexDirection.RowReverse;

    /// <summary>Whether this is reversed direction.</summary>
    public bool IsReverse => FlexDirection is FlexDirection.RowReverse or FlexDirection.ColumnReverse;

    /// <summary>
    /// Gets effective main axis gap.
    /// </summary>
    public float GetMainGap()
    {
        if (Gap > 0) return Gap;
        return IsRow ? ColumnGap : RowGap;
    }

    /// <summary>
    /// Gets effective cross axis gap.
    /// </summary>
    public float GetCrossGap()
    {
        if (Gap > 0) return Gap;
        return IsRow ? RowGap : ColumnGap;
    }
}
