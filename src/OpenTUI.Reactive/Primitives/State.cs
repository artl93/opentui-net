namespace OpenTUI.Reactive.Primitives;

/// <summary>
/// A reactive state container that notifies subscribers when its value changes.
/// Inspired by React's useState and Solid's createSignal.
/// </summary>
public class State<T>
{
    private T _value;
    private readonly List<Action<T>> _subscribers = new();
    private readonly EqualityComparer<T> _comparer;

    /// <summary>Gets the current value.</summary>
    public T Value
    {
        get => _value;
        set => Set(value);
    }

    /// <summary>Event raised when the value changes.</summary>
    public event EventHandler<T>? Changed;

    public State(T initialValue, EqualityComparer<T>? comparer = null)
    {
        _value = initialValue;
        _comparer = comparer ?? EqualityComparer<T>.Default;
    }

    /// <summary>
    /// Sets the value and notifies subscribers if changed.
    /// </summary>
    public void Set(T newValue)
    {
        if (_comparer.Equals(_value, newValue))
            return;

        _value = newValue;
        NotifySubscribers();
    }

    /// <summary>
    /// Updates the value using a function.
    /// </summary>
    public void Update(Func<T, T> updater)
    {
        Set(updater(_value));
    }

    /// <summary>
    /// Subscribes to value changes.
    /// </summary>
    public IDisposable Subscribe(Action<T> callback)
    {
        _subscribers.Add(callback);
        return new Subscription(() => _subscribers.Remove(callback));
    }

    /// <summary>
    /// Gets the value (for implicit conversion).
    /// </summary>
    public T Get() => _value;

    private void NotifySubscribers()
    {
        Changed?.Invoke(this, _value);
        foreach (var subscriber in _subscribers.ToArray())
        {
            subscriber(_value);
        }
    }

    public static implicit operator T(State<T> state) => state._value;

    public override string ToString() => _value?.ToString() ?? "null";

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
/// Factory methods for creating reactive state.
/// </summary>
public static class State
{
    /// <summary>Creates a new state with the given initial value.</summary>
    public static State<T> Create<T>(T initialValue) => new(initialValue);

    /// <summary>Creates a new state with a custom equality comparer.</summary>
    public static State<T> Create<T>(T initialValue, EqualityComparer<T> comparer)
        => new(initialValue, comparer);
}
