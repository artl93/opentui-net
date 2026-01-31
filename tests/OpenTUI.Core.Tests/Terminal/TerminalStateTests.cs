using FluentAssertions;
using OpenTUI.Core.Terminal;
using OpenTUI.Core.Rendering;

namespace OpenTUI.Core.Tests.Terminal;

public class TerminalStateTests
{
    [Fact]
    public void Constructor_SetsCapabilities()
    {
        var caps = TerminalCapabilities.Full;
        using var state = new TerminalState(TextWriter.Null, caps);
        
        state.Capabilities.Should().Be(caps);
    }

    [Fact]
    public void Constructor_DetectsCapabilitiesIfNotProvided()
    {
        using var state = new TerminalState(TextWriter.Null);
        
        state.Capabilities.Should().NotBeNull();
    }

    [Fact]
    public void IsRawMode_InitiallyFalse()
    {
        using var state = new TerminalState(TextWriter.Null);
        
        state.IsRawMode.Should().BeFalse();
    }

    [Fact]
    public void Write_WritesToOutput()
    {
        var writer = new StringWriter();
        using var state = new TerminalState(writer);
        
        state.Write("Hello");
        
        writer.ToString().Should().Be("Hello");
    }

    [Fact]
    public void ClearScreen_WritesClearSequence()
    {
        var writer = new StringWriter();
        using var state = new TerminalState(writer);
        
        state.ClearScreen();
        
        var output = writer.ToString();
        output.Should().Contain(Ansi.ClearScreen);
        output.Should().Contain("\x1b[1;1H"); // MoveCursor(1, 1)
    }

    [Fact]
    public void HideCursor_WritesHideSequence()
    {
        var writer = new StringWriter();
        using var state = new TerminalState(writer, TerminalCapabilities.Full);
        
        state.HideCursor();
        
        writer.ToString().Should().Contain(Ansi.HideCursor);
    }

    [Fact]
    public void ShowCursor_WritesShowSequence()
    {
        var writer = new StringWriter();
        using var state = new TerminalState(writer, TerminalCapabilities.Full);
        
        state.HideCursor();
        state.ShowCursor();
        
        writer.ToString().Should().Contain(Ansi.ShowCursor);
    }

    [Fact]
    public void HideCursor_IsIdempotent()
    {
        var writer = new StringWriter();
        using var state = new TerminalState(writer, TerminalCapabilities.Full);
        
        state.HideCursor();
        var lengthAfterFirst = writer.ToString().Length;
        
        state.HideCursor();
        var lengthAfterSecond = writer.ToString().Length;
        
        lengthAfterSecond.Should().Be(lengthAfterFirst);
    }

    [Fact]
    public void EnterAlternateScreen_WritesSequence()
    {
        var writer = new StringWriter();
        using var state = new TerminalState(writer, TerminalCapabilities.Full);
        
        state.EnterAlternateScreen();
        
        writer.ToString().Should().Contain(Ansi.EnterAlternateScreen);
    }

    [Fact]
    public void ExitAlternateScreen_WritesSequence()
    {
        var writer = new StringWriter();
        using var state = new TerminalState(writer, TerminalCapabilities.Full);
        
        state.EnterAlternateScreen();
        state.ExitAlternateScreen();
        
        writer.ToString().Should().Contain(Ansi.ExitAlternateScreen);
    }

    [Fact]
    public void EnterAlternateScreen_RespectsCapabilities()
    {
        var writer = new StringWriter();
        var caps = new TerminalCapabilities { SupportsAlternateScreen = false };
        using var state = new TerminalState(writer, caps);
        
        state.EnterAlternateScreen();
        
        writer.ToString().Should().NotContain(Ansi.EnterAlternateScreen);
    }

    [Fact]
    public void EnableMouse_WritesSequence()
    {
        var writer = new StringWriter();
        using var state = new TerminalState(writer, TerminalCapabilities.Full);
        
        state.EnableMouse();
        
        writer.ToString().Should().Contain(Ansi.EnableMouse);
    }

    [Fact]
    public void EnableMouse_RespectsCapabilities()
    {
        var writer = new StringWriter();
        var caps = new TerminalCapabilities { SupportsMouse = false };
        using var state = new TerminalState(writer, caps);
        
        state.EnableMouse();
        
        writer.ToString().Should().NotContain(Ansi.EnableMouse);
    }

    [Fact]
    public void EnableBracketedPaste_WritesSequence()
    {
        var writer = new StringWriter();
        using var state = new TerminalState(writer, TerminalCapabilities.Full);
        
        state.EnableBracketedPaste();
        
        writer.ToString().Should().Contain(Ansi.EnableBracketedPaste);
    }

    [Fact]
    public void Dispose_RestoresState()
    {
        var writer = new StringWriter();
        var state = new TerminalState(writer, TerminalCapabilities.Full);
        
        state.HideCursor();
        state.EnterAlternateScreen();
        state.EnableMouse();
        
        state.Dispose();
        
        var output = writer.ToString();
        output.Should().Contain(Ansi.ShowCursor);
        output.Should().Contain(Ansi.ExitAlternateScreen);
        output.Should().Contain(Ansi.DisableMouse);
        output.Should().Contain(Ansi.Reset);
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        var writer = new StringWriter();
        var state = new TerminalState(writer, TerminalCapabilities.Full);
        state.HideCursor();
        
        state.Dispose();
        var lengthAfterFirst = writer.ToString().Length;
        
        state.Dispose();
        var lengthAfterSecond = writer.ToString().Length;
        
        lengthAfterSecond.Should().Be(lengthAfterFirst);
    }
}
