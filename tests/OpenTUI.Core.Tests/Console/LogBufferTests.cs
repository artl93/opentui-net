using FluentAssertions;
using OpenTUI.Core.Console;

namespace OpenTUI.Core.Tests.Console;

public class LogBufferTests
{
    [Fact]
    public void Constructor_DefaultMaxEntries_Is1000()
    {
        var buffer = new LogBuffer();
        buffer.MaxEntries.Should().Be(1000);
    }

    [Fact]
    public void Constructor_CustomMaxEntries()
    {
        var buffer = new LogBuffer(500);
        buffer.MaxEntries.Should().Be(500);
    }

    [Fact]
    public void Add_IncreasesCount()
    {
        var buffer = new LogBuffer();
        buffer.Count.Should().Be(0);
        
        buffer.Add("test");
        buffer.Count.Should().Be(1);
        
        buffer.Add("test2");
        buffer.Count.Should().Be(2);
    }

    [Fact]
    public void Add_RaisesEntryAddedEvent()
    {
        var buffer = new LogBuffer();
        LogEntry? received = null;
        buffer.EntryAdded += (s, e) => received = e;
        
        buffer.Add("test message", LogLevel.Warning);
        
        received.Should().NotBeNull();
        received!.Value.Message.Should().Be("test message");
        received.Value.Level.Should().Be(LogLevel.Warning);
    }

    [Fact]
    public void Add_RemovesOldEntriesWhenOverCapacity()
    {
        var buffer = new LogBuffer(3);
        
        buffer.Add("first");
        buffer.Add("second");
        buffer.Add("third");
        buffer.Add("fourth");
        
        buffer.Count.Should().Be(3);
        var entries = buffer.GetEntries().ToList();
        entries[0].Message.Should().Be("second");
        entries[2].Message.Should().Be("fourth");
    }

    [Fact]
    public void Debug_AddsWithDebugLevel()
    {
        var buffer = new LogBuffer();
        buffer.Debug("debug msg", "src");
        
        var entry = buffer.GetEntries().First();
        entry.Level.Should().Be(LogLevel.Debug);
        entry.Source.Should().Be("src");
    }

    [Fact]
    public void Info_AddsWithInfoLevel()
    {
        var buffer = new LogBuffer();
        buffer.Info("info msg");
        
        buffer.GetEntries().First().Level.Should().Be(LogLevel.Info);
    }

    [Fact]
    public void Warning_AddsWithWarningLevel()
    {
        var buffer = new LogBuffer();
        buffer.Warning("warn msg");
        
        buffer.GetEntries().First().Level.Should().Be(LogLevel.Warning);
    }

    [Fact]
    public void Error_AddsWithErrorLevel()
    {
        var buffer = new LogBuffer();
        buffer.Error("error msg");
        
        buffer.GetEntries().First().Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public void GetEntries_ReturnsAllEntries()
    {
        var buffer = new LogBuffer();
        buffer.Add("one");
        buffer.Add("two");
        buffer.Add("three");
        
        var entries = buffer.GetEntries().ToList();
        entries.Should().HaveCount(3);
    }

    [Fact]
    public void GetEntries_WithMinLevel_FiltersCorrectly()
    {
        var buffer = new LogBuffer();
        buffer.Debug("debug");
        buffer.Info("info");
        buffer.Warning("warning");
        buffer.Error("error");
        
        buffer.GetEntries(LogLevel.Debug).Should().HaveCount(4);
        buffer.GetEntries(LogLevel.Info).Should().HaveCount(3);
        buffer.GetEntries(LogLevel.Warning).Should().HaveCount(2);
        buffer.GetEntries(LogLevel.Error).Should().HaveCount(1);
    }

    [Fact]
    public void GetRecentEntries_ReturnsLastN()
    {
        var buffer = new LogBuffer();
        for (int i = 0; i < 10; i++)
            buffer.Add($"message {i}");
        
        var recent = buffer.GetRecentEntries(3).ToList();
        recent.Should().HaveCount(3);
        recent[0].Message.Should().Be("message 7");
        recent[2].Message.Should().Be("message 9");
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        var buffer = new LogBuffer();
        buffer.Add("one");
        buffer.Add("two");
        buffer.Clear();
        
        buffer.Count.Should().Be(0);
        buffer.GetEntries().Should().BeEmpty();
    }

    [Fact]
    public void Clear_RaisesClearedEvent()
    {
        var buffer = new LogBuffer();
        buffer.Add("test");
        
        var raised = false;
        buffer.Cleared += (s, e) => raised = true;
        buffer.Clear();
        
        raised.Should().BeTrue();
    }

    [Fact]
    public void ThreadSafety_ConcurrentAdds()
    {
        var buffer = new LogBuffer(1000);
        var tasks = Enumerable.Range(0, 10)
            .Select(i => Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                    buffer.Add($"Thread {i} message {j}");
            }))
            .ToArray();
        
        Task.WaitAll(tasks);
        
        buffer.Count.Should().Be(1000);
    }
}
