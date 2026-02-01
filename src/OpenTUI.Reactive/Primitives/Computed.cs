namespace OpenTUI.Reactive.Primitives;

/// <summary>
/// A computed value that automatically updates when its dependencies change.
/// Similar to React's useMemo or Vue's computed properties.
/// </summary>
public class Computed<T> : IDisposable
{
    private T _cachedValue;
    private bool _isDirty = true;
    private readonly Func<T> _compute;
    private readonly List<IDisposable> _subscriptions = new();
    private readonly List<Action<T>> _subscribers = new();
    private bool _disposed;

    /// <summary>Gets the computed value, recalculating if dirty.</summary>
    public T Value
    {
        get
        {
            if (_isDirty)
            {
                _cachedValue = _compute();
                _isDirty = false;
            }
            return _cachedValue;
        }
    }

    /// <summary>Event raised when the computed value changes.</summary>
    public event EventHandler<T>? Changed;

    public Computed(Func<T> compute)
    {
        _compute = compute;
        _cachedValue = compute();
        _isDirty = false;
    }

    /// <summary>
    /// Declares a dependency on a state value.
    /// When the state changes, this computed will be marked dirty.
    /// </summary>
    public Computed<T> DependsOn<TDep>(State<TDep> state)
    {
        var subscription = state.Subscribe(_ => MarkDirty());
        _subscriptions.Add(subscription);
        return this;
    }

    /// <summary>
    /// Subscribes to value changes.
    /// </summary>
    public IDisposable Subscribe(Action<T> callback)
    {
        _subscribers.Add(callback);
        return new Subscription(() => _subscribers.Remove(callback));
    }

    private void MarkDirty()
    {
        if (_isDirty) return;

        _isDirty = true;
        var newValue = Value; // Recompute immediately
        Changed?.Invoke(this, newValue);

        foreach (var subscriber in _subscribers.ToArray())
        {
            subscriber(newValue);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        foreach (var sub in _subscriptions)
        {
            sub.Dispose();
        }
        _subscriptions.Clear();
        _disposed = true;
    }

    public static implicit operator T(Computed<T> computed) => computed.Value;

    public override string ToString() => Value?.ToString() ?? "null";

    private class Subscription : IDisposable
    {
        private readonly Action _dispose;
        private bool _disposed;

        public Subscription(Action dispose) => _dispose = dispose;

        public void Dispose()
        {
            if (!_disposed)
            {
                _dispose();
                _disposed = true;
            }
        }
    }
}

/// <summary>
/// Factory methods for creating computed values.
/// </summary>
public static class Computed
{
    /// <summary>Creates a computed value from a function.</summary>
    public static Computed<T> Create<T>(Func<T> compute) => new(compute);
}
