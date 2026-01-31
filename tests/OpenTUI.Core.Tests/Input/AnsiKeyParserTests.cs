using FluentAssertions;
using OpenTUI.Core.Input;

namespace OpenTUI.Core.Tests.Input;

public class AnsiKeyParserTests
{
    [Fact]
    public void Parse_SingleLetter_ReturnsKeyEvent()
    {
        var result = AnsiKeyParser.Parse("a".AsSpan());
        
        result.Should().NotBeNull();
        result!.Value.Key.Should().Be(Key.A);
        result.Value.Character.Should().Be('a');
    }

    [Fact]
    public void Parse_Escape_ReturnsEscapeKey()
    {
        var result = AnsiKeyParser.Parse("\u001b".AsSpan());
        
        result.Should().NotBeNull();
        result!.Value.Key.Should().Be(Key.Escape);
    }

    [Fact]
    public void Parse_CtrlA_ReturnsControlModifier()
    {
        // Ctrl+A is character code 1
        var result = AnsiKeyParser.Parse("\x01".AsSpan());
        
        result.Should().NotBeNull();
        result!.Value.Key.Should().Be(Key.A);
        result.Value.Control.Should().BeTrue();
    }

    [Fact]
    public void Parse_CtrlC_ReturnsControlC()
    {
        // Ctrl+C is character code 3
        var result = AnsiKeyParser.Parse("\x03".AsSpan());
        
        result.Should().NotBeNull();
        result!.Value.Key.Should().Be(Key.C);
        result.Value.Control.Should().BeTrue();
    }

    [Fact]
    public void Parse_Backspace_ReturnsBackspace()
    {
        var result = AnsiKeyParser.Parse("\x7f".AsSpan());
        
        result.Should().NotBeNull();
        result!.Value.Key.Should().Be(Key.Backspace);
    }

    [Fact]
    public void Parse_ArrowUp_ReturnsUpKey()
    {
        var result = AnsiKeyParser.Parse("\u001b[A".AsSpan());
        
        result.Should().NotBeNull();
        result!.Value.Key.Should().Be(Key.Up);
    }

    [Fact]
    public void Parse_ArrowDown_ReturnsDownKey()
    {
        var result = AnsiKeyParser.Parse("\u001b[B".AsSpan());
        
        result.Should().NotBeNull();
        result!.Value.Key.Should().Be(Key.Down);
    }

    [Fact]
    public void Parse_ArrowRight_ReturnsRightKey()
    {
        var result = AnsiKeyParser.Parse("\u001b[C".AsSpan());
        
        result.Should().NotBeNull();
        result!.Value.Key.Should().Be(Key.Right);
    }

    [Fact]
    public void Parse_ArrowLeft_ReturnsLeftKey()
    {
        var result = AnsiKeyParser.Parse("\u001b[D".AsSpan());
        
        result.Should().NotBeNull();
        result!.Value.Key.Should().Be(Key.Left);
    }

    [Fact]
    public void Parse_Home_ReturnsHomeKey()
    {
        var result = AnsiKeyParser.Parse("\u001b[H".AsSpan());
        
        result.Should().NotBeNull();
        result!.Value.Key.Should().Be(Key.Home);
    }

    [Fact]
    public void Parse_End_ReturnsEndKey()
    {
        var result = AnsiKeyParser.Parse("\u001b[F".AsSpan());
        
        result.Should().NotBeNull();
        result!.Value.Key.Should().Be(Key.End);
    }

    [Fact]
    public void Parse_ShiftTab_ReturnsShiftTab()
    {
        var result = AnsiKeyParser.Parse("\u001b[Z".AsSpan());
        
        result.Should().NotBeNull();
        result!.Value.Key.Should().Be(Key.Tab);
        result.Value.Shift.Should().BeTrue();
    }

    [Fact]
    public void Parse_Delete_ReturnsDeleteKey()
    {
        var result = AnsiKeyParser.Parse("\u001b[3~".AsSpan());
        
        result.Should().NotBeNull();
        result!.Value.Key.Should().Be(Key.Delete);
    }

