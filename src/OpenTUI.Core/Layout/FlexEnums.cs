namespace OpenTUI.Core.Layout;

/// <summary>
/// Flex direction for layout.
/// </summary>
public enum FlexDirection
{
    Row,
    Column,
    RowReverse,
    ColumnReverse
}

/// <summary>
/// Justify content alignment along the main axis.
/// </summary>
public enum JustifyContent
{
    FlexStart,
    FlexEnd,
    Center,
    SpaceBetween,
    SpaceAround,
    SpaceEvenly
}

/// <summary>
/// Align items along the cross axis.
/// </summary>
public enum AlignItems
{
    FlexStart,
    FlexEnd,
    Center,
    Stretch,
    Baseline
}

/// <summary>
/// Align self override for individual items.
/// </summary>
public enum AlignSelf
{
    Auto,
    FlexStart,
    FlexEnd,
    Center,
    Stretch,
    Baseline
}

/// <summary>
/// Align content for multi-line flex containers.
/// </summary>
public enum AlignContent
{
    FlexStart,
    FlexEnd,
    Center,
    Stretch,
    SpaceBetween,
    SpaceAround
}

/// <summary>
/// Flex wrap behavior.
/// </summary>
public enum FlexWrap
{
    NoWrap,
    Wrap,
    WrapReverse
}

/// <summary>
/// Position type for layout.
/// </summary>
public enum PositionType
{
    Relative,
    Absolute
}

/// <summary>
/// Display type.
/// </summary>
public enum Display
{
    Flex,
    None
}

/// <summary>
/// Overflow behavior.
/// </summary>
public enum Overflow
{
    Visible,
    Hidden,
    Scroll
}
