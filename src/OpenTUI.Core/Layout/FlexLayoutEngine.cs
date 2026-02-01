namespace OpenTUI.Core.Layout;

/// <summary>
/// Flex layout calculation engine.
/// Implements a simplified CSS Flexbox algorithm.
/// </summary>
public static class FlexLayoutEngine
{
    /// <summary>
    /// Calculates layout for a node tree.
    /// </summary>
    public static void Calculate(FlexNode node, float availableWidth, float availableHeight)
    {
        // Start layout from root
        LayoutNode(node, availableWidth, availableHeight, 0, 0);
    }

    private static void LayoutNode(FlexNode node, float availableWidth, float availableHeight, float x, float y)
    {
        // Skip hidden nodes
        if (node.Display == Display.None)
        {
            node.SetLayout(new LayoutRect(x, y, 0, 0));
            return;
        }

        // Resolve own dimensions
        var width = ResolveSize(node.Width, availableWidth, node.MinWidth, node.MaxWidth, availableWidth);
        var height = ResolveSize(node.Height, availableHeight, node.MinHeight, node.MaxHeight, availableHeight);

        // Account for padding and border
        var paddingH = node.Padding.GetHorizontal(width) + node.Border.GetHorizontal(width);
        var paddingV = node.Padding.GetVertical(height) + node.Border.GetVertical(height);

        var contentWidth = width - paddingH;
        var contentHeight = height - paddingV;

        // Handle absolute vs relative children separately
        var relativeChildren = node.Children.Where(c => c.PositionType == PositionType.Relative && c.Display != Display.None).ToList();
        var absoluteChildren = node.Children.Where(c => c.PositionType == PositionType.Absolute && c.Display != Display.None).ToList();

        // Layout relative children with flexbox
        if (relativeChildren.Count > 0)
        {
            LayoutFlexChildren(node, relativeChildren, contentWidth, contentHeight, paddingH, paddingV);
        }

        // Layout absolute children
        foreach (var child in absoluteChildren)
        {
            LayoutAbsoluteChild(child, width, height);
        }

        // If height is auto, compute from children
        if (node.Height.IsAuto && relativeChildren.Count > 0)
        {
            height = ComputeAutoHeight(node, relativeChildren) + paddingV;
            height = ApplyConstraints(height, node.MinHeight, node.MaxHeight, availableHeight);
        }

        // If width is auto, compute from children
        if (node.Width.IsAuto && relativeChildren.Count > 0)
        {
            width = ComputeAutoWidth(node, relativeChildren) + paddingH;
            width = ApplyConstraints(width, node.MinWidth, node.MaxWidth, availableWidth);
        }

        node.SetLayout(new LayoutRect(x, y, width, height));
    }

