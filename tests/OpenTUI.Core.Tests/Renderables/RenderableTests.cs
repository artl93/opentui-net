using FluentAssertions;
using OpenTUI.Core.Colors;
using OpenTUI.Core.Layout;
using OpenTUI.Core.Rendering;
using OpenTUI.Core.Renderables;

namespace OpenTUI.Core.Tests.Renderables;

public class RenderableTests
{
    [Fact]
    public void Constructor_CreatesWithDefaultLayout()
    {
        var renderable = new TestRenderable();

        renderable.Layout.Should().NotBeNull();
        renderable.Visible.Should().BeTrue();
        renderable.Focusable.Should().BeFalse();
        renderable.IsFocused.Should().BeFalse();
    }

    [Fact]
    public void Id_SyncsWithLayout()
    {
        var renderable = new TestRenderable { Id = "test-id" };

        renderable.Layout.Id.Should().Be("test-id");
    }

    [Fact]
    public void Visible_SyncsWithLayoutDisplay()
    {
        var renderable = new TestRenderable { Visible = false };

        renderable.Layout.Display.Should().Be(Display.None);

        renderable.Visible = true;
        renderable.Layout.Display.Should().Be(Display.Flex);
    }

    [Fact]
    public void Add_AddsChildAndSyncsLayout()
    {
        var parent = new TestRenderable();
        var child = new TestRenderable();

        parent.Add(child);

        parent.Children.Should().Contain(child);
        child.Parent.Should().Be(parent);
        parent.Layout.Children.Should().Contain(child.Layout);
    }

    [Fact]
    public void Insert_InsertsAtCorrectPosition()
    {
        var parent = new TestRenderable();
        var child1 = new TestRenderable { Id = "1" };
        var child2 = new TestRenderable { Id = "2" };
        var child3 = new TestRenderable { Id = "3" };

        parent.Add(child1);
        parent.Add(child3);
        parent.Insert(1, child2);

        parent.Children[0].Id.Should().Be("1");
        parent.Children[1].Id.Should().Be("2");
        parent.Children[2].Id.Should().Be("3");
    }

    [Fact]
    public void Remove_RemovesChildAndSyncsLayout()
    {
        var parent = new TestRenderable();
        var child = new TestRenderable();

        parent.Add(child);
        parent.Remove(child);

        parent.Children.Should().NotContain(child);
        child.Parent.Should().BeNull();
        parent.Layout.Children.Should().NotContain(child.Layout);
    }

    [Fact]
    public void Clear_RemovesAllChildren()
    {
        var parent = new TestRenderable();
        parent.Add(new TestRenderable());
        parent.Add(new TestRenderable());

        parent.Clear();

        parent.Children.Should().BeEmpty();
        parent.Layout.Children.Should().BeEmpty();
    }

    [Fact]
    public void Focus_SetsFocusWhenFocusable()
    {
        var renderable = new TestRenderable { Focusable = true };

        renderable.Focus();

        renderable.IsFocused.Should().BeTrue();
        renderable.OnFocusCalled.Should().BeTrue();
    }

    [Fact]
    public void Focus_DoesNothingWhenNotFocusable()
    {
        var renderable = new TestRenderable { Focusable = false };

        renderable.Focus();

        renderable.IsFocused.Should().BeFalse();
    }

    [Fact]
    public void Blur_RemovesFocus()
    {
        var renderable = new TestRenderable { Focusable = true };
        renderable.Focus();

        renderable.Blur();

        renderable.IsFocused.Should().BeFalse();
        renderable.OnBlurCalled.Should().BeTrue();
    }

    [Fact]
    public void FindById_FindsSelf()
    {
        var renderable = new TestRenderable { Id = "target" };

        renderable.FindById("target").Should().Be(renderable);
    }

    [Fact]
    public void FindById_FindsDescendant()
    {
        var root = new TestRenderable { Id = "root" };
        var child = new TestRenderable { Id = "child" };
        var grandchild = new TestRenderable { Id = "grandchild" };

        root.Add(child);
        child.Add(grandchild);

        root.FindById("grandchild").Should().Be(grandchild);
    }

    [Fact]
    public void FindById_ReturnsNullWhenNotFound()
    {
        var renderable = new TestRenderable { Id = "root" };

        renderable.FindById("nonexistent").Should().BeNull();
    }

    [Fact]
    public void GetFocusableDescendants_ReturnsFocusableElements()
    {
        var root = new TestRenderable();
        var focusable1 = new TestRenderable { Focusable = true, Id = "f1" };
        var notFocusable = new TestRenderable { Focusable = false };
        var focusable2 = new TestRenderable { Focusable = true, Id = "f2" };

        root.Add(focusable1);
        root.Add(notFocusable);
        notFocusable.Add(focusable2);

        var focusable = root.GetFocusableDescendants().ToList();

        focusable.Should().HaveCount(2);
        focusable[0].Id.Should().Be("f1");
        focusable[1].Id.Should().Be("f2");
    }

    [Fact]
    public void Render_DrawsBackground()
    {
        var renderer = CliRenderer.CreateForTesting(20, 10);
        // Clear the root's background so we can see the child's background
        renderer.Root.BackgroundColor = null;

        var renderable = new TestRenderable
        {
            BackgroundColor = RGBA.Red
        };
        renderable.Layout.Width = 10;
        renderable.Layout.Height = 5;
        // Align to start so it renders at position 0
        renderable.Layout.AlignSelf = AlignSelf.FlexStart;

        renderer.Root.Add(renderable);
        renderer.Render();

        var buffer = renderer.GetBuffer();
        // GetCell takes (row, col) - check row 0, col 0
        buffer.GetCell(0, 0).Background.Should().Be(RGBA.Red);
    }

    [Fact]
    public void Render_CallsRenderSelf()
    {
        var renderer = CliRenderer.CreateForTesting(20, 10);
        var renderable = new TestRenderable();
        renderable.Layout.Width = 10;
        renderable.Layout.Height = 5;

        renderer.Root.Add(renderable);
        renderer.Render();

        renderable.RenderSelfCalled.Should().BeTrue();
    }

    [Fact]
    public void Render_RendersChildren()
    {
        var renderer = CliRenderer.CreateForTesting(20, 10);
        var parent = new TestRenderable();
        var child = new TestRenderable();

        parent.Layout.Width = 20;
        parent.Layout.Height = 10;
        child.Layout.Width = 5;
        child.Layout.Height = 3;

        parent.Add(child);
        renderer.Root.Add(parent);
        renderer.Render();

        child.RenderSelfCalled.Should().BeTrue();
    }

    [Fact]
    public void Render_SkipsInvisibleElements()
    {
        var renderer = CliRenderer.CreateForTesting(20, 10);
        var renderable = new TestRenderable { Visible = false };
        renderable.Layout.Width = 10;
        renderable.Layout.Height = 5;

        renderer.Root.Add(renderable);
        renderer.Render();

        renderable.RenderSelfCalled.Should().BeFalse();
    }
}

/// <summary>
/// Test implementation of Renderable for testing.
/// </summary>
public class TestRenderable : Renderable
{
    public bool RenderSelfCalled { get; private set; }
    public bool OnFocusCalled { get; private set; }
    public bool OnBlurCalled { get; private set; }

    protected override void RenderSelf(FrameBuffer buffer, int x, int y, int width, int height)
    {
        RenderSelfCalled = true;
        base.RenderSelf(buffer, x, y, width, height);
    }

    public override void OnFocus()
    {
        OnFocusCalled = true;
        base.OnFocus();
    }

    public override void OnBlur()
    {
        OnBlurCalled = true;
        base.OnBlur();
    }
}
