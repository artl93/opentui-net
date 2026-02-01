namespace OpenTUI.Core.Terminal;

/// <summary>
/// Detected terminal color support level.
/// </summary>
public enum ColorSupport
{
    /// <summary>No color support (monochrome).</summary>
    None = 0,

    /// <summary>Basic 8 colors (ANSI).</summary>
    Basic = 1,

    /// <summary>256 color palette.</summary>
    Palette256 = 2,

    /// <summary>24-bit true color (16 million colors).</summary>
    TrueColor = 3
}

/// <summary>
/// Provides information about terminal capabilities.
/// </summary>
public class TerminalCapabilities
{
    /// <summary>Color support level.</summary>
    public ColorSupport ColorSupport { get; init; } = ColorSupport.TrueColor;

    /// <summary>Whether the terminal supports Unicode.</summary>
    public bool SupportsUnicode { get; init; } = true;

    /// <summary>Whether the terminal supports the alternate screen buffer.</summary>
    public bool SupportsAlternateScreen { get; init; } = true;

    /// <summary>Whether the terminal supports mouse input.</summary>
    public bool SupportsMouse { get; init; } = true;

    /// <summary>Whether the terminal supports bracketed paste mode.</summary>
    public bool SupportsBracketedPaste { get; init; } = true;

    /// <summary>Whether the terminal is running in a CI/headless environment.</summary>
    public bool IsCI { get; init; }

    /// <summary>Terminal type from TERM environment variable.</summary>
    public string? TermType { get; init; }

    /// <summary>
    /// Detects terminal capabilities from the environment.
    /// </summary>
    public static TerminalCapabilities Detect()
    {
        var term = Environment.GetEnvironmentVariable("TERM") ?? "";
        var colorterm = Environment.GetEnvironmentVariable("COLORTERM") ?? "";
        var termProgram = Environment.GetEnvironmentVariable("TERM_PROGRAM") ?? "";
        var ciEnv = Environment.GetEnvironmentVariable("CI");
        var noColor = Environment.GetEnvironmentVariable("NO_COLOR");

        var colorSupport = DetectColorSupport(term, colorterm, termProgram, noColor);
        var supportsUnicode = DetectUnicodeSupport(term);
        var isCI = !string.IsNullOrEmpty(ciEnv) ||
                   !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")) ||
                   !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITLAB_CI")) ||
                   !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JENKINS_URL"));

        return new TerminalCapabilities
        {
            ColorSupport = colorSupport,
            SupportsUnicode = supportsUnicode,
            SupportsAlternateScreen = !term.Contains("dumb"),
            SupportsMouse = !term.Contains("dumb") && !isCI,
            SupportsBracketedPaste = !term.Contains("dumb"),
            IsCI = isCI,
            TermType = term
        };
    }

    private static ColorSupport DetectColorSupport(string term, string colorterm, string termProgram, string? noColor)
    {
        // NO_COLOR standard: https://no-color.org/
        if (!string.IsNullOrEmpty(noColor))
            return ColorSupport.None;

        // Dumb terminals have no color
        if (term == "dumb" || string.IsNullOrEmpty(term))
            return ColorSupport.None;

        // True color detection
        if (colorterm == "truecolor" || colorterm == "24bit")
            return ColorSupport.TrueColor;

        // Known true color terminals
        if (termProgram is "iTerm.app" or "Apple_Terminal" or "Hyper" or "vscode")
            return ColorSupport.TrueColor;

        // Modern terminals with true color
        if (term.Contains("256color") || term.Contains("24bit") || term.Contains("truecolor"))
            return ColorSupport.TrueColor;

        // xterm and variants typically support 256 colors
        if (term.StartsWith("xterm") || term.StartsWith("screen") || term.StartsWith("tmux"))
            return ColorSupport.Palette256;

        // Linux console
        if (term == "linux")
            return ColorSupport.Basic;

        // Default to basic if any term is set
        return ColorSupport.Basic;
    }

    private static bool DetectUnicodeSupport(string term)
    {
        // Check locale for UTF-8
        var lang = Environment.GetEnvironmentVariable("LANG") ?? "";
        var lcAll = Environment.GetEnvironmentVariable("LC_ALL") ?? "";

        if (lang.Contains("UTF-8", StringComparison.OrdinalIgnoreCase) ||
            lcAll.Contains("UTF-8", StringComparison.OrdinalIgnoreCase))
            return true;

        // Most modern terminals support Unicode
        if (term != "dumb" && !string.IsNullOrEmpty(term))
            return true;

        return false;
    }

    /// <summary>
    /// Creates capabilities for a dumb/fallback terminal.
    /// </summary>
    public static TerminalCapabilities Dumb => new()
    {
        ColorSupport = ColorSupport.None,
        SupportsUnicode = false,
        SupportsAlternateScreen = false,
        SupportsMouse = false,
        SupportsBracketedPaste = false,
        IsCI = false,
        TermType = "dumb"
    };

    /// <summary>
    /// Creates full capabilities (for testing or known good terminals).
    /// </summary>
    public static TerminalCapabilities Full => new()
    {
        ColorSupport = ColorSupport.TrueColor,
        SupportsUnicode = true,
        SupportsAlternateScreen = true,
        SupportsMouse = true,
        SupportsBracketedPaste = true,
        IsCI = false,
        TermType = "xterm-256color"
    };
}
