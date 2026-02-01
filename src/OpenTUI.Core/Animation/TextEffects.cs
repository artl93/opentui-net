using OpenTUI.Core.Colors;

namespace OpenTUI.Core.Animation;

/// <summary>
/// Text effects that can be applied to strings.
/// </summary>
public static class TextEffects
{
    /// <summary>
    /// Creates a rainbow gradient across the text.
    /// </summary>
    /// <param name="text">Text to colorize.</param>
    /// <param name="time">Animation time (seconds).</param>
    /// <param name="speed">How fast the colors cycle.</param>
    /// <returns>Array of (character, color) tuples.</returns>
    public static (char ch, RGBA color)[] Rainbow(string text, double time, double speed = 50)
    {
        var result = new (char, RGBA)[text.Length];
        for (int i = 0; i < text.Length; i++)
        {
            var hue = (time * speed + i * 10) % 360;
            result[i] = (text[i], HsvToRgb(hue, 1.0, 1.0));
        }
        return result;
    }

    /// <summary>
    /// Creates a shimmer/glimmer effect across text.
    /// </summary>
    /// <param name="text">Text to apply effect to.</param>
    /// <param name="time">Animation time (seconds).</param>
    /// <param name="baseColor">Base color of the text.</param>
    /// <param name="highlightColor">Highlight/shimmer color.</param>
    /// <returns>Array of (character, color) tuples.</returns>
    public static (char ch, RGBA color)[] Shimmer(string text, double time, RGBA? baseColor = null, RGBA? highlightColor = null)
    {
        var result = new (char, RGBA)[text.Length];
        var baseTint = baseColor ?? RGBA.FromValues(0.6f, 0.6f, 0.8f);
        var highlight = highlightColor ?? RGBA.White;

        for (int i = 0; i < text.Length; i++)
        {
            var shimmer = (float)(Math.Sin(time * 3 + i * 0.3) + 1) / 2;
            var color = RGBA.FromValues(
                baseTint.R + (highlight.R - baseTint.R) * shimmer,
                baseTint.G + (highlight.G - baseTint.G) * shimmer,
                baseTint.B + (highlight.B - baseTint.B) * shimmer
            );
            result[i] = (text[i], color);
        }
        return result;
    }

    /// <summary>
    /// Creates a pulsing brightness effect.
    /// </summary>
    /// <param name="text">Text to apply effect to.</param>
    /// <param name="time">Animation time (seconds).</param>
    /// <param name="color">Base color.</param>
    /// <param name="minBrightness">Minimum brightness (0-1).</param>
    /// <param name="maxBrightness">Maximum brightness (0-1).</param>
    /// <returns>The text with a single pulsing color.</returns>
    public static RGBA Pulse(double time, RGBA color, float minBrightness = 0.3f, float maxBrightness = 1.0f)
    {
        var pulse = (float)(Math.Sin(time * 4) + 1) / 2;
        var brightness = minBrightness + pulse * (maxBrightness - minBrightness);
        return RGBA.FromValues(
            color.R * brightness,
            color.G * brightness,
            color.B * brightness
        );
    }

    /// <summary>
    /// Creates a wave effect where each character has a slightly different intensity.
    /// </summary>
    /// <param name="text">Text to apply effect to.</param>
    /// <param name="time">Animation time (seconds).</param>
    /// <param name="color">Base color.</param>
    /// <returns>Array of (character, color) tuples.</returns>
    public static (char ch, RGBA color)[] Wave(string text, double time, RGBA color)
    {
        var result = new (char, RGBA)[text.Length];
        for (int i = 0; i < text.Length; i++)
        {
            var wave = (float)(Math.Sin(time * 3 + i * 0.5) + 1) / 2;
            var intensity = 0.4f + wave * 0.6f;
            result[i] = (text[i], RGBA.FromValues(
                color.R * intensity,
                color.G * intensity,
                color.B * intensity
            ));
        }
        return result;
    }

    /// <summary>
    /// Creates a glow effect (pulsing with saturation).
    /// </summary>
    /// <param name="time">Animation time.</param>
    /// <param name="baseColor">Base color for the glow.</param>
    /// <returns>Glowing color.</returns>
    public static RGBA Glow(double time, RGBA baseColor)
    {
        var glow = (float)(Math.Sin(time * 2) + 1) / 2;
        return RGBA.FromValues(
            Math.Min(1f, baseColor.R + glow * 0.3f),
            Math.Min(1f, baseColor.G + glow * 0.2f),
            Math.Min(1f, baseColor.B + glow * 0.1f)
        );
    }

    /// <summary>
    /// Creates a typewriter effect - reveals text character by character.
    /// </summary>
    /// <param name="text">Full text.</param>
    /// <param name="time">Animation time (seconds).</param>
    /// <param name="charsPerSecond">Speed of typing.</param>
    /// <returns>Visible portion of the text.</returns>
    public static string Typewriter(string text, double time, double charsPerSecond = 8)
    {
        var charsToShow = (int)(time * charsPerSecond);
        return text[..Math.Min(charsToShow, text.Length)];
    }

    /// <summary>
    /// Returns whether the cursor should be visible for blinking effect.
    /// </summary>
    /// <param name="time">Current time.</param>
    /// <param name="blinkRate">Blinks per second.</param>
    public static bool CursorVisible(double time, double blinkRate = 2) =>
        (int)(time * blinkRate * 2) % 2 == 0;

    private static RGBA HsvToRgb(double h, double s, double v)
    {
        var hi = (int)(h / 60) % 6;
        var f = h / 60 - (int)(h / 60);
        var p = v * (1 - s);
        var q = v * (1 - f * s);
        var t = v * (1 - (1 - f) * s);

        return hi switch
        {
            0 => RGBA.FromValues((float)v, (float)t, (float)p),
            1 => RGBA.FromValues((float)q, (float)v, (float)p),
            2 => RGBA.FromValues((float)p, (float)v, (float)t),
            3 => RGBA.FromValues((float)p, (float)q, (float)v),
            4 => RGBA.FromValues((float)t, (float)p, (float)v),
            _ => RGBA.FromValues((float)v, (float)p, (float)q)
        };
    }
}
