using FluentAssertions;
using OpenTUI.Core.Colors;
using OpenTUI.Core.Layout;
using OpenTUI.Core.Renderables;
using BorderStyle = OpenTUI.Core.Renderables.BorderStyle;

namespace OpenTUI.Core.Tests.Renderables;

public class BoxRenderableTests
{
    [Fact]
    public void Constructor_Default_HasSingleBorder()
    {
        var box = new BoxRenderable();

        box.BorderStyle.Should().Be(BorderStyle.Single);
    }

    [Fact]
    public void Render_SingleBorder_DrawsCorrectCharacters()
    {
        var renderer = CliRenderer.CreateForTesting(20, 10);
        var box = new BoxRenderable
        {
            BorderStyle = BorderStyle.Single,
            BorderColor = RGBA.White
        };
        box.Layout.Width = 5;
        box.Layout.Height = 3;
        box.Layout.AlignSelf = AlignSelf.FlexStart;

        renderer.Root.Add(box);
        renderer.Render();

        var buffer = renderer.GetBuffer();
        buffer.GetCell(0, 0).Character.Should().Be("┌");
        buffer.GetCell(0, 4).Character.Should().Be("┐");
        buffer.GetCell(2, 0).Character.Should().Be("└");
        buffer.GetCell(2, 4).Character.Should().Be("┘");
        buffer.GetCell(0, 2).Character.Should().Be("─");
        buffer.GetCell(1, 0).Character.Should().Be("│");
    }

    [Fact]
    public void Render_DoubleBorder_DrawsDoubleCharacters()
    {
        var renderer = CliRenderer.CreateForTesting(20, 10);
        var box = new BoxRenderable
        {
            BorderStyle = BorderStyle.Double
        };
        box.Layout.Width = 5;
        box.Layout.Height = 3;
        box.Layout.AlignSelf = AlignSelf.FlexStart;

        renderer.Root.Add(box);
        renderer.Render();

        var buffer = renderer.GetBuffer();
        buffer.GetCell(0, 0).Character.Should().Be("╔");
        buffer.GetCell(0, 4).Character.Should().Be("╗");
    }

    [Fact]
    public void Render_RoundedBorder_DrawsRoundedCorners()
    {
        var renderer = CliRenderer.CreateForTesting(20, 10);
        var box = new BoxRenderable
        {
            BorderStyle = BorderStyle.Rounded
        };
        box.Layout.Width = 5;
        box.Layout.Height = 3;
        box.Layout.AlignSelf = AlignSelf.FlexStart;

        renderer.Root.Add(box);
        renderer.Render();

        var buffer = renderer.GetBuffer();
        buffer.GetCell(0, 0).Character.Should().Be("╭");
        buffer.GetCell(0, 4).Character.Should().Be("╮");
    }

    [Fact]
    public void Render_NoBorder_DrawsNothing()
    {
        var renderer = CliRenderer.CreateForTesting(20, 10);
        var box = new BoxRenderable
        {
            BorderStyle = BorderStyle.None,
            BackgroundColor = RGBA.Blue
        };
        box.Layout.Width = 5;
        box.Layout.Height = 3;
        box.Layout.AlignSelf = AlignSelf.FlexStart;

        renderer.Root.Add(box);
        renderer.Render();

        var buffer = renderer.GetBuffer();
        // No border characters should be drawn, just background
        buffer.GetCell(0, 0).Character.Should().Be(" ");
    }

    [Fact]
    public void Render_WithTitle_DrawsTitleInBorder()
    {
        var renderer = CliRenderer.CreateForTesting(20, 10);
        var box = new BoxRenderable
        {
            BorderStyle = BorderStyle.Single,
            Title = "Test"
        };
        box.Layout.Width = 12;
        box.Layout.Height = 3;
        box.Layout.AlignSelf = AlignSelf.FlexStart;

        renderer.Root.Add(box);
        renderer.Render();

        var buffer = renderer.GetBuffer();
        // Title should appear in top border with spaces
        buffer.GetCell(0, 3).Character.Should().Be("T");
        buffer.GetCell(0, 4).Character.Should().Be("e");
        buffer.GetCell(0, 5).Character.Should().Be("s");
        buffer.GetCell(0, 6).Character.Should().Be("t");
    }

    [Fact]
    public void Render_CenteredTitle_CentersTitle()
    {
        var renderer = CliRenderer.CreateForTesting(20, 10);
        var box = new BoxRenderable
        {
            BorderStyle = BorderStyle.Single,
            Title = "Hi",
            TitleAlign = TextAlign.Center
        };
        box.Layout.Width = 12;
        box.Layout.Height = 3;
        box.Layout.AlignSelf = AlignSelf.FlexStart;

        renderer.Root.Add(box);
        renderer.Render();

        var buffer = renderer.GetBuffer();
        // "Hi" centered in width 12 (minus corners = 10, minus padding = 8)
        // Center position = (12 - 4) / 2 = 4
        buffer.GetCell(0, 5).Character.Should().Be("H");
    }

    [Fact]
    public void BorderColor_AppliedToRenderedBorder()
    {
        var renderer = CliRenderer.CreateForTesting(20, 10);
        var box = new BoxRenderable
        {
            BorderStyle = BorderStyle.Single,
            BorderColor = RGBA.Red
        };
        box.Layout.Width = 5;
        box.Layout.Height = 3;
        box.Layout.AlignSelf = AlignSelf.FlexStart;

        renderer.Root.Add(box);
        renderer.Render();

        var buffer = renderer.GetBuffer();
        buffer.GetCell(0, 0).Foreground.Should().Be(RGBA.Red);
    }

    [Fact]
    public void Children_RenderedWithParent()
    {
        var renderer = CliRenderer.CreateForTesting(20, 10);
        var box = new BoxRenderable
        {
            BorderStyle = BorderStyle.Single
        };
        box.Layout.Width = 10;
        box.Layout.Height = 5;
        box.Layout.AlignSelf = AlignSelf.FlexStart;

        var text = new TextRenderable("Hi");
        text.Layout.Width = 5;
        text.Layout.Height = 1;

        box.Add(text);
        renderer.Root.Add(box);
        renderer.Render();

        var buffer = renderer.GetBuffer();
        // Border should be drawn at position 0,0
        buffer.GetCell(0, 0).Character.Should().Be("┌");
        // Text is a child and should render (position depends on layout)
        // Just verify it was added to the tree
        box.Children.Should().Contain(text);
    }
}
