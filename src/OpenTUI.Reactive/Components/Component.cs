using OpenTUI.Core.Renderables;
using OpenTUI.Reactive.Primitives;

namespace OpenTUI.Reactive.Components;

/// <summary>
/// Base class for reactive components with lifecycle methods.
/// Similar to React class components but simpler.
/// </summary>
public abstract class Component : IDisposable
{
    private readonly List<IDisposable> _disposables = new();
    private bool _isMounted;
    private bool _disposed;
    private IRenderable? _renderedContent;

    /// <summary>Whether the component is currently mounted.</summary>
    public bool IsMounted => _isMounted;

    /// <summary>The parent component, if any.</summary>
    public Component? Parent { get; internal set; }

    /// <summary>Child components.</summary>
    protected List<Component> Children { get; } = new();

    /// <summary>
    /// Called when the component is first mounted.
    /// Override to initialize resources, start effects, etc.
    /// </summary>
    protected virtual void OnMount() { }

    /// <summary>
    /// Called when the component is unmounted.
    /// Override to clean up resources.
    /// </summary>
    protected virtual void OnUnmount() { }

    /// <summary>
    /// Called before the component re-renders.
    /// Return false to skip the render.
    /// </summary>
    protected virtual bool ShouldRender() => true;

    /// <summary>
    /// Renders the component. Must be implemented by subclasses.
    /// </summary>
    public abstract IRenderable Render();

    /// <summary>
    /// Gets the current rendered output, re-rendering if necessary.
    /// </summary>
    public IRenderable GetRenderedContent()
    {
        if (_renderedContent == null || ShouldRender())
        {
            _renderedContent = Render();
        }
        return _renderedContent;
    }

    /// <summary>
    /// Mounts the component and its children.
    /// </summary>
    public void Mount()
    {
        if (_isMounted) return;

        _isMounted = true;
        OnMount();

        foreach (var child in Children)
        {
            child.Mount();
        }
    }

    /// <summary>
    /// Unmounts the component and its children.
    /// </summary>
    public void Unmount()
    {
        if (!_isMounted) return;

        foreach (var child in Children)
        {
            child.Unmount();
        }

        OnUnmount();
        _isMounted = false;
    }

    /// <summary>
    /// Forces a re-render of the component.
    /// </summary>
    public void ForceUpdate()
    {
        _renderedContent = null;
    }

    /// <summary>
    /// Adds a child component.
    /// </summary>
    protected void AddChild(Component child)
    {
        child.Parent = this;
        Children.Add(child);

        if (_isMounted)
        {
            child.Mount();
        }
    }

    /// <summary>
    /// Removes a child component.
    /// </summary>
    protected void RemoveChild(Component child)
    {
        if (Children.Remove(child))
        {
            if (_isMounted)
            {
                child.Unmount();
            }
            child.Parent = null;
        }
    }

    /// <summary>
    /// Creates a state that triggers re-render when changed.
    /// </summary>
    protected State<T> UseState<T>(T initialValue)
    {
        var state = State.Create(initialValue);
        state.Changed += (_, _) => ForceUpdate();
        Track(state.Subscribe(_ => { })); // Keep subscription alive
        return state;
    }

    /// <summary>
    /// Creates an effect that runs when dependencies change.
    /// </summary>
    protected Effect UseEffect(Action effect, Action? cleanup = null)
    {
        var eff = Effects.Create(effect, cleanup);
        Track(eff);
        return eff;
    }

    /// <summary>
    /// Tracks a disposable to be cleaned up when component unmounts.
    /// </summary>
    protected void Track(IDisposable disposable)
    {
        _disposables.Add(disposable);
    }

    public void Dispose()
    {
        if (_disposed) return;

        Unmount();

        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
        _disposables.Clear();

        foreach (var child in Children.ToArray())
        {
            child.Dispose();
        }
        Children.Clear();

        _disposed = true;
    }
}
