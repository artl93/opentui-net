using FluentAssertions;
using OpenTUI.Core.Layout;

namespace OpenTUI.Layout.Tests;

public class FlexNodeTests
{
    [Fact]
    public void Constructor_HasDefaultValues()
    {
        var node = new FlexNode();

        node.FlexDirection.Should().Be(FlexDirection.Row);
        node.JustifyContent.Should().Be(JustifyContent.FlexStart);
        node.AlignItems.Should().Be(AlignItems.Stretch);
        node.FlexGrow.Should().Be(0);
        node.FlexShrink.Should().Be(1);
        node.FlexBasis.Should().Be(FlexValue.Auto);
        node.Width.Should().Be(FlexValue.Auto);
        node.Height.Should().Be(FlexValue.Auto);
    }

    [Fact]
    public void AddChild_AddsToChildren()
    {
        var parent = new FlexNode();
        var child = new FlexNode();

        parent.AddChild(child);

        parent.Children.Should().Contain(child);
        child.Parent.Should().Be(parent);
    }

    [Fact]
    public void AddChild_RemovesFromPreviousParent()
    {
        var parent1 = new FlexNode();
        var parent2 = new FlexNode();
        var child = new FlexNode();

        parent1.AddChild(child);
        parent2.AddChild(child);

        parent1.Children.Should().NotContain(child);
        parent2.Children.Should().Contain(child);
        child.Parent.Should().Be(parent2);
    }

    [Fact]
    public void InsertChild_InsertsAtIndex()
    {
        var parent = new FlexNode();
        var child1 = new FlexNode { Id = "1" };
        var child2 = new FlexNode { Id = "2" };
        var child3 = new FlexNode { Id = "3" };

        parent.AddChild(child1);
        parent.AddChild(child3);
        parent.InsertChild(1, child2);

        parent.Children[0].Id.Should().Be("1");
        parent.Children[1].Id.Should().Be("2");
        parent.Children[2].Id.Should().Be("3");
    }

    [Fact]
    public void RemoveChild_RemovesFromChildren()
    {
        var parent = new FlexNode();
        var child = new FlexNode();

        parent.AddChild(child);
        parent.RemoveChild(child);

        parent.Children.Should().NotContain(child);
        child.Parent.Should().BeNull();
    }

    [Fact]
    public void ClearChildren_RemovesAllChildren()
    {
        var parent = new FlexNode();
        parent.AddChild(new FlexNode());
        parent.AddChild(new FlexNode());

        parent.ClearChildren();

        parent.Children.Should().BeEmpty();
    }

    [Fact]
    public void MarkDirty_MarksNodeAndAncestors()
    {
        var root = new FlexNode();
        var child = new FlexNode();
        var grandchild = new FlexNode();

        root.AddChild(child);
        child.AddChild(grandchild);

        // Calculate layout to clear dirty flags
        root.CalculateLayout(100, 100);

        // Mark grandchild dirty
        grandchild.MarkDirty();

        grandchild.IsDirty.Should().BeTrue();
        child.IsDirty.Should().BeTrue();
        root.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void CalculateLayout_ClearsDirtyFlags()
    {
        var root = new FlexNode();
        root.AddChild(new FlexNode());

        root.CalculateLayout(100, 100);

        root.IsDirty.Should().BeFalse();
    }

    [Fact]
    public void IsRow_ReturnsTrueForRowDirections()
    {
        var node = new FlexNode { FlexDirection = FlexDirection.Row };
        node.IsRow.Should().BeTrue();

        node.FlexDirection = FlexDirection.RowReverse;
        node.IsRow.Should().BeTrue();

        node.FlexDirection = FlexDirection.Column;
        node.IsRow.Should().BeFalse();
    }

    [Fact]
    public void IsReverse_ReturnsTrueForReverseDirections()
    {
        var node = new FlexNode { FlexDirection = FlexDirection.RowReverse };
        node.IsReverse.Should().BeTrue();

        node.FlexDirection = FlexDirection.ColumnReverse;
        node.IsReverse.Should().BeTrue();

        node.FlexDirection = FlexDirection.Row;
        node.IsReverse.Should().BeFalse();
    }

    [Fact]
    public void GetMainGap_UsesGapWhenSet()
    {
        var node = new FlexNode { Gap = 10, RowGap = 5, ColumnGap = 15 };

        node.GetMainGap().Should().Be(10);
    }

    [Fact]
    public void GetMainGap_UsesColumnGapForRow()
    {
        var node = new FlexNode { FlexDirection = FlexDirection.Row, ColumnGap = 15 };

        node.GetMainGap().Should().Be(15);
    }

    [Fact]
    public void GetMainGap_UsesRowGapForColumn()
    {
        var node = new FlexNode { FlexDirection = FlexDirection.Column, RowGap = 10 };

        node.GetMainGap().Should().Be(10);
    }
}
