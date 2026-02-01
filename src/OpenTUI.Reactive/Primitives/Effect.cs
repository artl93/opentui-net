namespace OpenTUI.Reactive.Primitives;

/// <summary>
/// Represents a side effect that runs when dependencies change.
/// Similar to React's useEffect.
/// </summary>
public class Effect : IDisposable
{
    private readonly Action _effect;
    private readonly Action? _cleanup;
    private readonly List<IDisposable> _subscriptions = new();
    private bool _disposed;
    private bool _hasRun;

    public Effect(Action effect, Action? cleanup = null)
    {
        _effect = effect;
        _cleanup = cleanup;
    }

    /// <summary>
    /// Declares a dependency on a state value.
    /// When the state changes, the effect will re-run.
    /// </summary>
    public Effect DependsOn<T>(State<T> state)
    {
        var subscription = state.Subscribe(_ => Run());
        _subscriptions.Add(subscription);
        return this;
    }

    /// <summary>
    /// Runs the effect immediately.
    /// </summary>
    public void Run()
    {
        if (_disposed) return;

        // Run cleanup from previous execution
        if (_hasRun)
        {
            _cleanup?.Invoke();
        }

        _effect();
        _hasRun = true;
    }

    /// <summary>
    /// Runs the effect immediately and returns this for chaining.
    /// </summary>
    public Effect RunNow()
    {
        Run();
        return this;
    }

    public void Dispose()
    {
        if (_disposed) return;

        _cleanup?.Invoke();

        foreach (var sub in _subscriptions)
        {
            sub.Dispose();
        }
        _subscriptions.Clear();
        _disposed = true;
    }
}

/// <summary>
/// Factory methods for creating effects.
/// </summary>
public static class Effects
{
    /// <summary>Creates an effect that runs the given action.</summary>
    public static Effect Create(Action effect, Action? cleanup = null)
        => new(effect, cleanup);

    /// <summary>Creates and immediately runs an effect.</summary>
    public static Effect Run(Action effect, Action? cleanup = null)
        => new Effect(effect, cleanup).RunNow();
}
