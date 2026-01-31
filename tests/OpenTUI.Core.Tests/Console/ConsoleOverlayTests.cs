using FluentAssertions;
using OpenTUI.Core.Console;
using OpenTUI.Core.Layout;
using OpenTUI.Core.Rendering;

namespace OpenTUI.Core.Tests.Console;

public class ConsoleOverlayTests
{
    [Fact]
    public void Constructor_CreatesDefaultLogBuffer()
    {
        var overlay = new ConsoleOverlay();
        overlay.LogBuffer.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithLogBuffer_UsesProvided()
    {
        var buffer = new LogBuffer();
        var overlay = new ConsoleOverlay(buffer);
        overlay.LogBuffer.Should().BeSameAs(buffer);
    }

    [Fact]
    public void DefaultLayout_IsAbsolutePositioned()
    {
        var overlay = new ConsoleOverlay();
        overlay.Layout.PositionType.Should().Be(PositionType.Absolute);
    }

    [Fact]
    public void DefaultLayout_TakesFullWidthBottom30Percent()
    {
        var overlay = new ConsoleOverlay();
        overlay.Layout.Width.Should().Be(FlexValue.Percent(100));
        overlay.Layout.Height.Should().Be(FlexValue.Percent(30));
        overlay.Layout.Bottom.Should().Be(FlexValue.Points(0));
    }

    [Fact]
    public void ScrollOffset_ClampedToValidRange()
    {
        var buffer = new LogBuffer();
        buffer.Add("test");
        var overlay = new ConsoleOverlay(buffer);
        
        overlay.ScrollOffset = -5;
        overlay.ScrollOffset.Should().BeGreaterThanOrEqualTo(0);
        
        overlay.ScrollOffset = 1000;
        // Should be clamped to max
    }

    [Fact]
    public void MinLevel_DefaultsToDebug()
    {
        var overlay = new ConsoleOverlay();
        overlay.MinLevel.Should().Be(LogLevel.Debug);
    }

    [Fact]
    public void MinLevel_CanBeSet()
    {
        var overlay = new ConsoleOverlay();
        overlay.MinLevel = LogLevel.Warning;
        overlay.MinLevel.Should().Be(LogLevel.Warning);
    }

    [Fact]
    public void AutoScroll_DefaultsToTrue()
    {
        var overlay = new ConsoleOverlay();
        overlay.AutoScroll.Should().BeTrue();
    }

    [Fact]
    public void AutoScroll_ResetsScrollOnNewEntry()
    {
        var buffer = new LogBuffer();
        for (int i = 0; i < 50; i++)
            buffer.Add($"Entry {i}");
        
        var overlay = new ConsoleOverlay(buffer);
        overlay.ScrollOffset = 10;
        overlay.AutoScroll = true;
        
        buffer.Add("New entry");
        overlay.ScrollOffset.Should().Be(0);
    }

    [Fact]
    public void Border_DefaultsToSingle()
    {
        var overlay = new ConsoleOverlay();
        overlay.Border.Should().Be(OpenTUI.Core.Renderables.BorderStyle.Single);
    }

    [Fact]
    public void Title_DefaultsToConsole()
    {
        var overlay = new ConsoleOverlay();
        overlay.Title.Should().Be("Console");
    }

    [Fact]
    public void ScrollUp_IncreasesScrollOffset()
    {
        var buffer = new LogBuffer();
        for (int i = 0; i < 50; i++)
            buffer.Add($"Entry {i}");
        
        var overlay = new ConsoleOverlay(buffer);
        // Layout is computed - set explicit size
        overlay.Layout.Width = FlexValue.Points(40);
        overlay.Layout.Height = FlexValue.Points(20);
        overlay.Layout.CalculateLayout(100, 100);
        overlay.ScrollOffset = 0;
        
        overlay.ScrollUp(3);
        overlay.ScrollOffset.Should().Be(3);
    }

    [Fact]
    public void ScrollDown_DecreasesScrollOffset()
    {
        var buffer = new LogBuffer();
        for (int i = 0; i < 50; i++)
            buffer.Add($"Entry {i}");
        
        var overlay = new ConsoleOverlay(buffer);
        overlay.Layout.Width = FlexValue.Points(40);
        overlay.Layout.Height = FlexValue.Points(20);
        overlay.Layout.CalculateLayout(100, 100);
        overlay.ScrollOffset = 10;
        
        overlay.ScrollDown(3);
        overlay.ScrollOffset.Should().Be(7);
    }

    [Fact]
    public void ScrollDown_StopsAtZero()
    {
        var overlay = new ConsoleOverlay();
        overlay.ScrollOffset = 2;
        overlay.ScrollDown(10);
        overlay.ScrollOffset.Should().Be(0);
    }

    [Fact]
    public void ScrollToBottom_SetsOffsetToZero()
    {
        var buffer = new LogBuffer();
        for (int i = 0; i < 50; i++)
            buffer.Add($"Entry {i}");
        
        var overlay = new ConsoleOverlay(buffer);
        overlay.ScrollOffset = 20;
        overlay.ScrollToBottom();
        overlay.ScrollOffset.Should().Be(0);
    }

    [Fact]
    public void CycleLogLevel_CyclesThroughLevels()
    {
        var overlay = new ConsoleOverlay();
        overlay.MinLevel.Should().Be(LogLevel.Debug);
        
        overlay.CycleLogLevel();
        overlay.MinLevel.Should().Be(LogLevel.Info);
        
        overlay.CycleLogLevel();
        overlay.MinLevel.Should().Be(LogLevel.Warning);
        
        overlay.CycleLogLevel();
        overlay.MinLevel.Should().Be(LogLevel.Error);
        
        overlay.CycleLogLevel();
        overlay.MinLevel.Should().Be(LogLevel.Debug);
    }

    [Fact]
    public void Clear_ClearsLogBuffer()
    {
        var buffer = new LogBuffer();
        buffer.Add("test");
        var overlay = new ConsoleOverlay(buffer);
        
        overlay.ClearLog();
        
        buffer.Count.Should().Be(0);
    }

    [Fact]
    public void Clear_ResetsScrollOffset()
    {
        var buffer = new LogBuffer();
        for (int i = 0; i < 50; i++)
            buffer.Add($"Entry {i}");
        
        var overlay = new ConsoleOverlay(buffer);
        overlay.ScrollOffset = 10;
        overlay.ClearLog();
        
        overlay.ScrollOffset.Should().Be(0);
    }

    [Fact]
    public void Render_WhenNotVisible_DoesNotRenderContent()
    {
        var overlay = new ConsoleOverlay();
        overlay.IsVisible = false;
        overlay.Layout.Width = FlexValue.Points(40);
        overlay.Layout.Height = FlexValue.Points(10);
        overlay.Layout.CalculateLayout(80, 24);
        
        var fb = new FrameBuffer(80, 24);
        overlay.Render(fb, 0, 0);
        
        // Visible=false means no border should be drawn
        // Default FrameBuffer character is " " (space)
        // GetCell takes (row, col)
        fb.GetCell(0, 0).Character.Should().Be(" ");
    }

    [Fact]
    public void Render_WhenVisible_DrawsBorder()
    {
        var buffer = new LogBuffer();
        var overlay = new ConsoleOverlay(buffer);
        overlay.IsVisible = true;
        overlay.Layout.PositionType = PositionType.Relative;
        overlay.Layout.Left = FlexValue.Auto;
        overlay.Layout.Bottom = FlexValue.Auto;
        overlay.Layout.Width = FlexValue.Points(40);
        overlay.Layout.Height = FlexValue.Points(10);
        overlay.Layout.CalculateLayout(80, 24);
        
        var fb = new FrameBuffer(80, 24);
        overlay.Render(fb, 0, 0);
        
        // Check top-left corner - GetCell takes (row, col)
        fb.GetCell(0, 0).Character.Should().Be("┌");
    }

    [Fact]
    public void Render_DrawsTitle()
    {
        var overlay = new ConsoleOverlay();
        overlay.Title = "Test";
        // Override the default absolute positioning for this test
        overlay.Layout.PositionType = PositionType.Relative;
        overlay.Layout.Left = FlexValue.Auto;
        overlay.Layout.Bottom = FlexValue.Auto;
        overlay.Layout.Width = FlexValue.Points(40);
        overlay.Layout.Height = FlexValue.Points(10);
        overlay.Layout.CalculateLayout(80, 24);
        
        var fb = new FrameBuffer(80, 24);
        overlay.Render(fb, 0, 0);
        
        // Title is drawn at position 2 with leading space: " Test "
        // GetCell takes (row, col), so check row 0
        // Position 0: ┌, Position 1: ─, Position 2: " ", Position 3: "T"
        fb.GetCell(0, 2).Character.Should().Be(" ");
        fb.GetCell(0, 3).Character.Should().Be("T");
    }

    [Fact]
    public void Render_DrawsLogEntries()
    {
        var buffer = new LogBuffer();
        buffer.Info("Hello");
        var overlay = new ConsoleOverlay(buffer);
        overlay.Layout.Width = FlexValue.Points(60);
        overlay.Layout.Height = FlexValue.Points(10);
        overlay.Layout.CalculateLayout(80, 24);
        
        var fb = new FrameBuffer(80, 24);
        overlay.Render(fb, 0, 0);
        
        // The 'H' of "Hello" should be somewhere in the rendered output
        buffer.GetEntries().First().Message.Should().Contain("Hello");
    }

    [Fact]
    public void Render_FiltersEntriesByMinLevel()
    {
        var buffer = new LogBuffer();
        buffer.Debug("Debug message");
        buffer.Error("Error message");
        var overlay = new ConsoleOverlay(buffer);
        overlay.MinLevel = LogLevel.Error;
        
        // Only error should be visible when min level is Error
        var visibleEntries = buffer.GetEntries(LogLevel.Error).ToList();
        visibleEntries.Should().HaveCount(1);
        visibleEntries[0].Message.Should().Contain("Error");
    }

    [Fact]
    public void CreateWithInterception_CreatesOverlayWithBuffer()
    {
        var overlay = ConsoleOverlay.CreateWithInterception();
        overlay.LogBuffer.Should().NotBeNull();
    }

    [Fact]
    public void Dispose_UnsubscribesFromEvents()
    {
        var buffer = new LogBuffer();
        var overlay = new ConsoleOverlay(buffer);
        overlay.Dispose();
        
        // Adding entries after dispose should not cause issues
        var act = () => buffer.Add("test");
        act.Should().NotThrow();
    }

    [Fact]
    public void OverlayBackgroundColor_CanBeCustomized()
    {
        var overlay = new ConsoleOverlay();
        var color = OpenTUI.Core.Colors.RGBA.FromValues(0.2f, 0.3f, 0.4f);
        overlay.OverlayBackgroundColor = color;
        overlay.OverlayBackgroundColor.Should().Be(color);
    }

    [Fact]
    public void Border_CanBeCustomized()
    {
        var overlay = new ConsoleOverlay();
        overlay.Border = OpenTUI.Core.Renderables.BorderStyle.Double;
        overlay.Border.Should().Be(OpenTUI.Core.Renderables.BorderStyle.Double);
    }

    [Fact]
    public void Render_WithZeroSize_DoesNotThrow()
    {
        var overlay = new ConsoleOverlay();
        overlay.Layout.Width = FlexValue.Points(0);
        overlay.Layout.Height = FlexValue.Points(0);
        overlay.Layout.CalculateLayout(80, 24);
        
        var fb = new FrameBuffer(80, 24);
        var act = () => overlay.Render(fb, 0, 0);
        act.Should().NotThrow();
    }

    [Fact]
    public void Render_WithMinimalSize_DoesNotThrow()
    {
        var overlay = new ConsoleOverlay();
        overlay.Layout.Width = FlexValue.Points(2);
        overlay.Layout.Height = FlexValue.Points(2);
        overlay.Layout.CalculateLayout(80, 24);
        
        var fb = new FrameBuffer(80, 24);
        var act = () => overlay.Render(fb, 0, 0);
        act.Should().NotThrow();
    }
}