    private static void LayoutFlexChildren(FlexNode parent, List<FlexNode> children, float contentWidth, float contentHeight, float paddingH, float paddingV)
    {
        var isRow = parent.IsRow;
        var mainSize = isRow ? contentWidth : contentHeight;
        var crossSize = isRow ? contentHeight : contentWidth;
        var gap = parent.GetMainGap();

        // Calculate flex basis for each child
        var childInfos = children.Select(child => new ChildInfo
        {
            Node = child,
            FlexBasis = CalculateFlexBasis(child, mainSize, isRow),
            FlexGrow = child.FlexGrow,
            FlexShrink = child.FlexShrink,
            MainMargin = isRow
                ? child.Margin.Left.ResolveOrDefault(contentWidth, 0) + child.Margin.Right.ResolveOrDefault(contentWidth, 0)
                : child.Margin.Top.ResolveOrDefault(contentHeight, 0) + child.Margin.Bottom.ResolveOrDefault(contentHeight, 0),
            CrossMargin = isRow
                ? child.Margin.Top.ResolveOrDefault(contentHeight, 0) + child.Margin.Bottom.ResolveOrDefault(contentHeight, 0)
                : child.Margin.Left.ResolveOrDefault(contentWidth, 0) + child.Margin.Right.ResolveOrDefault(contentWidth, 0),
        }).ToList();

        // Calculate total used space
        var totalBasis = childInfos.Sum(c => c.FlexBasis + c.MainMargin);
        var totalGaps = gap * Math.Max(0, children.Count - 1);
        var freeSpace = mainSize - totalBasis - totalGaps;

        // Distribute free space
        if (freeSpace > 0)
        {
            // Grow
            var totalGrow = childInfos.Sum(c => c.FlexGrow);
            if (totalGrow > 0)
            {
                foreach (var info in childInfos)
                {
                    info.MainSize = info.FlexBasis + (freeSpace * info.FlexGrow / totalGrow);
                }
            }
            else
            {
                foreach (var info in childInfos)
                    info.MainSize = info.FlexBasis;
            }
        }
        else if (freeSpace < 0)
        {
            // Shrink
            var totalShrink = childInfos.Sum(c => c.FlexShrink * c.FlexBasis);
            if (totalShrink > 0)
            {
                foreach (var info in childInfos)
                {
                    var shrinkRatio = (info.FlexShrink * info.FlexBasis) / totalShrink;
                    info.MainSize = Math.Max(0, info.FlexBasis + (freeSpace * shrinkRatio));
                }
            }
            else
            {
                foreach (var info in childInfos)
                    info.MainSize = info.FlexBasis;
            }
        }
        else
        {
            foreach (var info in childInfos)
                info.MainSize = info.FlexBasis;
        }

        // Calculate cross sizes
        foreach (var info in childInfos)
        {
            var alignSelf = info.Node.AlignSelf == AlignSelf.Auto
                ? (AlignSelf)parent.AlignItems
                : info.Node.AlignSelf;

            if (alignSelf == AlignSelf.Stretch)
            {
                info.CrossSize = crossSize - info.CrossMargin;
            }
            else
            {
                // Use child's specified cross size
                info.CrossSize = isRow
                    ? ResolveSize(info.Node.Height, crossSize, info.Node.MinHeight, info.Node.MaxHeight, crossSize - info.CrossMargin)
                    : ResolveSize(info.Node.Width, crossSize, info.Node.MinWidth, info.Node.MaxWidth, crossSize - info.CrossMargin);
            }
        }

        // Position children
        var mainOffset = CalculateMainStartOffset(parent.JustifyContent, freeSpace, children.Count, gap);
        var spacingBetween = CalculateSpacingBetween(parent.JustifyContent, freeSpace, children.Count);

        // Handle reverse
        var orderedChildren = parent.IsReverse ? childInfos.AsEnumerable().Reverse().ToList() : childInfos;

        var paddingLeft = parent.Padding.Left.ResolveOrDefault(contentWidth, 0) + parent.Border.Left.ResolveOrDefault(contentWidth, 0);
        var paddingTop = parent.Padding.Top.ResolveOrDefault(contentHeight, 0) + parent.Border.Top.ResolveOrDefault(contentHeight, 0);

        var currentMain = mainOffset;
        foreach (var info in orderedChildren)
        {
            var marginStart = isRow
                ? info.Node.Margin.Left.ResolveOrDefault(contentWidth, 0)
                : info.Node.Margin.Top.ResolveOrDefault(contentHeight, 0);
            var crossMarginStart = isRow
                ? info.Node.Margin.Top.ResolveOrDefault(contentHeight, 0)
                : info.Node.Margin.Left.ResolveOrDefault(contentWidth, 0);

            currentMain += marginStart;

            // Calculate cross offset based on alignment
            var alignSelf = info.Node.AlignSelf == AlignSelf.Auto
                ? (AlignSelf)parent.AlignItems
                : info.Node.AlignSelf;
            var crossOffset = CalculateCrossOffset(alignSelf, crossSize, info.CrossSize + info.CrossMargin) + crossMarginStart;

            float childX, childY;
            if (isRow)
            {
                childX = paddingLeft + currentMain;
                childY = paddingTop + crossOffset;
            }
            else
            {
                childX = paddingLeft + crossOffset;
                childY = paddingTop + currentMain;
            }

            // Recursively layout child
            LayoutNode(info.Node, info.MainSize, info.CrossSize, childX, childY);

            var marginEnd = isRow
                ? info.Node.Margin.Right.ResolveOrDefault(contentWidth, 0)
                : info.Node.Margin.Bottom.ResolveOrDefault(contentHeight, 0);

            currentMain += info.MainSize + marginEnd + gap + spacingBetween;
        }
    }

    private static void LayoutAbsoluteChild(FlexNode child, float parentWidth, float parentHeight)
    {
        // Resolve size
        var width = ResolveSize(child.Width, parentWidth, child.MinWidth, child.MaxWidth, parentWidth);
        var height = ResolveSize(child.Height, parentHeight, child.MinHeight, child.MaxHeight, parentHeight);

        // Position based on offsets
        float x = 0, y = 0;

        if (child.Left.IsDefined)
            x = child.Left.Resolve(parentWidth);
        else if (child.Right.IsDefined)
            x = parentWidth - width - child.Right.Resolve(parentWidth);

        if (child.Top.IsDefined)
            y = child.Top.Resolve(parentHeight);
        else if (child.Bottom.IsDefined)
            y = parentHeight - height - child.Bottom.Resolve(parentHeight);

        LayoutNode(child, width, height, x, y);
    }

