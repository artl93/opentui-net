using FluentAssertions;
using OpenTUI.Core.Console;

namespace OpenTUI.Core.Tests.Console;

public class ConsoleInterceptorTests
{
    [Fact]
    public void Write_SingleLine_AddsToBuffer()
    {
        var buffer = new LogBuffer();
        using var original = new StringWriter();
        using var interceptor = new ConsoleInterceptor(original, buffer);

        interceptor.WriteLine("Hello World");

        buffer.Count.Should().Be(1);
        buffer.GetEntries().First().Message.Should().Be("Hello World");
    }

    [Fact]
    public void Write_MultipleLines_AddsEach()
    {
        var buffer = new LogBuffer();
        using var original = new StringWriter();
        using var interceptor = new ConsoleInterceptor(original, buffer);

        interceptor.WriteLine("Line 1");
        interceptor.WriteLine("Line 2");

        buffer.Count.Should().Be(2);
    }

    [Fact]
    public void Write_CharByChar_BuffersUntilNewline()
    {
        var buffer = new LogBuffer();
        using var original = new StringWriter();
        using var interceptor = new ConsoleInterceptor(original, buffer);

        interceptor.Write('H');
        interceptor.Write('i');
        buffer.Count.Should().Be(0);

        interceptor.Write('\n');
        buffer.Count.Should().Be(1);
        buffer.GetEntries().First().Message.Should().Be("Hi");
    }

    [Fact]
    public void Write_WithNewlineInString_SplitsLines()
    {
        var buffer = new LogBuffer();
        using var original = new StringWriter();
        using var interceptor = new ConsoleInterceptor(original, buffer);

        interceptor.Write("Line1\nLine2\n");

        buffer.Count.Should().Be(2);
    }

    [Fact]
    public void Write_IgnoresCarriageReturn()
    {
        var buffer = new LogBuffer();
        using var original = new StringWriter();
        using var interceptor = new ConsoleInterceptor(original, buffer);

        // \r\n should be treated as just newline (CR stripped, LF causes flush)
        interceptor.WriteLine("Hello\r");
        interceptor.WriteLine("World");

        buffer.Count.Should().Be(2);
        buffer.GetEntries().First().Message.Should().Be("Hello");
    }

    [Fact]
    public void PassThrough_True_WritesToOriginal()
    {
        var buffer = new LogBuffer();
        using var original = new StringWriter();
        using var interceptor = new ConsoleInterceptor(original, buffer) { PassThrough = true };

        interceptor.WriteLine("Test");

        original.ToString().Should().Contain("Test");
    }

    [Fact]
    public void PassThrough_False_DoesNotWriteToOriginal()
    {
        var buffer = new LogBuffer();
        using var original = new StringWriter();
        using var interceptor = new ConsoleInterceptor(original, buffer) { PassThrough = false };

        interceptor.WriteLine("Test");

        original.ToString().Should().BeEmpty();
    }

    [Fact]
    public void DetectsErrorLevel_FromPrefix()
    {
        var buffer = new LogBuffer();
        using var original = new StringWriter();
        using var interceptor = new ConsoleInterceptor(original, buffer);

        interceptor.WriteLine("[ERROR] Something failed");

        buffer.GetEntries().First().Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public void DetectsWarningLevel_FromPrefix()
    {
        var buffer = new LogBuffer();
        using var original = new StringWriter();
        using var interceptor = new ConsoleInterceptor(original, buffer);

        interceptor.WriteLine("[WARNING] Watch out");

        buffer.GetEntries().First().Level.Should().Be(LogLevel.Warning);
    }

    [Fact]
    public void DetectsDebugLevel_FromPrefix()
    {
        var buffer = new LogBuffer();
        using var original = new StringWriter();
        using var interceptor = new ConsoleInterceptor(original, buffer);

        interceptor.WriteLine("[DEBUG] Trace info");

        buffer.GetEntries().First().Level.Should().Be(LogLevel.Debug);
    }

    [Fact]
    public void DetectsInfoLevel_FromPrefix()
    {
        var buffer = new LogBuffer();
        using var original = new StringWriter();
        using var interceptor = new ConsoleInterceptor(original, buffer);

        interceptor.WriteLine("[INFO] Just info");

        buffer.GetEntries().First().Level.Should().Be(LogLevel.Info);
    }

    [Fact]
    public void DetectsLevel_CaseInsensitive()
    {
        var buffer = new LogBuffer();
        using var original = new StringWriter();
        using var interceptor = new ConsoleInterceptor(original, buffer);

        interceptor.WriteLine("error: lowercase");

        buffer.GetEntries().First().Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public void DefaultLevel_UsedWhenNoPrefix()
    {
        var buffer = new LogBuffer();
        using var original = new StringWriter();
        using var interceptor = new ConsoleInterceptor(original, buffer, LogLevel.Warning);

        interceptor.WriteLine("No prefix here");

        buffer.GetEntries().First().Level.Should().Be(LogLevel.Warning);
    }

    [Fact]
    public void Source_IsSetOnEntries()
    {
        var buffer = new LogBuffer();
        using var original = new StringWriter();
        using var interceptor = new ConsoleInterceptor(original, buffer, LogLevel.Info, "MyApp");

        interceptor.WriteLine("Test");

        buffer.GetEntries().First().Source.Should().Be("MyApp");
    }

    [Fact]
    public void Flush_WritesPartialLine()
    {
        var buffer = new LogBuffer();
        using var original = new StringWriter();
        using var interceptor = new ConsoleInterceptor(original, buffer);

        interceptor.Write("Partial");
        buffer.Count.Should().Be(0);

        interceptor.Flush();
        buffer.Count.Should().Be(1);
        buffer.GetEntries().First().Message.Should().Be("Partial");
    }

    [Fact]
    public void Dispose_FlushesBuffer()
    {
        var buffer = new LogBuffer();
        using var original = new StringWriter();

        using (var interceptor = new ConsoleInterceptor(original, buffer))
        {
            interceptor.Write("Not flushed yet");
        }

        buffer.Count.Should().Be(1);
    }

    [Fact]
    public void Original_ReturnsOriginalWriter()
    {
        var buffer = new LogBuffer();
        using var original = new StringWriter();
        using var interceptor = new ConsoleInterceptor(original, buffer);

        interceptor.Original.Should().BeSameAs(original);
    }

    [Fact]
    public void Buffer_ReturnsLogBuffer()
    {
        var buffer = new LogBuffer();
        using var original = new StringWriter();
        using var interceptor = new ConsoleInterceptor(original, buffer);

        interceptor.Buffer.Should().BeSameAs(buffer);
    }

    [Fact]
    public void WriteLine_Empty_DoesNotAddEntry()
    {
        var buffer = new LogBuffer();
        using var original = new StringWriter();
        using var interceptor = new ConsoleInterceptor(original, buffer);

        interceptor.WriteLine();

        // Empty lines might be added or not - this tests current behavior
        buffer.Count.Should().Be(0);
    }

    [Fact]
    public void Write_Null_DoesNotThrow()
    {
        var buffer = new LogBuffer();
        using var original = new StringWriter();
        using var interceptor = new ConsoleInterceptor(original, buffer);

        var act = () => interceptor.Write((string?)null);

        act.Should().NotThrow();
    }
}
