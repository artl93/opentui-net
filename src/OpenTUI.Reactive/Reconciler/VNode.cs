using OpenTUI.Core.Renderables;

namespace OpenTUI.Reactive.Reconciler;

/// <summary>
/// Represents a virtual node in the component tree.
/// Used for diffing and reconciliation.
/// </summary>
public abstract class VNode
{
    /// <summary>Unique key for efficient diffing.</summary>
    public string? Key { get; init; }

    /// <summary>The component type or element type.</summary>
    public abstract Type NodeType { get; }

    /// <summary>Creates the actual renderable from this virtual node.</summary>
    public abstract IRenderable CreateRenderable();

    /// <summary>Updates an existing renderable with new props.</summary>
    public abstract bool UpdateRenderable(IRenderable existing);
}

/// <summary>
/// A virtual node representing a renderable element.
/// </summary>
public class ElementNode : VNode
{
    private readonly Func<IRenderable> _factory;
    private readonly Action<IRenderable>? _configure;
    private readonly Type _type;

    public override Type NodeType => _type;

    public ElementNode(Type type, Func<IRenderable> factory, Action<IRenderable>? configure = null)
    {
        _type = type;
        _factory = factory;
        _configure = configure;
    }

    public override IRenderable CreateRenderable()
    {
        var renderable = _factory();
        _configure?.Invoke(renderable);
        return renderable;
    }

    public override bool UpdateRenderable(IRenderable existing)
    {
        if (existing.GetType() != _type)
            return false;

        _configure?.Invoke(existing);
        return true;
    }
}

/// <summary>
/// A virtual node representing a component.
/// </summary>
public class ComponentNode : VNode
{
    private readonly Func<Components.Component> _factory;
    private readonly Type _type;

    public override Type NodeType => _type;

    public ComponentNode(Type type, Func<Components.Component> factory)
    {
        _type = type;
        _factory = factory;
    }

    public override IRenderable CreateRenderable()
    {
        var component = _factory();
        component.Mount();
        return component.GetRenderedContent();
    }

    public override bool UpdateRenderable(IRenderable existing)
    {
        // Components are managed separately
        return false;
    }
}

/// <summary>
/// A virtual node containing child nodes.
/// </summary>
public class FragmentNode : VNode
{
    public List<VNode> Children { get; } = new();

    public override Type NodeType => typeof(FragmentNode);

    public FragmentNode(params VNode[] children)
    {
        Children.AddRange(children);
    }

    public override IRenderable CreateRenderable()
    {
        var group = new GroupRenderable();
        foreach (var child in Children)
        {
            group.Add(child.CreateRenderable());
        }
        return group;
    }

    public override bool UpdateRenderable(IRenderable existing)
    {
        // Fragments need special reconciliation
        return false;
    }
}

/// <summary>
/// Factory methods for creating virtual nodes.
/// </summary>
public static class VNodes
{
    /// <summary>Creates an element node for a renderable type.</summary>
    public static ElementNode Element<T>(Action<T>? configure = null) where T : IRenderable, new()
    {
        return new ElementNode(
            typeof(T),
            () => new T(),
            configure != null ? r => configure((T)r) : null
        );
    }

    /// <summary>Creates an element node with a factory.</summary>
    public static ElementNode Element<T>(Func<T> factory, Action<T>? configure = null) where T : IRenderable
    {
        return new ElementNode(
            typeof(T),
            () => factory(),
            configure != null ? r => configure((T)r) : null
        );
    }

    /// <summary>Creates a component node.</summary>
    public static ComponentNode Component<T>() where T : Components.Component, new()
    {
        return new ComponentNode(typeof(T), () => new T());
    }

    /// <summary>Creates a component node with factory.</summary>
    public static ComponentNode Component<T>(Func<T> factory) where T : Components.Component
    {
        return new ComponentNode(typeof(T), factory);
    }

    /// <summary>Creates a fragment containing multiple nodes.</summary>
    public static FragmentNode Fragment(params VNode[] children)
    {
        return new FragmentNode(children);
    }
}
