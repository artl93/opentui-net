using OpenTUI.Core.Colors;
using OpenTUI.Core.Rendering;

namespace OpenTUI.Core.Renderables;

/// <summary>
/// A slider for selecting numeric values.
/// </summary>
public class SliderRenderable : Renderable
{
    private float _value;
    private float _min;
    private float _max = 100;
    private float _step = 1;
    private bool _showValue = true;

    /// <summary>Current value.</summary>
    public float Value
    {
        get => _value;
        set
        {
            var newValue = Math.Clamp(value, _min, _max);
            // Snap to step
            newValue = MathF.Round(newValue / _step) * _step;
            newValue = Math.Clamp(newValue, _min, _max);
            
            if (Math.Abs(_value - newValue) > float.Epsilon)
            {
                _value = newValue;
                MarkDirty();
                ValueChanged?.Invoke(this, _value);
            }
        }
    }

    /// <summary>Minimum value.</summary>
    public float Min
    {
        get => _min;
        set
        {
            if (Math.Abs(_min - value) > float.Epsilon)
            {
                _min = value;
                _value = Math.Clamp(_value, _min, _max);
                MarkDirty();
            }
        }
    }

    /// <summary>Maximum value.</summary>
    public float Max
    {
        get => _max;
        set
        {
            if (Math.Abs(_max - value) > float.Epsilon)
            {
                _max = value;
                _value = Math.Clamp(_value, _min, _max);
                MarkDirty();
            }
        }
    }

    /// <summary>Step increment.</summary>
    public float Step
    {
        get => _step;
        set
        {
            if (value > 0 && Math.Abs(_step - value) > float.Epsilon)
            {
                _step = value;
                MarkDirty();
            }
        }
    }

    /// <summary>Whether to show the current value.</summary>
    public bool ShowValue
    {
        get => _showValue;
        set
        {
            if (_showValue != value)
            {
                _showValue = value;
                MarkDirty();
            }
        }
    }

    /// <summary>Format string for displaying the value.</summary>
    public string ValueFormat { get; set; } = "F0";

    /// <summary>Track color.</summary>
    public RGBA TrackColor { get; set; } = RGBA.FromValues(0.3f, 0.3f, 0.3f);

    /// <summary>Filled track color.</summary>
    public RGBA FilledColor { get; set; } = RGBA.FromValues(0.2f, 0.6f, 1f);

    /// <summary>Thumb character.</summary>
    public string ThumbChar { get; set; } = "●";

    /// <summary>Track character.</summary>
    public string TrackChar { get; set; } = "─";

    /// <summary>Filled track character.</summary>
    public string FilledChar { get; set; } = "━";

    /// <summary>Event raised when value changes.</summary>
    public event EventHandler<float>? ValueChanged;

    public SliderRenderable()
    {
        Focusable = true;
    }

    /// <summary>Increases value by one step.</summary>
    public void Increment()
    {
        Value += _step;
    }

    /// <summary>Decreases value by one step.</summary>
    public void Decrement()
    {
        Value -= _step;
    }

    /// <summary>Sets value to minimum.</summary>
    public void SetToMin()
    {
        Value = _min;
    }

    /// <summary>Sets value to maximum.</summary>
    public void SetToMax()
    {
        Value = _max;
    }

    /// <summary>Gets the normalized value (0-1).</summary>
    public float NormalizedValue => _max > _min ? (_value - _min) / (_max - _min) : 0;

    protected override void RenderSelf(FrameBuffer buffer, int x, int y, int width, int height)
    {
        if (width <= 0 || height <= 0) return;

        var fg = ForegroundColor ?? RGBA.White;
        var bg = BackgroundColor ?? buffer.GetCell(y, x).Background;

        // Calculate value label width
        var valueLabel = _showValue ? _value.ToString(ValueFormat) : string.Empty;
        var labelWidth = _showValue ? valueLabel.Length + 1 : 0;
        var trackWidth = width - labelWidth;

        if (trackWidth <= 0) return;

        // Calculate thumb position
        var normalized = NormalizedValue;
        var thumbPos = (int)Math.Round(normalized * (trackWidth - 1));

        // Draw track
        for (int i = 0; i < trackWidth; i++)
        {
            string ch;
            RGBA color;

            if (i == thumbPos)
            {
                ch = ThumbChar;
                color = IsFocused ? FilledColor : fg;
            }
            else if (i < thumbPos)
            {
                ch = FilledChar;
                color = FilledColor;
            }
            else
            {
                ch = TrackChar;
                color = TrackColor;
            }

            buffer.SetCell(x + i, y, new Cell(ch, color, bg));
        }

        // Draw value label
        if (_showValue && labelWidth > 0)
        {
            buffer.SetCell(x + trackWidth, y, new Cell(" ", fg, bg));
            for (int i = 0; i < valueLabel.Length; i++)
            {
                buffer.SetCell(x + trackWidth + 1 + i, y, new Cell(valueLabel[i].ToString(), fg, bg));
            }
        }
    }
}
