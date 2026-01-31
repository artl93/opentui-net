using FluentAssertions;
using OpenTUI.Core.Terminal;

namespace OpenTUI.Core.Tests.Terminal;

public class TerminalSizeTests
{
    [Fact]
    public void Constructor_SetsWidthAndHeight()
    {
        var size = new TerminalSize(80, 24);
        
        size.Width.Should().Be(80);
        size.Height.Should().Be(24);
    }

    [Fact]
    public void GetCurrent_ReturnsValidSize()
    {
        var size = TerminalSize.GetCurrent();
        
        size.Width.Should().BeGreaterThan(0);
        size.Height.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var size = new TerminalSize(120, 40);
        
        size.ToString().Should().Be("120x40");
    }

    [Fact]
    public void Equality_WorksCorrectly()
    {
        var a = new TerminalSize(80, 24);
        var b = new TerminalSize(80, 24);
        var c = new TerminalSize(100, 30);
        
        (a == b).Should().BeTrue();
        (a != c).Should().BeTrue();
        a.Equals(b).Should().BeTrue();
        a.Equals((object)b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Equality_HandlesNull()
    {
        var size = new TerminalSize(80, 24);
        TerminalSize? nullSize = null;
        
        (size == nullSize).Should().BeFalse();
        (nullSize == size).Should().BeFalse();
        (nullSize == null).Should().BeTrue();
    }
}
