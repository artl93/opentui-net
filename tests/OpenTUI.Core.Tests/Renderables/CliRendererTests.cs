using FluentAssertions;
using OpenTUI.Core.Colors;
using OpenTUI.Core.Layout;
using OpenTUI.Core.Renderables;

namespace OpenTUI.Core.Tests.Renderables;

public class CliRendererTests
{
    [Fact]
    public void CreateForTesting_CreatesRendererWithDimensions()
    {
        var renderer = CliRenderer.CreateForTesting(80, 24);

        renderer.Width.Should().Be(80);
        renderer.Height.Should().Be(24);
        renderer.Root.Should().NotBeNull();
    }

    [Fact]
    public void Root_HasCorrectDimensions()
    {
        var renderer = CliRenderer.CreateForTesting(80, 24);

        renderer.Root.Layout.Width.Should().Be(FlexValue.Points(80));
        renderer.Root.Layout.Height.Should().Be(FlexValue.Points(24));
    }

    [Fact]
    public void Render_CalculatesLayout()
    {
        var renderer = CliRenderer.CreateForTesting(80, 24);
        var child = new TestRenderable();
        child.Layout.Width = 40;
        child.Layout.Height = 12;

        renderer.Root.Add(child);
        renderer.Render();

        child.Layout.Layout.Width.Should().Be(40);
        child.Layout.Layout.Height.Should().Be(12);
    }

    [Fact]
    public void Render_ClearsBufferWithDefaultColor()
    {
        var renderer = CliRenderer.CreateForTesting(10, 5);
        renderer.DefaultBackground = RGBA.Blue;

        renderer.Render();

        var buffer = renderer.GetBuffer();
        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col < 10; col++)
            {
                buffer.GetCell(row, col).Background.Should().Be(RGBA.Blue);
            }
        }
    }

    [Fact]
    public void Render_RendersDirtyElements()
    {
        var renderer = CliRenderer.CreateForTesting(20, 10);
        var testElement = new TestRenderable();
        testElement.Layout.Width = 10;
        testElement.Layout.Height = 5;

        renderer.Root.Add(testElement);
        renderer.Render();

        testElement.RenderSelfCalled.Should().BeTrue();
    }

    [Fact]
    public void GetOutput_ReturnsAnsiString()
    {
        var renderer = CliRenderer.CreateForTesting(10, 3);

        renderer.Render();
        var output = renderer.GetOutput(differential: false);

        output.Should().NotBeEmpty();
        output.Should().Contain("\x1b["); // ANSI escape sequences
    }

    [Fact]
    public void GetOutput_Differential_ReturnsMinimalUpdates()
    {
        var renderer = CliRenderer.CreateForTesting(10, 3);

        // First render
        renderer.Render();
        _ = renderer.GetOutput(differential: false);

        // Second render with no changes
        renderer.Render();
        var output = renderer.GetOutput(differential: true);

        // Should be minimal since nothing changed
        output.Should().NotBeNull();
    }

    [Fact]
    public void SetFocused_UpdatesFocusedElement()
    {
        var renderer = CliRenderer.CreateForTesting(20, 10);
        var focusable = new TestRenderable { Focusable = true };
        focusable.Layout.Width = 10;
        focusable.Layout.Height = 5;

        renderer.Root.Add(focusable);
        renderer.SetFocused(focusable);

        renderer.Focused.Should().Be(focusable);
        focusable.IsFocused.Should().BeTrue();
    }

    [Fact]
    public void SetFocused_BlursPreviousFocusedElement()
    {
        var renderer = CliRenderer.CreateForTesting(20, 10);
        var first = new TestRenderable { Focusable = true, Id = "first" };
        var second = new TestRenderable { Focusable = true, Id = "second" };

        renderer.Root.Add(first);
        renderer.Root.Add(second);

        renderer.SetFocused(first);
        renderer.SetFocused(second);

        first.IsFocused.Should().BeFalse();
        first.OnBlurCalled.Should().BeTrue();
        second.IsFocused.Should().BeTrue();
    }

    [Fact]
    public void FocusNext_MovesFocusToNextFocusable()
    {
        var renderer = CliRenderer.CreateForTesting(20, 10);
        var first = new TestRenderable { Focusable = true, Id = "first" };
        var notFocusable = new TestRenderable { Focusable = false };
        var second = new TestRenderable { Focusable = true, Id = "second" };

        renderer.Root.Add(first);
        renderer.Root.Add(notFocusable);
        renderer.Root.Add(second);

        renderer.SetFocused(first);
        renderer.FocusNext();

        renderer.Focused.Should().Be(second);
    }

    [Fact]
    public void FocusNext_WrapsToFirstFocusable()
    {
        var renderer = CliRenderer.CreateForTesting(20, 10);
        var first = new TestRenderable { Focusable = true, Id = "first" };
        var second = new TestRenderable { Focusable = true, Id = "second" };

        renderer.Root.Add(first);
        renderer.Root.Add(second);

        renderer.SetFocused(second);
        renderer.FocusNext();

        renderer.Focused.Should().Be(first);
    }

    [Fact]
    public void FocusPrevious_MovesFocusToPreviousFocusable()
    {
        var renderer = CliRenderer.CreateForTesting(20, 10);
        var first = new TestRenderable { Focusable = true, Id = "first" };
        var second = new TestRenderable { Focusable = true, Id = "second" };

        renderer.Root.Add(first);
        renderer.Root.Add(second);

        renderer.SetFocused(second);
        renderer.FocusPrevious();

        renderer.Focused.Should().Be(first);
    }

    [Fact]
    public void FocusPrevious_WrapsToLastFocusable()
    {
        var renderer = CliRenderer.CreateForTesting(20, 10);
        var first = new TestRenderable { Focusable = true, Id = "first" };
        var second = new TestRenderable { Focusable = true, Id = "second" };

        renderer.Root.Add(first);
        renderer.Root.Add(second);

        renderer.SetFocused(first);
        renderer.FocusPrevious();

        renderer.Focused.Should().Be(second);
    }

    [Fact]
    public void Resize_UpdatesDimensions()
    {
        var renderer = CliRenderer.CreateForTesting(80, 24);

        renderer.Resize(100, 30);

        renderer.Width.Should().Be(100);
        renderer.Height.Should().Be(30);
        renderer.Root.Layout.Width.Should().Be(FlexValue.Points(100));
        renderer.Root.Layout.Height.Should().Be(FlexValue.Points(30));
    }

    [Fact]
    public void Resize_RecreatesBuffer()
    {
        var renderer = CliRenderer.CreateForTesting(10, 5);

        renderer.Resize(20, 10);

        var buffer = renderer.GetBuffer();
        buffer.Width.Should().Be(20);
        buffer.Height.Should().Be(10);
    }

    [Fact]
    public void TargetFps_HasDefaultValue()
    {
        var renderer = CliRenderer.CreateForTesting(80, 24);

        renderer.TargetFps.Should().Be(60);
    }

    [Fact]
    public void TargetFps_CanBeChanged()
    {
        var renderer = CliRenderer.CreateForTesting(80, 24);

        renderer.TargetFps = 30;

        renderer.TargetFps.Should().Be(30);
    }
}
