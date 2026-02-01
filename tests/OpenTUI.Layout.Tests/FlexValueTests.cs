using FluentAssertions;
using OpenTUI.Core.Layout;

namespace OpenTUI.Layout.Tests;

public class FlexValueTests
{
    [Fact]
    public void Points_CreatesPointValue()
    {
        var value = FlexValue.Points(100);

        value.Value.Should().Be(100);
        value.Unit.Should().Be(FlexUnit.Point);
        value.IsPoint.Should().BeTrue();
        value.IsDefined.Should().BeTrue();
    }

    [Fact]
    public void Percent_CreatesPercentValue()
    {
        var value = FlexValue.Percent(50);

        value.Value.Should().Be(50);
        value.Unit.Should().Be(FlexUnit.Percent);
        value.IsPercent.Should().BeTrue();
    }

    [Fact]
    public void Auto_IsAutoValue()
    {
        var value = FlexValue.Auto;

        value.IsAuto.Should().BeTrue();
        // Auto values have a defined Unit (FlexUnit.Auto) but resolve to NaN
        value.Unit.Should().Be(FlexUnit.Auto);
    }

    [Fact]
    public void Undefined_IsNotDefined()
    {
        var value = FlexValue.Undefined;

        value.IsDefined.Should().BeFalse();
    }

    [Fact]
    public void Resolve_PointReturnsValue()
    {
        var value = FlexValue.Points(100);

        value.Resolve(500).Should().Be(100);
    }

    [Fact]
    public void Resolve_PercentReturnsPercentageOfParent()
    {
        var value = FlexValue.Percent(50);

        value.Resolve(200).Should().Be(100);
    }

    [Fact]
    public void ResolveOrDefault_ReturnsDefaultForAuto()
    {
        var value = FlexValue.Auto;

        value.ResolveOrDefault(100, 50).Should().Be(50);
    }

    [Fact]
    public void ResolveOrDefault_ReturnsValueForDefined()
    {
        var value = FlexValue.Points(75);

        value.ResolveOrDefault(100, 50).Should().Be(75);
    }

    [Fact]
    public void ImplicitConversion_FloatToPoints()
    {
        FlexValue value = 100f;

        value.Should().Be(FlexValue.Points(100));
    }

    [Fact]
    public void ImplicitConversion_IntToPoints()
    {
        FlexValue value = 100;

        value.Should().Be(FlexValue.Points(100));
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        FlexValue.Points(100).ToString().Should().Be("100");
        FlexValue.Percent(50).ToString().Should().Be("50%");
        FlexValue.Auto.ToString().Should().Be("auto");
        FlexValue.Undefined.ToString().Should().Be("undefined");
    }

    [Fact]
    public void Equality_WorksCorrectly()
    {
        var a = FlexValue.Points(100);
        var b = FlexValue.Points(100);
        var c = FlexValue.Points(200);

        (a == b).Should().BeTrue();
        (a != c).Should().BeTrue();
        a.Equals(b).Should().BeTrue();
    }
}

public class EdgesTests
{
    [Fact]
    public void Constructor_SingleValue_AppliesToAllSides()
    {
        var edges = new Edges(FlexValue.Points(10));

        edges.Top.Should().Be(FlexValue.Points(10));
        edges.Right.Should().Be(FlexValue.Points(10));
        edges.Bottom.Should().Be(FlexValue.Points(10));
        edges.Left.Should().Be(FlexValue.Points(10));
    }

    [Fact]
    public void Constructor_TwoValues_AppliesVerticalHorizontal()
    {
        var edges = new Edges(FlexValue.Points(10), FlexValue.Points(20));

        edges.Top.Should().Be(FlexValue.Points(10));
        edges.Bottom.Should().Be(FlexValue.Points(10));
        edges.Left.Should().Be(FlexValue.Points(20));
        edges.Right.Should().Be(FlexValue.Points(20));
    }

    [Fact]
    public void Constructor_FourValues_AppliesIndividually()
    {
        var edges = new Edges(
            FlexValue.Points(1),
            FlexValue.Points(2),
            FlexValue.Points(3),
            FlexValue.Points(4)
        );

        edges.Top.Should().Be(FlexValue.Points(1));
        edges.Right.Should().Be(FlexValue.Points(2));
        edges.Bottom.Should().Be(FlexValue.Points(3));
        edges.Left.Should().Be(FlexValue.Points(4));
    }

    [Fact]
    public void GetHorizontal_SumsLeftAndRight()
    {
        var edges = new Edges(
            FlexValue.Points(10),
            FlexValue.Points(20),
            FlexValue.Points(10),
            FlexValue.Points(30)
        );

        edges.GetHorizontal(100).Should().Be(50); // 20 + 30
    }

    [Fact]
    public void GetVertical_SumsTopAndBottom()
    {
        var edges = new Edges(
            FlexValue.Points(10),
            FlexValue.Points(20),
            FlexValue.Points(15),
            FlexValue.Points(30)
        );

        edges.GetVertical(100).Should().Be(25); // 10 + 15
    }

    [Fact]
    public void Zero_HasAllZeroValues()
    {
        var edges = Edges.Zero;

        edges.GetHorizontal(100).Should().Be(0);
        edges.GetVertical(100).Should().Be(0);
    }
}
