using FluentAssertions;
using OpenTUI.Core.Input;

namespace OpenTUI.Core.Tests.Input;

public class KeyEventTests
{
    [Fact]
    public void FromChar_Letter_CreatesCorrectEvent()
    {
        var keyEvent = KeyEvent.FromChar('a');

        keyEvent.Key.Should().Be(Key.A);
        keyEvent.Character.Should().Be('a');
        keyEvent.Modifiers.Should().Be(KeyModifiers.None);
    }

    [Fact]
    public void FromChar_UppercaseLetter_CreatesCorrectEvent()
    {
        var keyEvent = KeyEvent.FromChar('A');

        keyEvent.Key.Should().Be(Key.A);
        keyEvent.Character.Should().Be('A');
    }

    [Fact]
    public void FromChar_Number_CreatesCorrectEvent()
    {
        var keyEvent = KeyEvent.FromChar('5');

        keyEvent.Key.Should().Be(Key.D5);
        keyEvent.Character.Should().Be('5');
    }

    [Fact]
    public void FromChar_Space_CreatesSpaceEvent()
    {
        var keyEvent = KeyEvent.FromChar(' ');

        keyEvent.Key.Should().Be(Key.Space);
        keyEvent.Character.Should().Be(' ');
    }

    [Fact]
    public void FromChar_Tab_CreatesTabEvent()
    {
        var keyEvent = KeyEvent.FromChar('\t');

        keyEvent.Key.Should().Be(Key.Tab);
    }

    [Fact]
    public void FromChar_Enter_CreatesEnterEvent()
    {
        var keyEvent = KeyEvent.FromChar('\r');

        keyEvent.Key.Should().Be(Key.Enter);
    }

    [Fact]
    public void FromKey_WithModifiers_SetsModifiers()
    {
        var keyEvent = KeyEvent.FromKey(Key.A, KeyModifiers.Control | KeyModifiers.Shift);

        keyEvent.Key.Should().Be(Key.A);
        keyEvent.Control.Should().BeTrue();
        keyEvent.Shift.Should().BeTrue();
        keyEvent.Alt.Should().BeFalse();
    }

    [Fact]
    public void IsPrintable_Letter_ReturnsTrue()
    {
        var keyEvent = KeyEvent.FromChar('a');

        keyEvent.IsPrintable.Should().BeTrue();
    }

    [Fact]
    public void IsPrintable_ControlKey_ReturnsFalse()
    {
        var keyEvent = KeyEvent.FromKey(Key.Enter);

        keyEvent.IsPrintable.Should().BeFalse();
    }

    [Fact]
    public void ToString_SimpleKey_ReturnsKeyName()
    {
        var keyEvent = KeyEvent.FromKey(Key.Enter);

        keyEvent.ToString().Should().Be("Enter");
    }

    [Fact]
    public void ToString_WithModifiers_IncludesModifiers()
    {
        var keyEvent = KeyEvent.FromKey(Key.A, KeyModifiers.Control | KeyModifiers.Shift);

        keyEvent.ToString().Should().Contain("Ctrl");
        keyEvent.ToString().Should().Contain("Shift");
    }

    [Fact]
    public void ToString_PrintableChar_ShowsQuotedChar()
    {
        var keyEvent = KeyEvent.FromChar('x');

        keyEvent.ToString().Should().Contain("'x'");
    }

    [Theory]
    [InlineData('a', Key.A)]
    [InlineData('z', Key.Z)]
    [InlineData('0', Key.D0)]
    [InlineData('9', Key.D9)]
    [InlineData(',', Key.Comma)]
    [InlineData('.', Key.Period)]
    [InlineData('/', Key.Slash)]
    [InlineData('-', Key.Minus)]
    public void FromChar_VariousChars_MapsCorrectly(char c, Key expectedKey)
    {
        var keyEvent = KeyEvent.FromChar(c);

        keyEvent.Key.Should().Be(expectedKey);
    }
}
