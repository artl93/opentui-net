using OpenTUI.Core.Colors;

namespace OpenTUI.Core.Animation;

/// <summary>
/// A progress bar that can be determinate or indeterminate.
/// </summary>
public class ProgressBar
{
    private double _progress;
    private int _indeterminateOffset;
    private DateTime _lastUpdate;
    private readonly TimeSpan _frameDelay;

    /// <summary>Width of the progress bar in characters.</summary>
    public int Width { get; set; } = 20;

    /// <summary>Progress value from 0.0 to 1.0. Set to -1 for indeterminate mode.</summary>
    public double Progress
    {
        get => _progress;
        set => _progress = Math.Clamp(value, -1, 1);
    }

    /// <summary>Whether the progress bar is in indeterminate mode.</summary>
    public bool IsIndeterminate => _progress < 0;

    /// <summary>Character used for filled portion.</summary>
    public string FilledChar { get; set; } = "█";

    /// <summary>Character used for empty portion.</summary>
    public string EmptyChar { get; set; } = "░";

    /// <summary>Character used for the leading edge in indeterminate mode.</summary>
    public string LeadChar { get; set; } = "▓";

    /// <summary>Color for filled portion.</summary>
    public RGBA FilledColor { get; set; } = RGBA.Green;

    /// <summary>Color for empty portion.</summary>
    public RGBA EmptyColor { get; set; } = RGBA.FromInts(80, 80, 80);

    /// <summary>Whether to show percentage text.</summary>
    public bool ShowPercentage { get; set; } = true;

    /// <summary>Optional label displayed before the bar.</summary>
    public string? Label { get; set; }

    /// <summary>Width of the indeterminate highlight section.</summary>
    public int IndeterminateWidth { get; set; } = 6;

    /// <summary>
    /// Creates a new progress bar.
    /// </summary>
    /// <param name="width">Width in characters.</param>
    public ProgressBar(int width = 20)
    {
        Width = width;
        _frameDelay = TimeSpan.FromMilliseconds(50);
        _lastUpdate = DateTime.Now;
    }

    /// <summary>
    /// Creates a determinate progress bar.
    /// </summary>
    public static ProgressBar Determinate(double progress = 0, int width = 20) => 
        new(width) { Progress = progress };

    /// <summary>
    /// Creates an indeterminate progress bar.
    /// </summary>
    public static ProgressBar Indeterminate(int width = 20) => 
        new(width) { Progress = -1 };

    /// <summary>
    /// Updates the animation for indeterminate mode.
    /// </summary>
    public void Update()
    {
        if (!IsIndeterminate) return;
        
        var now = DateTime.Now;
        if (now - _lastUpdate >= _frameDelay)
        {
            _indeterminateOffset = (_indeterminateOffset + 1) % (Width + IndeterminateWidth);
            _lastUpdate = now;
        }
    }

    /// <summary>
    /// Renders the progress bar to a string.
    /// </summary>
    public string Render()
    {
        var bar = new System.Text.StringBuilder();
        bar.Append('[');

        if (IsIndeterminate)
        {
            for (int i = 0; i < Width; i++)
            {
                var pos = _indeterminateOffset - IndeterminateWidth;
                if (i >= pos && i < pos + IndeterminateWidth)
                    bar.Append(FilledChar);
                else
                    bar.Append(EmptyChar);
            }
        }
        else
        {
            var filled = (int)(Progress * Width);
            for (int i = 0; i < Width; i++)
            {
                bar.Append(i < filled ? FilledChar : EmptyChar);
            }
        }

        bar.Append(']');

        if (ShowPercentage && !IsIndeterminate)
        {
            bar.Append($" {Progress * 100:F0}%");
        }

        if (!string.IsNullOrEmpty(Label))
        {
            return $"{Label}: {bar}";
        }

        return bar.ToString();
    }

    /// <summary>
    /// Gets the bar portion only (without label or percentage).
    /// </summary>
    public (string bar, int filledCount) GetBarParts()
    {
        var bar = new System.Text.StringBuilder();
        int filledCount = 0;

        if (IsIndeterminate)
        {
            for (int i = 0; i < Width; i++)
            {
                var pos = _indeterminateOffset - IndeterminateWidth;
                if (i >= pos && i < pos + IndeterminateWidth)
                {
                    bar.Append(FilledChar);
                    filledCount++;
                }
                else
                {
                    bar.Append(EmptyChar);
                }
            }
        }
        else
        {
            filledCount = (int)(Progress * Width);
            for (int i = 0; i < Width; i++)
            {
                bar.Append(i < filledCount ? FilledChar : EmptyChar);
            }
        }

        return (bar.ToString(), filledCount);
    }

    public override string ToString() => Render();
}
