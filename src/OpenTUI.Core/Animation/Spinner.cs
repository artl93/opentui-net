namespace OpenTUI.Core.Animation;

/// <summary>
/// Predefined spinner styles with character sequences.
/// </summary>
public static class SpinnerStyles
{
    /// <summary>Dots spinner: ⠋⠙⠹⠸⠼⠴⠦⠧⠇⠏</summary>
    public static readonly string[] Dots = { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };

    /// <summary>Line spinner: -\|/</summary>
    public static readonly string[] Line = { "-", "\\", "|", "/" };

    /// <summary>Arrow spinner: ←↖↑↗→↘↓↙</summary>
    public static readonly string[] Arrow = { "←", "↖", "↑", "↗", "→", "↘", "↓", "↙" };

    /// <summary>Block spinner: ▖▘▝▗</summary>
    public static readonly string[] Block = { "▖", "▘", "▝", "▗" };

    /// <summary>Circle spinner: ◐◓◑◒</summary>
    public static readonly string[] Circle = { "◐", "◓", "◑", "◒" };

    /// <summary>Square corners: ◰◳◲◱</summary>
    public static readonly string[] Square = { "◰", "◳", "◲", "◱" };

    /// <summary>Growing dots: .oO@*</summary>
    public static readonly string[] GrowingDots = { ".", "o", "O", "@", "*" };

    /// <summary>Clock hands: 🕐🕑🕒🕓🕔🕕🕖🕗🕘🕙🕚🕛</summary>
    public static readonly string[] Clock = { "🕐", "🕑", "🕒", "🕓", "🕔", "🕕", "🕖", "🕗", "🕘", "🕙", "🕚", "🕛" };

    /// <summary>Moon phases: 🌑🌒🌓🌔🌕🌖🌗🌘</summary>
    public static readonly string[] Moon = { "🌑", "🌒", "🌓", "🌔", "🌕", "🌖", "🌗", "🌘" };

    /// <summary>Bouncing ball: [    ●    ]</summary>
    public static readonly string[] BouncingBall =
    {
        "[●         ]",
        "[ ●        ]",
        "[  ●       ]",
        "[   ●      ]",
        "[    ●     ]",
        "[     ●    ]",
        "[      ●   ]",
        "[       ●  ]",
        "[        ● ]",
        "[         ●]",
        "[        ● ]",
        "[       ●  ]",
        "[      ●   ]",
        "[     ●    ]",
        "[    ●     ]",
        "[   ●      ]",
        "[  ●       ]",
        "[ ●        ]"
    };
}

/// <summary>
/// A simple animated spinner.
/// </summary>
public class Spinner
{
    private readonly string[] _frames;
    private int _frameIndex;
    private DateTime _lastUpdate;
    private readonly TimeSpan _frameDelay;

    /// <summary>Current frame character.</summary>
    public string CurrentFrame => _frames[_frameIndex];

    /// <summary>Optional label displayed after the spinner.</summary>
    public string? Label { get; set; }

    /// <summary>
    /// Creates a spinner with the specified style.
    /// </summary>
    /// <param name="frames">Array of spinner frame characters.</param>
    /// <param name="framesPerSecond">Animation speed in frames per second.</param>
    public Spinner(string[] frames, int framesPerSecond = 10)
    {
        _frames = frames;
        _frameDelay = TimeSpan.FromMilliseconds(1000.0 / framesPerSecond);
        _lastUpdate = DateTime.Now;
    }

    /// <summary>
    /// Creates a spinner with predefined dots style.
    /// </summary>
    public static Spinner Dots(string? label = null) => new(SpinnerStyles.Dots) { Label = label };

    /// <summary>
    /// Creates a spinner with predefined line style.
    /// </summary>
    public static Spinner Line(string? label = null) => new(SpinnerStyles.Line) { Label = label };

    /// <summary>
    /// Creates a spinner with predefined arrow style.
    /// </summary>
    public static Spinner Arrow(string? label = null) => new(SpinnerStyles.Arrow) { Label = label };

    /// <summary>
    /// Creates a spinner with predefined bouncing ball style.
    /// </summary>
    public static Spinner BouncingBall() => new(SpinnerStyles.BouncingBall, 15);

    /// <summary>
    /// Updates the spinner animation. Call this each frame.
    /// </summary>
    /// <returns>True if the frame changed.</returns>
    public bool Update()
    {
        var now = DateTime.Now;
        if (now - _lastUpdate >= _frameDelay)
        {
            _frameIndex = (_frameIndex + 1) % _frames.Length;
            _lastUpdate = now;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets the display string including label if set.
    /// </summary>
    public override string ToString() =>
        string.IsNullOrEmpty(Label) ? CurrentFrame : $"{CurrentFrame} {Label}";
}
