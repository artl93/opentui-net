using FluentAssertions;
using OpenTUI.Core.Colors;

namespace OpenTUI.Core.Tests.Colors;

public class RGBATests
{
    [Fact]
    public void FromValues_ClampsToValidRange()
    {
        var color = RGBA.FromValues(1.5f, -0.5f, 0.5f, 2f);

        color.R.Should().Be(1f);
        color.G.Should().Be(0f);
        color.B.Should().Be(0.5f);
        color.A.Should().Be(1f);
    }

    [Fact]
    public void FromInts_ConvertsCorrectly()
    {
        var color = RGBA.FromInts(255, 128, 0, 255);

        color.R.Should().Be(1f);
        color.G.Should().BeApproximately(128f / 255f, 0.001f);
        color.B.Should().Be(0f);
        color.A.Should().Be(1f);
    }

    [Theory]
    [InlineData("#FF0000", 1f, 0f, 0f)]
    [InlineData("#00FF00", 0f, 1f, 0f)]
    [InlineData("#0000FF", 0f, 0f, 1f)]
    [InlineData("#FFFFFF", 1f, 1f, 1f)]
    [InlineData("#000000", 0f, 0f, 0f)]
    [InlineData("FF0000", 1f, 0f, 0f)]
    public void FromHex_ParsesLongFormat(string hex, float r, float g, float b)
    {
        var color = RGBA.FromHex(hex);

        color.R.Should().Be(r);
        color.G.Should().Be(g);
        color.B.Should().Be(b);
        color.A.Should().Be(1f);
    }

    [Theory]
    [InlineData("#F00", 1f, 0f, 0f)]
    [InlineData("#0F0", 0f, 1f, 0f)]
    [InlineData("#00F", 0f, 0f, 1f)]
    [InlineData("#FFF", 1f, 1f, 1f)]
    public void FromHex_ParsesShortFormat(string hex, float r, float g, float b)
    {
        var color = RGBA.FromHex(hex);

        color.R.Should().Be(r);
        color.G.Should().Be(g);
        color.B.Should().Be(b);
    }

    [Theory]
    [InlineData("#FF000080", 1f, 0f, 0f, 128f / 255f)]
    [InlineData("#FFFFFFFF", 1f, 1f, 1f, 1f)]
    [InlineData("#00000000", 0f, 0f, 0f, 0f)]
    public void FromHex_ParsesAlphaFormat(string hex, float r, float g, float b, float a)
    {
        var color = RGBA.FromHex(hex);

        color.R.Should().Be(r);
        color.G.Should().Be(g);
        color.B.Should().Be(b);
        color.A.Should().BeApproximately(a, 0.01f);
    }

    [Theory]
    [InlineData("#F008", 1f, 0f, 0f, 0.533f)]
    public void FromHex_ParsesShortAlphaFormat(string hex, float r, float g, float b, float a)
    {
        var color = RGBA.FromHex(hex);

        color.R.Should().Be(r);
        color.G.Should().Be(g);
        color.B.Should().Be(b);
        color.A.Should().BeApproximately(a, 0.01f);
    }

