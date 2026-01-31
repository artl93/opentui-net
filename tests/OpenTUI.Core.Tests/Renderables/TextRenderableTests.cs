using FluentAssertions;
using OpenTUI.Core.Colors;
using OpenTUI.Core.Layout;
using OpenTUI.Core.Rendering;
using OpenTUI.Core.Renderables;

namespace OpenTUI.Core.Tests.Renderables;

public class TextRenderableTests
{
    [Fact]
    public void Constructor_Default_CreatesEmptyText()
    {
        var text = new TextRenderable();
        
        text.Text.Should().BeEmpty();
        text.Wrap.Should().Be(TextWrap.None);
        text.Align.Should().Be(TextAlign.Left);
    }

    [Fact]
    public void Constructor_WithText_SetsText()
    {
        var text = new TextRenderable("Hello");
        
        text.Text.Should().Be("Hello");
    }

    [Fact]
    public void Text_Change_MarksDirty()
    {
        var text = new TextRenderable("Hello");
        text.Layout.CalculateLayout(100, 100);
        
        text.Text = "World";
        
        text.Layout.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void Render_DrawsTextAtPosition()
    {
        var renderer = CliRenderer.CreateForTesting(20, 5);
        var text = new TextRenderable("Hello")
        {
            ForegroundColor = RGBA.White
        };
        text.Layout.Width = 10;
        text.Layout.Height = 1;
        text.Layout.AlignSelf = AlignSelf.FlexStart;
        
        renderer.Root.Add(text);
        renderer.Render();
        
        var buffer = renderer.GetBuffer();
        buffer.GetCell(0, 0).Character.Should().Be("H");
        buffer.GetCell(0, 1).Character.Should().Be("e");
        buffer.GetCell(0, 4).Character.Should().Be("o");
    }

    [Fact]
    public void Render_CenterAlignment_CentersText()
    {
        var renderer = CliRenderer.CreateForTesting(20, 5);
        var text = new TextRenderable("Hi")
        {
            Align = TextAlign.Center
        };
        text.Layout.Width = 10;
        text.Layout.Height = 1;
        text.Layout.AlignSelf = AlignSelf.FlexStart;
        
        renderer.Root.Add(text);
        renderer.Render();
        
        var buffer = renderer.GetBuffer();
        // "Hi" (2 chars) centered in 10 chars = position 4
        buffer.GetCell(0, 4).Character.Should().Be("H");
    }

    [Fact]
    public void Render_RightAlignment_AlignsRight()
    {
        var renderer = CliRenderer.CreateForTesting(20, 5);
        var text = new TextRenderable("Hi")
        {
            Align = TextAlign.Right
        };
        text.Layout.Width = 10;
        text.Layout.Height = 1;
        text.Layout.AlignSelf = AlignSelf.FlexStart;
        
        renderer.Root.Add(text);
        renderer.Render();
        
        var buffer = renderer.GetBuffer();
        // "Hi" (2 chars) right-aligned in 10 chars = position 8
        buffer.GetCell(0, 8).Character.Should().Be("H");
    }

    [Fact]
    public void Wrap_None_TruncatesText()
    {
        var renderer = CliRenderer.CreateForTesting(20, 5);
        var text = new TextRenderable("Hello World")
        {
            Wrap = TextWrap.None
        };
        text.Layout.Width = 5;
        text.Layout.Height = 2;
        text.Layout.AlignSelf = AlignSelf.FlexStart;
        
        renderer.Root.Add(text);
        renderer.Render();
        
        var buffer = renderer.GetBuffer();
        buffer.GetCell(0, 0).Character.Should().Be("H");
        buffer.GetCell(0, 4).Character.Should().Be("o");
        // Second row should be empty (no wrap)
        buffer.GetCell(1, 0).Character.Should().Be(" ");
    }

    [Fact]
    public void Wrap_Word_WrapsAtWordBoundary()
    {
        var renderer = CliRenderer.CreateForTesting(20, 5);
        var text = new TextRenderable("Hello World")
        {
            Wrap = TextWrap.Word
        };
        text.Layout.Width = 7;
        text.Layout.Height = 3;
        text.Layout.AlignSelf = AlignSelf.FlexStart;
        
        renderer.Root.Add(text);
        renderer.Render();
        
        var buffer = renderer.GetBuffer();
        // First line: "Hello"
        buffer.GetCell(0, 0).Character.Should().Be("H");
        // Second line: "World"
        buffer.GetCell(1, 0).Character.Should().Be("W");
    }

    [Fact]
    public void Wrap_Character_WrapsAtCharacterBoundary()
    {
        var renderer = CliRenderer.CreateForTesting(20, 5);
        var text = new TextRenderable("HelloWorld")
        {
            Wrap = TextWrap.Character
        };
        text.Layout.Width = 5;
        text.Layout.Height = 3;
        text.Layout.AlignSelf = AlignSelf.FlexStart;
        
        renderer.Root.Add(text);
        renderer.Render();
        
        var buffer = renderer.GetBuffer();
        // First line: "Hello"
        buffer.GetCell(0, 4).Character.Should().Be("o");
        // Second line: "World"
        buffer.GetCell(1, 0).Character.Should().Be("W");
    }

    [Fact]
    public void Attributes_AppliedToRenderedCells()
    {
        var renderer = CliRenderer.CreateForTesting(20, 5);
        var text = new TextRenderable("Hi")
        {
            Attributes = TextAttributes.Bold
        };
        text.Layout.Width = 5;
        text.Layout.Height = 1;
        text.Layout.AlignSelf = AlignSelf.FlexStart;
        
        renderer.Root.Add(text);
        renderer.Render();
        
        var buffer = renderer.GetBuffer();
        buffer.GetCell(0, 0).Attributes.Should().HaveFlag(TextAttributes.Bold);
    }
}
