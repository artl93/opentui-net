using FluentAssertions;
using OpenTUI.Core.Console;

namespace OpenTUI.Core.Tests.Console;

public class LogEntryTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var entry = new LogEntry("Test message", LogLevel.Warning, "TestSource");

        entry.Message.Should().Be("Test message");
        entry.Level.Should().Be(LogLevel.Warning);
        entry.Source.Should().Be("TestSource");
        entry.Timestamp.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_DefaultsToInfo()
    {
        var entry = new LogEntry("Test");
        entry.Level.Should().Be(LogLevel.Info);
    }

    [Fact]
    public void Constructor_DefaultSourceIsNull()
    {
        var entry = new LogEntry("Test");
        entry.Source.Should().BeNull();
    }

    [Fact]
    public void ToString_WithoutSource_FormatsCorrectly()
    {
        var entry = new LogEntry("Hello world", LogLevel.Error);
        var str = entry.ToString();

        str.Should().Contain("[E]");
        str.Should().Contain("Hello world");
        str.Should().MatchRegex(@"\[\d{2}:\d{2}:\d{2}\.\d{3}\]");
    }

    [Fact]
    public void ToString_WithSource_IncludesSource()
    {
        var entry = new LogEntry("Test", LogLevel.Debug, "MySource");
        var str = entry.ToString();

        str.Should().Contain("[D]");
        str.Should().Contain("[MySource]");
        str.Should().Contain("Test");
    }

    [Theory]
    [InlineData(LogLevel.Debug, "[D]")]
    [InlineData(LogLevel.Info, "[I]")]
    [InlineData(LogLevel.Warning, "[W]")]
    [InlineData(LogLevel.Error, "[E]")]
    public void ToString_ShowsCorrectLevelPrefix(LogLevel level, string expectedPrefix)
    {
        var entry = new LogEntry("msg", level);
        entry.ToString().Should().Contain(expectedPrefix);
    }

    [Fact]
    public void LogLevel_HasCorrectOrder()
    {
        ((int)LogLevel.Debug).Should().BeLessThan((int)LogLevel.Info);
        ((int)LogLevel.Info).Should().BeLessThan((int)LogLevel.Warning);
        ((int)LogLevel.Warning).Should().BeLessThan((int)LogLevel.Error);
    }
}
