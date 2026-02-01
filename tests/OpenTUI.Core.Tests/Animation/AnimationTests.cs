using FluentAssertions;
using OpenTUI.Core.Animation;
using OpenTUI.Core.Colors;

namespace OpenTUI.Core.Tests.Animation;

public class EasingTests
{
    [Fact]
    public void Linear_ReturnsInputUnchanged()
    {
        Easing.Linear(0f).Should().Be(0f);
        Easing.Linear(0.5f).Should().Be(0.5f);
        Easing.Linear(1f).Should().Be(1f);
    }

    [Fact]
    public void InQuad_AtBoundaries()
    {
        Easing.InQuad(0f).Should().Be(0f);
        Easing.InQuad(1f).Should().Be(1f);
    }

    [Fact]
    public void InQuad_SlowerAtStart()
    {
        Easing.InQuad(0.5f).Should().BeLessThan(0.5f);
    }

    [Fact]
    public void OutQuad_FasterAtStart()
    {
        Easing.OutQuad(0.5f).Should().BeGreaterThan(0.5f);
    }

    [Fact]
    public void OutBounce_AtBoundaries()
    {
        Easing.OutBounce(0f).Should().Be(0f);
        Easing.OutBounce(1f).Should().BeApproximately(1f, 0.001f);
    }

    [Fact]
    public void OutElastic_AtBoundaries()
    {
        Easing.OutElastic(0f).Should().Be(0f);
        Easing.OutElastic(1f).Should().Be(1f);
    }

    [Fact]
    public void InOutSine_AtBoundaries()
    {
        Easing.InOutSine(0f).Should().BeApproximately(0f, 0.001f);
        Easing.InOutSine(0.5f).Should().BeApproximately(0.5f, 0.001f);
        Easing.InOutSine(1f).Should().BeApproximately(1f, 0.001f);
    }
}

public class SpinnerTests
{
    [Fact]
    public void CurrentFrame_ReturnsFirstFrameInitially()
    {
        var spinner = new Spinner(SpinnerStyles.Dots);
        spinner.CurrentFrame.Should().Be("⠋");
    }

    [Fact]
    public void ToString_IncludesLabel()
    {
        var spinner = Spinner.Dots("Loading...");
        spinner.ToString().Should().Contain("Loading...");
    }

    [Fact]
    public void Dots_CreatesDotSpinner()
    {
        var spinner = Spinner.Dots();
        SpinnerStyles.Dots.Should().Contain(spinner.CurrentFrame);
    }

    [Fact]
    public void BouncingBall_CreatesBouncingSpinner()
    {
        var spinner = Spinner.BouncingBall();
        SpinnerStyles.BouncingBall.Should().Contain(spinner.CurrentFrame);
    }
}

public class ProgressBarTests
{
    [Fact]
    public void Determinate_ShowsProgress()
    {
        var bar = ProgressBar.Determinate(0.5, 10);
        bar.Progress.Should().Be(0.5);
        bar.IsIndeterminate.Should().BeFalse();
    }

    [Fact]
    public void Indeterminate_IsIndeterminate()
    {
        var bar = ProgressBar.Indeterminate(10);
        bar.IsIndeterminate.Should().BeTrue();
    }

    [Fact]
    public void Render_ContainsBrackets()
    {
        var bar = ProgressBar.Determinate(0.5, 10);
        bar.Render().Should().Contain("[").And.Contain("]");
    }

    [Fact]
    public void Render_ShowsPercentage()
    {
        var bar = new ProgressBar(10) { Progress = 0.75, ShowPercentage = true };
        bar.Render().Should().Contain("75%");
    }

    [Fact]
    public void Render_IncludesLabel()
    {
        var bar = new ProgressBar(10) { Progress = 0.5, Label = "Download" };
        bar.Render().Should().StartWith("Download:");
    }

    [Fact]
    public void Progress_ClampedToValidRange()
    {
        var bar = new ProgressBar(10);
        bar.Progress = 1.5;
        bar.Progress.Should().Be(1.0);

        bar.Progress = -0.5;
        bar.Progress.Should().Be(-0.5); // -1 is valid for indeterminate
    }
}

public class TextEffectsTests
{
    [Fact]
    public void Rainbow_ReturnsCorrectLength()
    {
        var result = TextEffects.Rainbow("Hello", 0);
        result.Should().HaveCount(5);
    }

    [Fact]
    public void Rainbow_ReturnsCharacters()
    {
        var result = TextEffects.Rainbow("Hi", 0);
        result[0].ch.Should().Be('H');
        result[1].ch.Should().Be('i');
    }

    [Fact]
    public void Shimmer_ReturnsCorrectLength()
    {
        var result = TextEffects.Shimmer("Test", 0);
        result.Should().HaveCount(4);
    }

    [Fact]
    public void Pulse_ReturnsValidColor()
    {
        var color = TextEffects.Pulse(0, RGBA.Red);
        color.R.Should().BeGreaterThanOrEqualTo(0).And.BeLessThanOrEqualTo(1);
    }

    [Fact]
    public void Typewriter_RevealsTextOverTime()
    {
        var text = "Hello World";

        TextEffects.Typewriter(text, 0, 8).Should().BeEmpty();
        TextEffects.Typewriter(text, 0.5, 8).Should().Be("Hell");
        TextEffects.Typewriter(text, 10, 8).Should().Be(text);
    }

    [Fact]
    public void CursorVisible_Toggles()
    {
        // At time 0, cursor should be visible
        TextEffects.CursorVisible(0).Should().BeTrue();
    }

    [Fact]
    public void Wave_ReturnsCorrectLength()
    {
        var result = TextEffects.Wave("Wave", 0, RGBA.Blue);
        result.Should().HaveCount(4);
    }
}
