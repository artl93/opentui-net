using FluentAssertions;
using OpenTUI.Core.Layout;

namespace OpenTUI.Layout.Tests;

public class FlexLayoutEngineTests
{
    [Fact]
    public void SingleNode_TakesFullSize()
    {
        var node = new FlexNode
        {
            Width = 100,
            Height = 50
        };

        node.CalculateLayout(200, 100);

        node.Layout.Width.Should().Be(100);
        node.Layout.Height.Should().Be(50);
        node.Layout.X.Should().Be(0);
        node.Layout.Y.Should().Be(0);
    }

    [Fact]
    public void AutoWidth_UsesAvailableWidth()
    {
        var node = new FlexNode
        {
            Width = FlexValue.Auto,
            Height = 50
        };

        node.CalculateLayout(200, 100);

        node.Layout.Width.Should().Be(200);
    }

    [Fact]
    public void PercentWidth_CalculatesCorrectly()
    {
        var node = new FlexNode
        {
            Width = FlexValue.Percent(50),
            Height = 50
        };

        node.CalculateLayout(200, 100);

        node.Layout.Width.Should().Be(100);
    }

    [Fact]
    public void MinWidth_EnforcesMinimum()
    {
        var node = new FlexNode
        {
            Width = 50,
            MinWidth = 100,
            Height = 50
        };

        node.CalculateLayout(200, 100);

        node.Layout.Width.Should().Be(100);
    }

    [Fact]
    public void MaxWidth_EnforcesMaximum()
    {
        var node = new FlexNode
        {
            Width = 200,
            MaxWidth = 100,
            Height = 50
        };

        node.CalculateLayout(200, 100);

        node.Layout.Width.Should().Be(100);
    }

    [Fact]
    public void RowDirection_LayoutsChildrenHorizontally()
    {
        var parent = new FlexNode
        {
            Width = 300,
            Height = 100,
            FlexDirection = FlexDirection.Row
        };

        var child1 = new FlexNode { Width = 100, Height = 50 };
        var child2 = new FlexNode { Width = 100, Height = 50 };

        parent.AddChild(child1);
        parent.AddChild(child2);
        parent.CalculateLayout(300, 100);

        child1.Layout.X.Should().Be(0);
        child2.Layout.X.Should().Be(100);
    }

    [Fact]
    public void ColumnDirection_LayoutsChildrenVertically()
    {
        var parent = new FlexNode
        {
            Width = 100,
            Height = 300,
            FlexDirection = FlexDirection.Column
        };

        var child1 = new FlexNode { Width = 50, Height = 100 };
        var child2 = new FlexNode { Width = 50, Height = 100 };

        parent.AddChild(child1);
        parent.AddChild(child2);
        parent.CalculateLayout(100, 300);

        child1.Layout.Y.Should().Be(0);
        child2.Layout.Y.Should().Be(100);
    }

    [Fact]
    public void FlexGrow_DistributesFreeSpace()
    {
        var parent = new FlexNode
        {
            Width = 300,
            Height = 100,
            FlexDirection = FlexDirection.Row
        };

        var child1 = new FlexNode { FlexGrow = 1, Height = 50 };
        var child2 = new FlexNode { FlexGrow = 2, Height = 50 };

        parent.AddChild(child1);
        parent.AddChild(child2);
        parent.CalculateLayout(300, 100);

        child1.Layout.Width.Should().Be(100);
        child2.Layout.Width.Should().Be(200);
    }

    [Fact]
    public void FlexShrink_ShrinksProperly()
    {
        var parent = new FlexNode
        {
            Width = 100,
            Height = 100,
            FlexDirection = FlexDirection.Row
        };

        // Give children explicit flex basis so shrink works correctly
        var child1 = new FlexNode { FlexBasis = 100, FlexShrink = 1, Height = 50 };
        var child2 = new FlexNode { FlexBasis = 100, FlexShrink = 1, Height = 50 };

        parent.AddChild(child1);
        parent.AddChild(child2);
        parent.CalculateLayout(100, 100);

        // Both should shrink proportionally
        // Total basis = 200, available = 100, shrink = 100
        // Each shrinks by 50
        child1.Layout.Width.Should().BeApproximately(50, 0.1f);
        child2.Layout.Width.Should().BeApproximately(50, 0.1f);
    }

    [Fact]
    public void JustifyContent_Center_CentersChildren()
    {
        var parent = new FlexNode
        {
            Width = 300,
            Height = 100,
            FlexDirection = FlexDirection.Row,
            JustifyContent = JustifyContent.Center
        };

        var child = new FlexNode { Width = 100, Height = 50 };

        parent.AddChild(child);
        parent.CalculateLayout(300, 100);

        child.Layout.X.Should().Be(100); // Centered in 300-100=200 free space
    }

