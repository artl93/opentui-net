using OpenTUI.Core.Renderables;

namespace OpenTUI.Reactive.Components;

/// <summary>
/// A functional component defined by a render function.
/// Simpler alternative to Component class for stateless components.
/// </summary>
public class FunctionalComponent : Component
{
    private readonly Func<IRenderable> _render;

    public FunctionalComponent(Func<IRenderable> render)
    {
        _render = render;
    }

    public override IRenderable Render() => _render();
}

/// <summary>
/// A functional component with props.
/// </summary>
public class FunctionalComponent<TProps> : Component
{
    private readonly Func<TProps, IRenderable> _render;
    private TProps _props;

    public TProps Props
    {
        get => _props;
        set
        {
            if (!EqualityComparer<TProps>.Default.Equals(_props, value))
            {
                _props = value;
                ForceUpdate();
            }
        }
    }

    public FunctionalComponent(Func<TProps, IRenderable> render, TProps initialProps)
    {
        _render = render;
        _props = initialProps;
    }

    public override IRenderable Render() => _render(_props);
}

/// <summary>
/// Factory for creating functional components.
/// </summary>
public static class FC
{
    /// <summary>Creates a stateless functional component.</summary>
    public static FunctionalComponent Create(Func<IRenderable> render) => new(render);

    /// <summary>Creates a functional component with props.</summary>
    public static FunctionalComponent<TProps> Create<TProps>(
        Func<TProps, IRenderable> render,
        TProps initialProps) => new(render, initialProps);
}
