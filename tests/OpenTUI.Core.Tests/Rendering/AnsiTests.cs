using FluentAssertions;
using OpenTUI.Core.Colors;
using OpenTUI.Core.Rendering;

namespace OpenTUI.Core.Tests.Rendering;

public class AnsiTests
{
    [Fact]
    public void MoveCursor_GeneratesCorrectSequence()
    {
        Ansi.MoveCursor(5, 10).Should().Be("\x1b[5;10H");
    }

    [Fact]
    public void MoveCursorDirections_GenerateCorrectSequences()
    {
        Ansi.MoveCursorUp(3).Should().Be("\x1b[3A");
        Ansi.MoveCursorDown(2).Should().Be("\x1b[2B");
        Ansi.MoveCursorRight(5).Should().Be("\x1b[5C");
        Ansi.MoveCursorLeft(1).Should().Be("\x1b[1D");
    }

    [Fact]
    public void CursorVisibility_GeneratesCorrectSequences()
    {
        Ansi.HideCursor.Should().Be("\x1b[?25l");
        Ansi.ShowCursor.Should().Be("\x1b[?25h");
    }

    [Fact]
    public void ScreenControl_GeneratesCorrectSequences()
    {
        Ansi.ClearScreen.Should().Be("\x1b[2J");
        Ansi.ClearLine.Should().Be("\x1b[2K");
    }

    [Fact]
    public void AlternateScreen_GeneratesCorrectSequences()
    {
        Ansi.EnterAlternateScreen.Should().Be("\x1b[?1049h");
        Ansi.ExitAlternateScreen.Should().Be("\x1b[?1049l");
    }

    [Fact]
    public void SetForeground_GeneratesTrueColorSequence()
    {
        var color = RGBA.FromInts(255, 128, 64);

        Ansi.SetForeground(color).Should().Be("\x1b[38;2;255;128;64m");
    }

    [Fact]
    public void SetBackground_GeneratesTrueColorSequence()
    {
        var color = RGBA.FromInts(64, 128, 255);

        Ansi.SetBackground(color).Should().Be("\x1b[48;2;64;128;255m");
    }

    [Fact]
    public void SetAttributes_GeneratesCorrectSequences()
    {
        Ansi.SetAttributes(TextAttributes.Bold).Should().Contain("1m");
        Ansi.SetAttributes(TextAttributes.Italic).Should().Contain("3m");
        Ansi.SetAttributes(TextAttributes.Underline).Should().Contain("4m");
        Ansi.SetAttributes(TextAttributes.None).Should().BeEmpty();
    }

    [Fact]
    public void SetAttributes_CombinesMultiple()
    {
        var attrs = TextAttributes.Bold | TextAttributes.Underline;
        var result = Ansi.SetAttributes(attrs);

        result.Should().Contain("1m");
        result.Should().Contain("4m");
    }

    [Fact]
    public void SetStyle_CombinesFgBgAndAttributes()
    {
        var fg = RGBA.Red;
        var bg = RGBA.Blue;
        var attrs = TextAttributes.Bold;

        var result = Ansi.SetStyle(fg, bg, attrs);

        result.Should().Contain(Ansi.Reset);
        result.Should().Contain("38;2;255;0;0m"); // Red foreground
        result.Should().Contain("48;2;0;0;255m"); // Blue background
        result.Should().Contain("1m"); // Bold
    }

    [Fact]
    public void Reset_GeneratesCorrectSequence()
    {
        Ansi.Reset.Should().Be("\x1b[0m");
    }

    [Fact]
    public void StripAnsi_RemovesEscapeSequences()
    {
        var styled = "\x1b[38;2;255;0;0mHello\x1b[0m World\x1b[1m!\x1b[0m";

        Ansi.StripAnsi(styled).Should().Be("Hello World!");
    }

    [Fact]
    public void StripAnsi_HandlesEmptyAndNull()
    {
        Ansi.StripAnsi("").Should().BeEmpty();
        Ansi.StripAnsi(null!).Should().BeNull();
    }

    [Fact]
    public void StripAnsi_HandlesPlainText()
    {
        Ansi.StripAnsi("Hello World").Should().Be("Hello World");
    }

    [Fact]
    public void GetDisplayWidth_IgnoresAnsiSequences()
    {
        var styled = "\x1b[1mBold\x1b[0m";

        Ansi.GetDisplayWidth(styled).Should().Be(4); // "Bold" = 4 chars
    }

    [Fact]
    public void GetDisplayWidth_CountsPlainTextCorrectly()
    {
        Ansi.GetDisplayWidth("Hello").Should().Be(5);
    }
}
