namespace OpenTUI.Core.Animation;

/// <summary>
/// Easing functions for animations.
/// </summary>
public static class Easing
{
    /// <summary>Linear interpolation (no easing).</summary>
    public static float Linear(float t) => t;

    /// <summary>Quadratic ease in.</summary>
    public static float InQuad(float t) => t * t;

    /// <summary>Quadratic ease out.</summary>
    public static float OutQuad(float t) => t * (2 - t);

    /// <summary>Quadratic ease in-out.</summary>
    public static float InOutQuad(float t) => t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;

    /// <summary>Cubic ease in.</summary>
    public static float InCubic(float t) => t * t * t;

    /// <summary>Cubic ease out.</summary>
    public static float OutCubic(float t) => (--t) * t * t + 1;

    /// <summary>Cubic ease in-out.</summary>
    public static float InOutCubic(float t) => t < 0.5f ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;

    /// <summary>Exponential ease in.</summary>
    public static float InExpo(float t) => t == 0 ? 0 : MathF.Pow(2, 10 * (t - 1));

    /// <summary>Exponential ease out.</summary>
    public static float OutExpo(float t) => t == 1 ? 1 : 1 - MathF.Pow(2, -10 * t);

    /// <summary>Sine ease in-out.</summary>
    public static float InOutSine(float t) => -(MathF.Cos(MathF.PI * t) - 1) / 2;

    /// <summary>Bounce ease out.</summary>
    public static float OutBounce(float t)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        if (t < 1 / d1)
            return n1 * t * t;
        if (t < 2 / d1)
            return n1 * (t -= 1.5f / d1) * t + 0.75f;
        if (t < 2.5f / d1)
            return n1 * (t -= 2.25f / d1) * t + 0.9375f;
        return n1 * (t -= 2.625f / d1) * t + 0.984375f;
    }

    /// <summary>Bounce ease in.</summary>
    public static float InBounce(float t) => 1 - OutBounce(1 - t);

    /// <summary>Elastic ease out.</summary>
    public static float OutElastic(float t)
    {
        const float c4 = 2 * MathF.PI / 3;
        return t == 0 ? 0 : t == 1 ? 1 : MathF.Pow(2, -10 * t) * MathF.Sin((t * 10 - 0.75f) * c4) + 1;
    }

    /// <summary>Elastic ease in.</summary>
    public static float InElastic(float t)
    {
        const float c4 = 2 * MathF.PI / 3;
        return t == 0 ? 0 : t == 1 ? 1 : -MathF.Pow(2, 10 * t - 10) * MathF.Sin((t * 10 - 10.75f) * c4);
    }

    /// <summary>Circular ease in.</summary>
    public static float InCirc(float t) => 1 - MathF.Sqrt(1 - t * t);

    /// <summary>Circular ease out.</summary>
    public static float OutCirc(float t) => MathF.Sqrt(1 - MathF.Pow(t - 1, 2));

    /// <summary>Back ease in (overshoots then returns).</summary>
    public static float InBack(float t, float s = 1.70158f) => t * t * ((s + 1) * t - s);

    /// <summary>Back ease out (overshoots then returns).</summary>
    public static float OutBack(float t, float s = 1.70158f) => (t -= 1) * t * ((s + 1) * t + s) + 1;
}

/// <summary>
/// Delegate for easing functions.
/// </summary>
public delegate float EasingFunction(float t);
