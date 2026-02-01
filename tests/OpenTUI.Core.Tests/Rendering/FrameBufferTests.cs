using FluentAssertions;
using OpenTUI.Core.Colors;
using OpenTUI.Core.Rendering;

namespace OpenTUI.Core.Tests.Rendering;

public class FrameBufferTests
{
    [Fact]
    public void Constructor_CreatesBufferWithCorrectDimensions()
    {
        var buffer = new FrameBuffer(80, 24);

        buffer.Width.Should().Be(80);
        buffer.Height.Should().Be(24);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(10, 0)]
    [InlineData(-1, 10)]
    [InlineData(10, -1)]
    public void Constructor_ThrowsOnInvalidDimensions(int width, int height)
    {
        var act = () => new FrameBuffer(width, height);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SetCell_UpdatesCellContent()
    {
        var buffer = new FrameBuffer(10, 10);
        var cell = new Cell("X", RGBA.Red, RGBA.Blue);

        buffer.SetCell(5, 3, cell);

        buffer.GetCell(3, 5).Character.Should().Be("X");
        buffer.GetCell(3, 5).Foreground.Should().Be(RGBA.Red);
        buffer.GetCell(3, 5).Background.Should().Be(RGBA.Blue);
    }

    [Fact]
    public void SetCell_IgnoresOutOfBoundsCoordinates()
    {
        var buffer = new FrameBuffer(10, 10);

        // These should not throw
        buffer.SetCell(-1, 0, new Cell("X"));
        buffer.SetCell(0, -1, new Cell("X"));
        buffer.SetCell(100, 0, new Cell("X"));
        buffer.SetCell(0, 100, new Cell("X"));
    }

    [Fact]
    public void GetCell_ReturnsDefaultForOutOfBounds()
    {
        var buffer = new FrameBuffer(10, 10);

        var cell = buffer.GetCell(-1, 0);
        cell.Character.Should().Be(" ");
    }

    [Fact]
    public void SetCell_MarksCellDirty()
    {
        var buffer = new FrameBuffer(10, 10);
        buffer.ClearDirty();

        buffer.SetCell(5, 3, new Cell("X"));

        buffer.IsDirty(3, 5).Should().BeTrue();
        buffer.HasDirtyCells().Should().BeTrue();
    }

    [Fact]
    public void SetCell_DoesNotMarkDirtyIfUnchanged()
    {
        var buffer = new FrameBuffer(10, 10);
        var cell = new Cell("X", RGBA.Red);
        buffer.SetCell(5, 3, cell);
        buffer.ClearDirty();

        buffer.SetCell(5, 3, cell); // Same cell

        buffer.IsDirty(3, 5).Should().BeFalse();
    }

    [Fact]
    public void ClearDirty_ResetsAllDirtyFlags()
    {
        var buffer = new FrameBuffer(10, 10);
        buffer.SetCell(0, 0, new Cell("X"));
        buffer.SetCell(5, 5, new Cell("Y"));

        buffer.ClearDirty();

        buffer.HasDirtyCells().Should().BeFalse();
    }

    [Fact]
    public void MarkAllDirty_SetsAllCellsDirty()
    {
        var buffer = new FrameBuffer(5, 5);
        buffer.ClearDirty();

        buffer.MarkAllDirty();

        for (var r = 0; r < 5; r++)
            for (var c = 0; c < 5; c++)
                buffer.IsDirty(r, c).Should().BeTrue();
    }

    [Fact]
    public void DrawText_WritesCharactersToBuffer()
    {
        var buffer = new FrameBuffer(20, 10);

        buffer.DrawText("Hello", 5, 3, RGBA.Red);

        buffer.GetCell(3, 5).Character.Should().Be("H");
        buffer.GetCell(3, 6).Character.Should().Be("e");
        buffer.GetCell(3, 7).Character.Should().Be("l");
        buffer.GetCell(3, 8).Character.Should().Be("l");
        buffer.GetCell(3, 9).Character.Should().Be("o");
        buffer.GetCell(3, 5).Foreground.Should().Be(RGBA.Red);
    }

    [Fact]
    public void DrawText_ClipsAtBufferBoundary()
    {
        var buffer = new FrameBuffer(10, 10);

        buffer.DrawText("Hello World", 7, 0);

        buffer.GetCell(0, 7).Character.Should().Be("H");
        buffer.GetCell(0, 8).Character.Should().Be("e");
        buffer.GetCell(0, 9).Character.Should().Be("l");
        // Beyond buffer width should be ignored
    }

    [Fact]
    public void FillRect_FillsArea()
    {
        var buffer = new FrameBuffer(20, 10);

        buffer.FillRect(2, 2, 5, 3, RGBA.Blue);

        for (var r = 2; r < 5; r++)
            for (var c = 2; c < 7; c++)
                buffer.GetCell(r, c).Background.Should().Be(RGBA.Blue);
    }

    [Fact]
    public void FillRect_ClipsAtBoundary()
    {
        var buffer = new FrameBuffer(10, 10);

        // Should not throw
        buffer.FillRect(-2, -2, 20, 20, RGBA.Red);
    }

    [Fact]
    public void DrawBorder_DrawsSingleBorder()
    {
        var buffer = new FrameBuffer(10, 10);

        buffer.DrawBorder(0, 0, 5, 3, BorderStyle.Single, RGBA.White);

        // Corners
        buffer.GetCell(0, 0).Character.Should().Be("┌");
        buffer.GetCell(0, 4).Character.Should().Be("┐");
        buffer.GetCell(2, 0).Character.Should().Be("└");
        buffer.GetCell(2, 4).Character.Should().Be("┘");

        // Edges
        buffer.GetCell(0, 1).Character.Should().Be("─");
        buffer.GetCell(1, 0).Character.Should().Be("│");
    }

    [Fact]
    public void DrawBorder_DrawsDoubleBorder()
    {
        var buffer = new FrameBuffer(10, 10);

        buffer.DrawBorder(0, 0, 5, 3, BorderStyle.Double, RGBA.White);

        buffer.GetCell(0, 0).Character.Should().Be("╔");
        buffer.GetCell(0, 4).Character.Should().Be("╗");
    }

    [Fact]
    public void DrawBorder_DrawsRoundedBorder()
    {
        var buffer = new FrameBuffer(10, 10);

        buffer.DrawBorder(0, 0, 5, 3, BorderStyle.Rounded, RGBA.White);

        buffer.GetCell(0, 0).Character.Should().Be("╭");
        buffer.GetCell(0, 4).Character.Should().Be("╮");
    }

    [Fact]
    public void Clear_ResetsAllCells()
    {
        var buffer = new FrameBuffer(10, 10);
        buffer.SetCell(0, 0, new Cell("X", RGBA.Red, RGBA.Blue));

        buffer.Clear();

        buffer.GetCell(0, 0).Character.Should().Be(" ");
        buffer.GetCell(0, 0).Background.Should().Be(RGBA.Transparent);
    }

    [Fact]
    public void Clear_WithBackground_SetsBackground()
    {
        var buffer = new FrameBuffer(10, 10);

        buffer.Clear(RGBA.Blue);

        buffer.GetCell(0, 0).Background.Should().Be(RGBA.Blue);
        buffer.GetCell(5, 5).Background.Should().Be(RGBA.Blue);
    }

    [Fact]
    public void DrawFrameBuffer_CopiesContent()
    {
        var source = new FrameBuffer(5, 5);
        var dest = new FrameBuffer(10, 10);

        source.SetCell(0, 0, new Cell("A", RGBA.Red));
        source.SetCell(1, 1, new Cell("B", RGBA.Blue));

        dest.DrawFrameBuffer(source, 2, 3);

        dest.GetCell(3, 2).Character.Should().Be("A");
        dest.GetCell(4, 3).Character.Should().Be("B");
    }

    [Fact]
    public void SetCellWithAlphaBlending_BlendsColors()
    {
        var buffer = new FrameBuffer(10, 10);
        buffer.SetCell(0, 0, new Cell(" ", RGBA.White, RGBA.Blue));

        var semiTransparent = new Cell(" ", RGBA.White, RGBA.Red.WithAlpha(0.5f));
        buffer.SetCellWithAlphaBlending(0, 0, semiTransparent);

        // Result should be a blend of red over blue
        var result = buffer.GetCell(0, 0).Background;
        result.R.Should().BeGreaterThan(0);
        result.B.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ToAnsiString_GeneratesOutput()
    {
        var buffer = new FrameBuffer(5, 2);
        buffer.DrawText("Hi", 0, 0, RGBA.Red);

        var output = buffer.ToAnsiString();

        output.Should().Contain("Hi");
        output.Should().Contain("\x1b["); // Contains escape sequences
    }

    [Fact]
    public void ToDifferentialAnsiString_OnlyOutputsDirtyCells()
    {
        var buffer = new FrameBuffer(10, 10);
        buffer.DrawText("Hello", 0, 0);
        buffer.ClearDirty();

        buffer.SetCell(5, 5, new Cell("X", RGBA.Red));

        var output = buffer.ToDifferentialAnsiString();

        output.Should().Contain("X");
        output.Should().NotContain("Hello");
    }

    [Fact]
    public void Indexer_ProvidesDirectAccess()
    {
        var buffer = new FrameBuffer(10, 10);

        buffer[3, 5] = new Cell("Z", RGBA.Green);

        buffer[3, 5].Character.Should().Be("Z");
    }
}

public class CellTests
{
    [Fact]
    public void DefaultConstructor_CreatesSpaceCell()
    {
        var cell = new Cell();

        cell.Character.Should().Be(" ");
        cell.Foreground.Should().Be(RGBA.White);
        cell.Background.Should().Be(RGBA.Transparent);
        cell.Attributes.Should().Be(TextAttributes.None);
        cell.Width.Should().Be(1);
    }

    [Fact]
    public void Constructor_SetsProperties()
    {
        var cell = new Cell("X", RGBA.Red, RGBA.Blue, TextAttributes.Bold);

        cell.Character.Should().Be("X");
        cell.Foreground.Should().Be(RGBA.Red);
        cell.Background.Should().Be(RGBA.Blue);
        cell.Attributes.Should().Be(TextAttributes.Bold);
    }

    [Fact]
    public void Clear_ResetsToDefaults()
    {
        var cell = new Cell("X", RGBA.Red, RGBA.Blue, TextAttributes.Bold);

        cell.Clear();

        cell.Character.Should().Be(" ");
        cell.Foreground.Should().Be(RGBA.White);
        cell.Background.Should().Be(RGBA.Transparent);
        cell.Attributes.Should().Be(TextAttributes.None);
    }

    [Fact]
    public void Clear_WithBackground_SetsBackground()
    {
        var cell = new Cell("X", RGBA.Red, RGBA.Blue);

        cell.Clear(RGBA.Green);

        cell.Background.Should().Be(RGBA.Green);
    }

    [Fact]
    public void WidePlaceholder_HasZeroWidth()
    {
        var placeholder = Cell.WidePlaceholder;

        placeholder.Width.Should().Be(0);
        placeholder.Character.Should().BeEmpty();
    }

    [Fact]
    public void Equality_WorksCorrectly()
    {
        var a = new Cell("X", RGBA.Red, RGBA.Blue, TextAttributes.Bold);
        var b = new Cell("X", RGBA.Red, RGBA.Blue, TextAttributes.Bold);
        var c = new Cell("Y", RGBA.Red, RGBA.Blue, TextAttributes.Bold);

        (a == b).Should().BeTrue();
        (a != c).Should().BeTrue();
        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}
