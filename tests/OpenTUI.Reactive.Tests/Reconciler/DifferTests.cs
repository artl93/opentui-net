using FluentAssertions;
using OpenTUI.Core.Renderables;
using OpenTUI.Reactive.Reconciler;

namespace OpenTUI.Reactive.Tests.Reconciler;

public class DifferTests
{
    [Fact]
    public void Diff_BothNull_ReturnsEmpty()
    {
        var patches = Differ.Diff(null, null, Array.Empty<int>());
        patches.Should().BeEmpty();
    }

    [Fact]
    public void Diff_OldNull_ReturnsCreate()
    {
        var node = VNodes.Element<TextRenderable>();
        var patches = Differ.Diff(null, node, Array.Empty<int>());

        patches.Should().HaveCount(1);
        patches[0].Should().BeOfType<CreatePatch>();
    }

    [Fact]
    public void Diff_NewNull_ReturnsRemove()
    {
        var node = VNodes.Element<TextRenderable>();
        var patches = Differ.Diff(node, null, Array.Empty<int>());

        patches.Should().HaveCount(1);
        patches[0].Should().BeOfType<RemovePatch>();
    }

    [Fact]
    public void Diff_SameType_ReturnsUpdate()
    {
        var oldNode = VNodes.Element<TextRenderable>();
        var newNode = VNodes.Element<TextRenderable>();

        var patches = Differ.Diff(oldNode, newNode, Array.Empty<int>());

        patches.Should().Contain(p => p is UpdatePatch);
    }

    [Fact]
    public void Diff_DifferentType_ReturnsReplace()
    {
        var oldNode = VNodes.Element<TextRenderable>();
        var newNode = VNodes.Element<BoxRenderable>();

        var patches = Differ.Diff(oldNode, newNode, Array.Empty<int>());

        patches.Should().HaveCount(1);
        patches[0].Should().BeOfType<ReplacePatch>();
    }

    [Fact]
    public void Diff_DifferentKey_ReturnsReplace()
    {
        var oldNode = new ElementNode(typeof(TextRenderable), () => new TextRenderable(), null) { Key = "a" };
        var newNode = new ElementNode(typeof(TextRenderable), () => new TextRenderable(), null) { Key = "b" };

        var patches = Differ.Diff(oldNode, newNode, Array.Empty<int>());

        patches.Should().HaveCount(1);
        patches[0].Should().BeOfType<ReplacePatch>();
    }

    [Fact]
    public void Diff_Fragment_DiffsChildren()
    {
        var oldNode = VNodes.Fragment(
            VNodes.Element<TextRenderable>(),
            VNodes.Element<TextRenderable>()
        );
        var newNode = VNodes.Fragment(
            VNodes.Element<TextRenderable>(),
            VNodes.Element<TextRenderable>(),
            VNodes.Element<TextRenderable>()
        );

        var patches = Differ.Diff(oldNode, newNode, Array.Empty<int>());

        // Should have update for parent + updates for existing children + create for new child
        patches.Should().Contain(p => p is CreatePatch);
    }

    [Fact]
    public void Diff_Fragment_RemovesChildren()
    {
        var oldNode = VNodes.Fragment(
            VNodes.Element<TextRenderable>(),
            VNodes.Element<TextRenderable>(),
            VNodes.Element<TextRenderable>()
        );
        var newNode = VNodes.Fragment(
            VNodes.Element<TextRenderable>()
        );

        var patches = Differ.Diff(oldNode, newNode, Array.Empty<int>());

        patches.Should().Contain(p => p is RemovePatch);
    }

    [Fact]
    public void Diff_Path_IsSetCorrectly()
    {
        var node = VNodes.Element<TextRenderable>();
        var path = new int[] { 1, 2, 3 };

        var patches = Differ.Diff(null, node, path);

        patches[0].Path.Should().Equal(1, 2, 3);
    }
}