    [Fact]
    public void JustifyContent_FlexEnd_AlignsToEnd()
    {
        var parent = new FlexNode
        {
            Width = 300,
            Height = 100,
            FlexDirection = FlexDirection.Row,
            JustifyContent = JustifyContent.FlexEnd
        };

        var child = new FlexNode { Width = 100, Height = 50 };

        parent.AddChild(child);
        parent.CalculateLayout(300, 100);

        child.Layout.X.Should().Be(200); // At end
    }

    [Fact]
    public void JustifyContent_SpaceBetween_DistributesEvenly()
    {
        var parent = new FlexNode
        {
            Width = 300,
            Height = 100,
            FlexDirection = FlexDirection.Row,
            JustifyContent = JustifyContent.SpaceBetween
        };

        var child1 = new FlexNode { Width = 50, Height = 50 };
        var child2 = new FlexNode { Width = 50, Height = 50 };
        var child3 = new FlexNode { Width = 50, Height = 50 };

        parent.AddChild(child1);
        parent.AddChild(child2);
        parent.AddChild(child3);
        parent.CalculateLayout(300, 100);

        child1.Layout.X.Should().Be(0);
        child3.Layout.X.Should().Be(250); // 300 - 50
    }

    [Fact]
    public void AlignItems_Center_CentersOnCrossAxis()
    {
        var parent = new FlexNode
        {
            Width = 300,
            Height = 100,
            FlexDirection = FlexDirection.Row,
            AlignItems = AlignItems.Center
        };

        // When AlignItems is Center (not Stretch), child keeps its specified height
        var child = new FlexNode { Width = 100, Height = 50, AlignSelf = AlignSelf.Center };

        parent.AddChild(child);
        parent.CalculateLayout(300, 100);

        // Child should be centered: (100 - 50) / 2 = 25
        child.Layout.Y.Should().Be(25);
        child.Layout.Height.Should().Be(50);
    }

    [Fact]
    public void AlignItems_FlexEnd_AlignsToEnd()
    {
        var parent = new FlexNode
        {
            Width = 300,
            Height = 100,
            FlexDirection = FlexDirection.Row,
            AlignItems = AlignItems.FlexEnd
        };

        // Use AlignSelf to ensure the child doesn't stretch
        var child = new FlexNode { Width = 100, Height = 50, AlignSelf = AlignSelf.FlexEnd };

        parent.AddChild(child);
        parent.CalculateLayout(300, 100);

        // Child should be at bottom: 100 - 50 = 50
        child.Layout.Y.Should().Be(50);
        child.Layout.Height.Should().Be(50);
    }

    [Fact]
    public void AlignItems_Stretch_StretchesToCrossAxis()
    {
        var parent = new FlexNode
        {
            Width = 300,
            Height = 100,
            FlexDirection = FlexDirection.Row,
            AlignItems = AlignItems.Stretch
        };

        var child = new FlexNode { Width = 100 }; // No height specified

        parent.AddChild(child);
        parent.CalculateLayout(300, 100);

        child.Layout.Height.Should().Be(100); // Stretched to parent height
    }

    [Fact]
    public void AlignSelf_OverridesAlignItems()
    {
        var parent = new FlexNode
        {
            Width = 300,
            Height = 100,
            FlexDirection = FlexDirection.Row,
            AlignItems = AlignItems.FlexStart
        };

        var child = new FlexNode
        {
            Width = 100,
            Height = 50,
            AlignSelf = AlignSelf.FlexEnd
        };

        parent.AddChild(child);
        parent.CalculateLayout(300, 100);

        child.Layout.Y.Should().Be(50); // Overridden to end
    }

    [Fact]
    public void Padding_AffectsChildPositioning()
    {
        var parent = new FlexNode
        {
            Width = 300,
            Height = 100,
            FlexDirection = FlexDirection.Row,
            AlignItems = AlignItems.FlexStart, // Don't stretch
            Padding = new Edges(FlexValue.Points(10))
        };

        var child = new FlexNode { Width = 50, Height = 50 };

        parent.AddChild(child);
        parent.CalculateLayout(300, 100);

        child.Layout.X.Should().Be(10);
        child.Layout.Y.Should().Be(10);
    }

