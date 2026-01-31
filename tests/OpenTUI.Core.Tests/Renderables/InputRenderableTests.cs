using FluentAssertions;
using OpenTUI.Core.Colors;
using OpenTUI.Core.Layout;
using OpenTUI.Core.Renderables;

namespace OpenTUI.Core.Tests.Renderables;

public class InputRenderableTests
{
    [Fact]
    public void Constructor_Default_IsFocusable()
    {
        var input = new InputRenderable();
        
        input.Focusable.Should().BeTrue();
        input.Value.Should().BeEmpty();
        input.CursorPosition.Should().Be(0);
    }

    [Fact]
    public void Value_Change_RaisesEvent()
    {
        var input = new InputRenderable();
        string? changedValue = null;
        input.ValueChanged += (_, v) => changedValue = v;
        
        input.Value = "Hello";
        
        changedValue.Should().Be("Hello");
    }

    [Fact]
    public void Insert_AddsTextAtCursor()
    {
        var input = new InputRenderable { Value = "Hello" };
        input.CursorPosition = 5;
        
        input.Insert(" World");
        
        input.Value.Should().Be("Hello World");
        input.CursorPosition.Should().Be(11);
    }

    [Fact]
    public void Insert_InMiddle_InsertsCorrectly()
    {
        var input = new InputRenderable { Value = "Helo" };
        input.CursorPosition = 3;
        
        input.Insert("l");
        
        input.Value.Should().Be("Hello");
        input.CursorPosition.Should().Be(4);
    }

    [Fact]
    public void Backspace_RemovesCharacterBeforeCursor()
    {
        var input = new InputRenderable { Value = "Hello" };
        input.CursorPosition = 5;
        
        input.Backspace();
        
        input.Value.Should().Be("Hell");
        input.CursorPosition.Should().Be(4);
    }

    [Fact]
    public void Backspace_AtStart_DoesNothing()
    {
        var input = new InputRenderable { Value = "Hello" };
        input.CursorPosition = 0;
        
        input.Backspace();
        
        input.Value.Should().Be("Hello");
        input.CursorPosition.Should().Be(0);
    }

    [Fact]
    public void Delete_RemovesCharacterAtCursor()
    {
        var input = new InputRenderable { Value = "Hello" };
        input.CursorPosition = 0;
        
        input.Delete();
        
        input.Value.Should().Be("ello");
        input.CursorPosition.Should().Be(0);
    }

    [Fact]
    public void Delete_AtEnd_DoesNothing()
    {
        var input = new InputRenderable { Value = "Hello" };
        input.CursorPosition = 5;
        
        input.Delete();
        
        input.Value.Should().Be("Hello");
    }

    [Fact]
    public void MoveCursorLeft_MovesCursor()
    {
        var input = new InputRenderable { Value = "Hello" };
        input.CursorPosition = 3;
        
        input.MoveCursorLeft();
        
        input.CursorPosition.Should().Be(2);
    }

    [Fact]
    public void MoveCursorRight_MovesCursor()
    {
        var input = new InputRenderable { Value = "Hello" };
        input.CursorPosition = 3;
        
        input.MoveCursorRight();
        
        input.CursorPosition.Should().Be(4);
    }

    [Fact]
    public void MoveCursorHome_MovesToStart()
    {
        var input = new InputRenderable { Value = "Hello" };
        input.CursorPosition = 3;
        
        input.MoveCursorHome();
        
        input.CursorPosition.Should().Be(0);
    }

    [Fact]
    public void MoveCursorEnd_MovesToEnd()
    {
        var input = new InputRenderable { Value = "Hello" };
        input.CursorPosition = 0;
        
        input.MoveCursorEnd();
        
        input.CursorPosition.Should().Be(5);
    }

    [Fact]
    public void Clear_RemovesAllText()
    {
        var input = new InputRenderable { Value = "Hello" };
        input.CursorPosition = 3;
        
        input.Clear();
        
        input.Value.Should().BeEmpty();
        input.CursorPosition.Should().Be(0);
    }

    [Fact]
    public void Submit_RaisesEvent()
    {
        var input = new InputRenderable { Value = "Hello" };
        string? submitted = null;
        input.Submitted += (_, v) => submitted = v;
        
        input.Submit();
        
        submitted.Should().Be("Hello");
    }

    [Fact]
    public void Password_MasksInput()
    {
        var renderer = CliRenderer.CreateForTesting(20, 5);
        var input = new InputRenderable
        {
            Value = "secret",
            Password = true
        };
        input.Layout.Width = 10;
        input.Layout.Height = 1;
        input.Layout.AlignSelf = AlignSelf.FlexStart;
        
        renderer.Root.Add(input);
        renderer.Render();
        
        var buffer = renderer.GetBuffer();
        buffer.GetCell(0, 0).Character.Should().Be("*");
        buffer.GetCell(0, 1).Character.Should().Be("*");
    }

    [Fact]
    public void Render_ShowsPlaceholderWhenEmpty()
    {
        var renderer = CliRenderer.CreateForTesting(20, 5);
        var input = new InputRenderable
        {
            Placeholder = "Enter text..."
        };
        input.Layout.Width = 15;
        input.Layout.Height = 1;
        input.Layout.AlignSelf = AlignSelf.FlexStart;
        
        renderer.Root.Add(input);
        renderer.Render();
        
        var buffer = renderer.GetBuffer();
        buffer.GetCell(0, 0).Character.Should().Be("E");
    }

    [Fact]
    public void CursorPosition_ClampedToValueLength()
    {
        var input = new InputRenderable { Value = "Hi" };
        
        input.CursorPosition = 100;
        
        input.CursorPosition.Should().Be(2);
    }

    [Fact]
    public void Value_Change_ClampsCursor()
    {
        var input = new InputRenderable { Value = "Hello" };
        input.CursorPosition = 5;
        
        input.Value = "Hi";
        
        input.CursorPosition.Should().Be(2);
    }
}