    [Theory]
    [InlineData("")]
    [InlineData("#")]
    [InlineData("#GGG")]
    [InlineData("#12345")]
    [InlineData("#1234567890")]
    public void FromHex_ThrowsOnInvalidFormat(string hex)
    {
        var act = () => RGBA.FromHex(hex);
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void TryFromHex_ReturnsFalseOnInvalid()
    {
        RGBA.TryFromHex("invalid", out var result).Should().BeFalse();
        result.Should().Be(default(RGBA));
    }

    [Fact]
    public void TryFromHex_ReturnsTrueOnValid()
    {
        RGBA.TryFromHex("#FF0000", out var result).Should().BeTrue();
        result.R.Should().Be(1f);
    }

    [Theory]
    [InlineData("red", 255, 0, 0)]
    [InlineData("green", 0, 128, 0)]
    [InlineData("blue", 0, 0, 255)]
    [InlineData("white", 255, 255, 255)]
    [InlineData("black", 0, 0, 0)]
    [InlineData("RED", 255, 0, 0)]
    [InlineData("  Red  ", 255, 0, 0)]
    public void Parse_HandlesCssColorNames(string name, int r, int g, int b)
    {
        var color = RGBA.Parse(name);
        var ints = color.ToInts();

        ints.R.Should().Be(r);
        ints.G.Should().Be(g);
        ints.B.Should().Be(b);
    }

    [Fact]
    public void Parse_HandlesTransparent()
    {
        var color = RGBA.Parse("transparent");

        color.A.Should().Be(0f);
    }

    [Fact]
    public void Parse_ThrowsOnUnknownColor()
    {
        var act = () => RGBA.Parse("notacolor");
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void BlendOver_FullyOpaqueForegroundReturnsItself()
    {
        var fg = RGBA.FromValues(1f, 0f, 0f, 1f);
        var bg = RGBA.FromValues(0f, 1f, 0f, 1f);

        var result = fg.BlendOver(bg);

        result.Should().Be(fg);
    }

    [Fact]
    public void BlendOver_FullyTransparentForegroundReturnsBackground()
    {
        var fg = RGBA.FromValues(1f, 0f, 0f, 0f);
        var bg = RGBA.FromValues(0f, 1f, 0f, 1f);

        var result = fg.BlendOver(bg);

        result.Should().Be(bg);
    }

    [Fact]
    public void BlendOver_SemiTransparentBlendsProperly()
    {
        var fg = RGBA.FromValues(1f, 0f, 0f, 0.5f);
        var bg = RGBA.FromValues(0f, 0f, 1f, 1f);

        var result = fg.BlendOver(bg);

        // Alpha compositing: outR = (fgR * fgA + bgR * bgA * (1 - fgA)) / outA
        // outA = 0.5 + 1 * 0.5 = 1.0
        // outR = (1 * 0.5 + 0 * 1 * 0.5) / 1.0 = 0.5
        // outB = (0 * 0.5 + 1 * 1 * 0.5) / 1.0 = 0.5
        result.R.Should().BeApproximately(0.5f, 0.01f);
        result.B.Should().BeApproximately(0.5f, 0.01f);
        result.A.Should().Be(1f);
    }

    [Fact]
    public void WithAlpha_ReturnsNewColorWithAlpha()
    {
        var color = RGBA.Red;
        var transparent = color.WithAlpha(0.5f);

        transparent.R.Should().Be(1f);
        transparent.A.Should().Be(0.5f);
        color.A.Should().Be(1f); // Original unchanged
    }

    [Fact]
    public void ToHex_ReturnsCorrectFormat()
    {
        var color = RGBA.FromInts(255, 128, 64);

        color.ToHex().Should().Be("#FF8040");
        color.ToHex(includeAlpha: true).Should().Be("#FF8040FF");
    }

    [Fact]
    public void ToInts_ReturnsCorrectValues()
    {
        var color = RGBA.FromValues(1f, 0.5f, 0f, 0.5f);
        var (r, g, b, a) = color.ToInts();

        r.Should().Be(255);
        g.Should().Be(127);
        b.Should().Be(0);
        a.Should().Be(127);
    }

    [Fact]
    public void Equality_WorksCorrectly()
    {
        var a = RGBA.FromHex("#FF0000");
        var b = RGBA.FromHex("#FF0000");
        var c = RGBA.FromHex("#00FF00");

        (a == b).Should().BeTrue();
        (a != c).Should().BeTrue();
        a.Equals(b).Should().BeTrue();
        a.Equals((object)b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void PredefinedColors_AreCorrect()
    {
        RGBA.Black.Should().Be(RGBA.FromValues(0, 0, 0, 1));
        RGBA.White.Should().Be(RGBA.FromValues(1, 1, 1, 1));
        RGBA.Red.Should().Be(RGBA.FromValues(1, 0, 0, 1));
        RGBA.Green.Should().Be(RGBA.FromValues(0, 1, 0, 1));
        RGBA.Blue.Should().Be(RGBA.FromValues(0, 0, 1, 1));
        RGBA.Transparent.A.Should().Be(0f);
    }

    [Fact]
    public void ToString_ReturnsHex()
    {
        RGBA.Red.ToString().Should().Be("#FF0000");
        RGBA.Red.WithAlpha(0.5f).ToString().Should().Contain("7F"); // Alpha in output
    }
}