    [Fact]
    public void Parse_Insert_ReturnsInsertKey()
    {
        var result = AnsiKeyParser.Parse("\u001b[2~".AsSpan());
        
        result.Should().NotBeNull();
        result!.Value.Key.Should().Be(Key.Insert);
    }

    [Fact]
    public void Parse_PageUp_ReturnsPageUpKey()
    {
        var result = AnsiKeyParser.Parse("\u001b[5~".AsSpan());
        
        result.Should().NotBeNull();
        result!.Value.Key.Should().Be(Key.PageUp);
    }

    [Fact]
    public void Parse_PageDown_ReturnsPageDownKey()
    {
        var result = AnsiKeyParser.Parse("\u001b[6~".AsSpan());
        
        result.Should().NotBeNull();
        result!.Value.Key.Should().Be(Key.PageDown);
    }

    [Fact]
    public void Parse_F1_ReturnsF1Key()
    {
        var result = AnsiKeyParser.Parse("\u001b[11~".AsSpan());
        
        result.Should().NotBeNull();
        result!.Value.Key.Should().Be(Key.F1);
    }

    [Fact]
    public void Parse_F5_ReturnsF5Key()
    {
        var result = AnsiKeyParser.Parse("\u001b[15~".AsSpan());
        
        result.Should().NotBeNull();
        result!.Value.Key.Should().Be(Key.F5);
    }

    [Fact]
    public void Parse_SS3_F1_ReturnsF1Key()
    {
        var result = AnsiKeyParser.Parse("\u001bOP".AsSpan());
        
        result.Should().NotBeNull();
        result!.Value.Key.Should().Be(Key.F1);
    }

    [Fact]
    public void Parse_AltA_ReturnsAltModifier()
    {
        // Use explicit escape to avoid hex escape ambiguity
        var input = "\u001ba";  // ESC followed by 'a'
        var result = AnsiKeyParser.Parse(input.AsSpan());
        
        result.Should().NotBeNull();
        result!.Value.Key.Should().Be(Key.A);
        result.Value.Alt.Should().BeTrue();
    }

    [Fact]
    public void Parse_EmptyInput_ReturnsNull()
    {
        var result = AnsiKeyParser.Parse(ReadOnlySpan<char>.Empty);
        
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("\u001b[A", Key.Up)]
    [InlineData("\u001b[B", Key.Down)]
    [InlineData("\u001b[C", Key.Right)]
    [InlineData("\u001b[D", Key.Left)]
    [InlineData("\u001b[H", Key.Home)]
    [InlineData("\u001b[F", Key.End)]
    public void Parse_NavigationKeys_ReturnsCorrectKey(string input, Key expectedKey)
    {
        var result = AnsiKeyParser.Parse(input.AsSpan());
        
        result.Should().NotBeNull();
        result!.Value.Key.Should().Be(expectedKey);
    }

    [Theory]
    [InlineData("\u001b[1~", Key.Home)]
    [InlineData("\u001b[2~", Key.Insert)]
    [InlineData("\u001b[3~", Key.Delete)]
    [InlineData("\u001b[4~", Key.End)]
    [InlineData("\u001b[5~", Key.PageUp)]
    [InlineData("\u001b[6~", Key.PageDown)]
    public void Parse_TildeSequences_ReturnsCorrectKey(string input, Key expectedKey)
    {
        var result = AnsiKeyParser.Parse(input.AsSpan());
        
        result.Should().NotBeNull();
        result!.Value.Key.Should().Be(expectedKey);
    }

    [Fact]
    public void Parse_CtrlShiftArrow_ReturnsModifiers()
    {
        // ESC [ 1 ; 6 A = Ctrl+Shift+Up (modifier 6 = 1 + shift(1) + ctrl(4))
        var result = AnsiKeyParser.Parse("\u001b[1;6A".AsSpan());
        
        result.Should().NotBeNull();
        result!.Value.Key.Should().Be(Key.Up);
        result.Value.Control.Should().BeTrue();
        result.Value.Shift.Should().BeTrue();
    }
}
