using FluentAssertions;
using OpenTUI.Core.Terminal;

namespace OpenTUI.Core.Tests.Terminal;

public class TerminalCapabilitiesTests
{
    [Fact]
    public void Detect_ReturnsNonNullCapabilities()
    {
        var caps = TerminalCapabilities.Detect();
        
        caps.Should().NotBeNull();
    }

    [Fact]
    public void Dumb_HasMinimalCapabilities()
    {
        var caps = TerminalCapabilities.Dumb;
        
        caps.ColorSupport.Should().Be(ColorSupport.None);
        caps.SupportsUnicode.Should().BeFalse();
        caps.SupportsAlternateScreen.Should().BeFalse();
        caps.SupportsMouse.Should().BeFalse();
        caps.SupportsBracketedPaste.Should().BeFalse();
        caps.TermType.Should().Be("dumb");
    }

    [Fact]
    public void Full_HasAllCapabilities()
    {
        var caps = TerminalCapabilities.Full;
        
        caps.ColorSupport.Should().Be(ColorSupport.TrueColor);
        caps.SupportsUnicode.Should().BeTrue();
        caps.SupportsAlternateScreen.Should().BeTrue();
        caps.SupportsMouse.Should().BeTrue();
        caps.SupportsBracketedPaste.Should().BeTrue();
    }

    [Fact]
    public void InitWithProperties_SetsCorrectly()
    {
        var caps = new TerminalCapabilities
        {
            ColorSupport = ColorSupport.Palette256,
            SupportsUnicode = true,
            SupportsAlternateScreen = false,
            IsCI = true
        };
        
        caps.ColorSupport.Should().Be(ColorSupport.Palette256);
        caps.SupportsUnicode.Should().BeTrue();
        caps.SupportsAlternateScreen.Should().BeFalse();
        caps.IsCI.Should().BeTrue();
    }
}

public class ColorSupportTests
{
    [Fact]
    public void ColorSupport_HasCorrectOrdering()
    {
        ((int)ColorSupport.None).Should().BeLessThan((int)ColorSupport.Basic);
        ((int)ColorSupport.Basic).Should().BeLessThan((int)ColorSupport.Palette256);
        ((int)ColorSupport.Palette256).Should().BeLessThan((int)ColorSupport.TrueColor);
    }

    [Fact]
    public void ColorSupport_HasExpectedValues()
    {
        ((int)ColorSupport.None).Should().Be(0);
        ((int)ColorSupport.Basic).Should().Be(1);
        ((int)ColorSupport.Palette256).Should().Be(2);
        ((int)ColorSupport.TrueColor).Should().Be(3);
    }
}
