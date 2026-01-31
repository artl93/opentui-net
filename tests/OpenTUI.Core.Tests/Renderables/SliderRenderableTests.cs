using FluentAssertions;
using OpenTUI.Core.Layout;
using OpenTUI.Core.Renderables;

namespace OpenTUI.Core.Tests.Renderables;

public class SliderRenderableTests
{
    [Fact]
    public void Constructor_Default_HasCorrectDefaults()
    {
        var slider = new SliderRenderable();
        
        slider.Focusable.Should().BeTrue();
        slider.Value.Should().Be(0);
        slider.Min.Should().Be(0);
        slider.Max.Should().Be(100);
        slider.Step.Should().Be(1);
    }

    [Fact]
    public void Value_ClampedToRange()
    {
        var slider = new SliderRenderable
        {
            Min = 0,
            Max = 100
        };
        
        slider.Value = 150;
        slider.Value.Should().Be(100);
        
        slider.Value = -50;
        slider.Value.Should().Be(0);
    }

    [Fact]
    public void Value_SnapsToStep()
    {
        var slider = new SliderRenderable
        {
            Min = 0,
            Max = 100,
            Step = 10
        };
        
        slider.Value = 17;
        slider.Value.Should().Be(20);
        
        slider.Value = 12;
        slider.Value.Should().Be(10);
    }

    [Fact]
    public void Increment_IncreasesValue()
    {
        var slider = new SliderRenderable
        {
            Value = 50,
            Step = 5
        };
        
        slider.Increment();
        
        slider.Value.Should().Be(55);
    }

    [Fact]
    public void Decrement_DecreasesValue()
    {
        var slider = new SliderRenderable
        {
            Value = 50,
            Step = 5
        };
        
        slider.Decrement();
        
        slider.Value.Should().Be(45);
    }

    [Fact]
    public void SetToMin_SetsMinimumValue()
    {
        var slider = new SliderRenderable
        {
            Min = 10,
            Max = 100,
            Value = 50
        };
        
        slider.SetToMin();
        
        slider.Value.Should().Be(10);
    }

    [Fact]
    public void SetToMax_SetsMaximumValue()
    {
        var slider = new SliderRenderable
        {
            Min = 0,
            Max = 100,
            Value = 50
        };
        
        slider.SetToMax();
        
        slider.Value.Should().Be(100);
    }

    [Fact]
    public void NormalizedValue_ReturnsCorrectRatio()
    {
        var slider = new SliderRenderable
        {
            Min = 0,
            Max = 100,
            Value = 50
        };
        
        slider.NormalizedValue.Should().BeApproximately(0.5f, 0.001f);
    }

    [Fact]
    public void NormalizedValue_WithOffset_ReturnsCorrectRatio()
    {
        var slider = new SliderRenderable
        {
            Min = 50,
            Max = 150,
            Value = 100
        };
        
        slider.NormalizedValue.Should().BeApproximately(0.5f, 0.001f);
    }

    [Fact]
    public void ValueChanged_RaisedOnChange()
    {
        var slider = new SliderRenderable();
        float? changed = null;
        slider.ValueChanged += (_, v) => changed = v;
        
        slider.Value = 42;
        
        changed.Should().Be(42);
    }

    [Fact]
    public void ValueChanged_NotRaisedWhenSameValue()
    {
        var slider = new SliderRenderable { Value = 50 };
        int callCount = 0;
        slider.ValueChanged += (_, _) => callCount++;
        
        slider.Value = 50;
        
        callCount.Should().Be(0);
    }

    [Fact]
    public void Render_DrawsSliderTrack()
    {
        var renderer = CliRenderer.CreateForTesting(20, 5);
        var slider = new SliderRenderable
        {
            Value = 50,
            ShowValue = false
        };
        slider.Layout.Width = 11;
        slider.Layout.Height = 1;
        slider.Layout.AlignSelf = AlignSelf.FlexStart;
        
        renderer.Root.Add(slider);
        renderer.Render();
        
        var buffer = renderer.GetBuffer();
        // At 50%, thumb should be roughly in the middle
        buffer.GetCell(0, 5).Character.Should().Be("●");
    }

    [Fact]
    public void Render_ShowsValue()
    {
        var renderer = CliRenderer.CreateForTesting(20, 5);
        var slider = new SliderRenderable
        {
            Value = 42,
            ShowValue = true,
            ValueFormat = "F0"
        };
        slider.Layout.Width = 15;
        slider.Layout.Height = 1;
        slider.Layout.AlignSelf = AlignSelf.FlexStart;
        
        renderer.Root.Add(slider);
        renderer.Render();
        
        var buffer = renderer.GetBuffer();
        // Value "42" should appear after the track
        // Find "4" in the buffer
        bool found42 = false;
        for (int col = 0; col < 15; col++)
        {
            if (buffer.GetCell(0, col).Character == "4" && 
                col + 1 < 15 && buffer.GetCell(0, col + 1).Character == "2")
            {
                found42 = true;
                break;
            }
        }
        found42.Should().BeTrue();
    }
}