    private static float CalculateFlexBasis(FlexNode node, float mainSize, bool isRow)
    {
        if (node.FlexBasis.IsDefined && !node.FlexBasis.IsAuto)
        {
            return node.FlexBasis.Resolve(mainSize);
        }

        // Fall back to width/height
        var sizeValue = isRow ? node.Width : node.Height;
        if (sizeValue.IsDefined && !sizeValue.IsAuto)
        {
            return sizeValue.Resolve(mainSize);
        }

        // Auto: use content size (for now, return 0 and let children determine)
        return 0;
    }

    private static float ResolveSize(FlexValue size, float parentSize, FlexValue minSize, FlexValue maxSize, float defaultSize)
    {
        float result;

        if (size.IsDefined && !size.IsAuto)
        {
            result = size.Resolve(parentSize);
        }
        else
        {
            result = defaultSize;
        }

        return ApplyConstraints(result, minSize, maxSize, parentSize);
    }

    private static float ApplyConstraints(float size, FlexValue minSize, FlexValue maxSize, float parentSize)
    {
        if (minSize.IsDefined)
            size = Math.Max(size, minSize.Resolve(parentSize));
        if (maxSize.IsDefined)
            size = Math.Min(size, maxSize.Resolve(parentSize));
        return size;
    }

    private static float CalculateMainStartOffset(JustifyContent justify, float freeSpace, int itemCount, float gap)
    {
        if (freeSpace <= 0 || itemCount == 0)
            return 0;

        return justify switch
        {
            JustifyContent.FlexStart => 0,
            JustifyContent.FlexEnd => freeSpace,
            JustifyContent.Center => freeSpace / 2,
            JustifyContent.SpaceBetween => 0,
            JustifyContent.SpaceAround => freeSpace / itemCount / 2,
            JustifyContent.SpaceEvenly => freeSpace / (itemCount + 1),
            _ => 0
        };
    }

    private static float CalculateSpacingBetween(JustifyContent justify, float freeSpace, int itemCount)
    {
        if (freeSpace <= 0 || itemCount <= 1)
            return 0;

        return justify switch
        {
            JustifyContent.SpaceBetween => freeSpace / (itemCount - 1),
            JustifyContent.SpaceAround => freeSpace / itemCount,
            JustifyContent.SpaceEvenly => freeSpace / (itemCount + 1),
            _ => 0
        };
    }

    private static float CalculateCrossOffset(AlignSelf align, float crossSize, float itemCrossSize)
    {
        return align switch
        {
            AlignSelf.FlexStart => 0,
            AlignSelf.FlexEnd => crossSize - itemCrossSize,
            AlignSelf.Center => (crossSize - itemCrossSize) / 2,
            AlignSelf.Stretch => 0,
            AlignSelf.Baseline => 0, // TODO: Implement baseline alignment
            _ => 0
        };
    }

    private static float ComputeAutoHeight(FlexNode parent, List<FlexNode> children)
    {
        if (parent.IsRow)
        {
            // Max of children heights
            return children.Max(c => c.Layout.Y + c.Layout.Height);
        }
        else
        {
            // Sum of children heights
            return children.Sum(c => c.Layout.Height +
                c.Margin.Top.ResolveOrDefault(0, 0) +
                c.Margin.Bottom.ResolveOrDefault(0, 0)) +
                parent.GetMainGap() * Math.Max(0, children.Count - 1);
        }
    }

    private static float ComputeAutoWidth(FlexNode parent, List<FlexNode> children)
    {
        if (parent.IsRow)
        {
            // Sum of children widths
            return children.Sum(c => c.Layout.Width +
                c.Margin.Left.ResolveOrDefault(0, 0) +
                c.Margin.Right.ResolveOrDefault(0, 0)) +
                parent.GetMainGap() * Math.Max(0, children.Count - 1);
        }
        else
        {
            // Max of children widths
            return children.Max(c => c.Layout.X + c.Layout.Width);
        }
    }

    private class ChildInfo
    {
        public FlexNode Node { get; set; } = null!;
        public float FlexBasis { get; set; }
        public float FlexGrow { get; set; }
        public float FlexShrink { get; set; }
        public float MainSize { get; set; }
        public float CrossSize { get; set; }
        public float MainMargin { get; set; }
        public float CrossMargin { get; set; }
    }
}