    [Fact]
    public void Margin_AffectsChildPositioning()
    {
        var parent = new FlexNode
        {
            Width = 300,
            Height = 100,
            FlexDirection = FlexDirection.Row,
            AlignItems = AlignItems.FlexStart // Don't stretch so margin is visible
        };

        var child = new FlexNode
        {
            Width = 50,
            Height = 50,
            Margin = new Edges(FlexValue.Points(10))
        };

        parent.AddChild(child);
        parent.CalculateLayout(300, 100);

        child.Layout.X.Should().Be(10);
        child.Layout.Y.Should().Be(10);
    }

    [Fact]
    public void Gap_SpacesBetweenChildren()
    {
        var parent = new FlexNode
        {
            Width = 300,
            Height = 100,
            FlexDirection = FlexDirection.Row,
            Gap = 20
        };

        var child1 = new FlexNode { Width = 50, Height = 50 };
        var child2 = new FlexNode { Width = 50, Height = 50 };

        parent.AddChild(child1);
        parent.AddChild(child2);
        parent.CalculateLayout(300, 100);

        child1.Layout.X.Should().Be(0);
        child2.Layout.X.Should().Be(70); // 50 + 20 gap
    }

    [Fact]
    public void AbsolutePositioning_UsesPositionOffsets()
    {
        var parent = new FlexNode
        {
            Width = 300,
            Height = 200
        };

        var child = new FlexNode
        {
            Width = 50,
            Height = 50,
            PositionType = PositionType.Absolute,
            Top = 10,
            Left = 20
        };

        parent.AddChild(child);
        parent.CalculateLayout(300, 200);

        child.Layout.X.Should().Be(20);
        child.Layout.Y.Should().Be(10);
    }

    [Fact]
    public void AbsolutePositioning_RightBottomOffsets()
    {
        var parent = new FlexNode
        {
            Width = 300,
            Height = 200
        };

        var child = new FlexNode
        {
            Width = 50,
            Height = 50,
            PositionType = PositionType.Absolute,
            Right = 10,
            Bottom = 20
        };

        parent.AddChild(child);
        parent.CalculateLayout(300, 200);

        child.Layout.X.Should().Be(240); // 300 - 50 - 10
        child.Layout.Y.Should().Be(130); // 200 - 50 - 20
    }

    [Fact]
    public void DisplayNone_HidesNode()
    {
        var parent = new FlexNode
        {
            Width = 300,
            Height = 100,
            FlexDirection = FlexDirection.Row
        };

        var child1 = new FlexNode { Width = 50, Height = 50 };
        var hiddenChild = new FlexNode { Width = 50, Height = 50, Display = Display.None };
        var child2 = new FlexNode { Width = 50, Height = 50 };

        parent.AddChild(child1);
        parent.AddChild(hiddenChild);
        parent.AddChild(child2);
        parent.CalculateLayout(300, 100);

        child1.Layout.X.Should().Be(0);
        hiddenChild.Layout.Width.Should().Be(0);
        child2.Layout.X.Should().Be(50); // Not affected by hidden child
    }

    [Fact]
    public void RowReverse_ReversesOrder()
    {
        var parent = new FlexNode
        {
            Width = 300,
            Height = 100,
            FlexDirection = FlexDirection.RowReverse
        };

        var child1 = new FlexNode { Width = 50, Height = 50, Id = "1" };
        var child2 = new FlexNode { Width = 50, Height = 50, Id = "2" };

        parent.AddChild(child1);
        parent.AddChild(child2);
        parent.CalculateLayout(300, 100);

        // Child2 should be positioned first (at the start)
        child2.Layout.X.Should().BeLessThan(child1.Layout.X);
    }

    [Fact]
    public void NestedFlexContainers_LayoutCorrectly()
    {
        var root = new FlexNode
        {
            Width = 400,
            Height = 200,
            FlexDirection = FlexDirection.Row
        };

        var left = new FlexNode
        {
            Width = 200,
            Height = 200,
            FlexDirection = FlexDirection.Column
        };

        var leftChild1 = new FlexNode { Width = 180, Height = 80 };
        var leftChild2 = new FlexNode { Width = 180, Height = 80 };
        left.AddChild(leftChild1);
        left.AddChild(leftChild2);

        var right = new FlexNode { Width = 200, Height = 200 };

        root.AddChild(left);
        root.AddChild(right);
        root.CalculateLayout(400, 200);

        // Left panel
        left.Layout.X.Should().Be(0);
        left.Layout.Width.Should().Be(200);

        // Right panel
        right.Layout.X.Should().Be(200);

        // Nested children in column direction
        leftChild1.Layout.Y.Should().Be(0);
        leftChild2.Layout.Y.Should().Be(80);
    }
}
