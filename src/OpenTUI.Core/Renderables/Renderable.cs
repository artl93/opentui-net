using OpenTUI.Core.Colors;
using OpenTUI.Core.Layout;
using OpenTUI.Core.Rendering;

namespace OpenTUI.Core.Renderables;

/// <summary>
/// Base class for all renderable UI elements.
/// </summary>
public abstract class Renderable : IRenderable
{
    private readonly List<IRenderable> _children = new();
    private IRenderable? _parent;
    private bool _isFocused;

    /// <summary>The layout node for this renderable.</summary>
    public FlexNode Layout { get; }

    /// <summary>Unique identifier.</summary>
    public string? Id
    {
        get => Layout.Id;
        set => Layout.Id = value;
    }

    /// <summary>Parent renderable.</summary>
    public IRenderable? Parent => _parent;

    /// <summary>Child renderables.</summary>
    public IReadOnlyList<IRenderable> Children => _children;

    /// <summary>Whether this renderable is visible.</summary>
    public bool Visible
    {
        get => Layout.Display != Display.None;
        set => Layout.Display = value ? Display.Flex : Display.None;
    }

    /// <summary>Whether this renderable can receive focus.</summary>
    public bool Focusable { get; set; }

    /// <summary>Whether this renderable currently has focus.</summary>
    public bool IsFocused => _isFocused;

    // Common style properties

    /// <summary>Background color.</summary>
    public RGBA? BackgroundColor { get; set; }

    /// <summary>Foreground (text) color.</summary>
    public RGBA? ForegroundColor { get; set; }

    protected Renderable()
    {
        Layout = new FlexNode();
    }

    /// <summary>
    /// Adds a child renderable.
    /// </summary>
    public virtual void Add(IRenderable child)
    {
        if (child is Renderable r)
        {
            r._parent = this;
        }
        _children.Add(child);
        Layout.AddChild(child.Layout);
    }

    /// <summary>
    /// Inserts a child at a specific index.
    /// </summary>
    public virtual void Insert(int index, IRenderable child)
    {
        if (child is Renderable r)
        {
            r._parent = this;
        }
        _children.Insert(index, child);
        Layout.InsertChild(index, child.Layout);
    }

    /// <summary>
    /// Removes a child renderable.
    /// </summary>
    public virtual void Remove(IRenderable child)
    {
        if (_children.Remove(child))
        {
            if (child is Renderable r)
            {
                r._parent = null;
            }
            Layout.RemoveChild(child.Layout);
        }
    }

    /// <summary>
    /// Removes all children.
    /// </summary>
    public virtual void Clear()
    {
        foreach (var child in _children)
        {
            if (child is Renderable r)
            {
                r._parent = null;
            }
        }
        _children.Clear();
        Layout.ClearChildren();
    }

    /// <summary>
    /// Renders this element and its children to the buffer.
    /// </summary>
    public virtual void Render(FrameBuffer buffer, int offsetX, int offsetY)
    {
        if (!Visible) return;

        var x = offsetX + (int)Layout.Layout.X;
        var y = offsetY + (int)Layout.Layout.Y;
        var width = (int)Layout.Layout.Width;
        var height = (int)Layout.Layout.Height;

        // Render background
        if (BackgroundColor.HasValue)
        {
            buffer.FillRect(x, y, width, height, BackgroundColor.Value);
        }

        // Render self (subclasses override this)
        RenderSelf(buffer, x, y, width, height);

        // Render children
        foreach (var child in _children)
        {
            child.Render(buffer, x, y);
        }
    }

    /// <summary>
    /// Override to render this element's specific content.
    /// </summary>
    protected virtual void RenderSelf(FrameBuffer buffer, int x, int y, int width, int height)
    {
        // Base implementation does nothing
    }

    /// <summary>
    /// Gives focus to this renderable.
    /// </summary>
    public void Focus()
    {
        if (!Focusable || _isFocused) return;
        _isFocused = true;
        OnFocus();
    }

    /// <summary>
    /// Removes focus from this renderable.
    /// </summary>
    public void Blur()
    {
        if (!_isFocused) return;
        _isFocused = false;
        OnBlur();
    }

    /// <summary>
    /// Called when the renderable receives focus.
    /// </summary>
    public virtual void OnFocus() { }

    /// <summary>
    /// Called when the renderable loses focus.
    /// </summary>
    public virtual void OnBlur() { }

    /// <summary>
    /// Marks the layout as needing recalculation.
    /// </summary>
    public void MarkDirty() => Layout.MarkDirty();

    /// <summary>
    /// Finds a child by ID recursively.
    /// </summary>
    public IRenderable? FindById(string id)
    {
        if (Id == id) return this;

        foreach (var child in _children)
        {
            if (child.Id == id) return child;
            if (child is Renderable r)
            {
                var found = r.FindById(id);
                if (found != null) return found;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets all focusable descendants in order.
    /// </summary>
    public IEnumerable<IRenderable> GetFocusableDescendants()
    {
        if (Focusable) yield return this;

        foreach (var child in _children)
        {
            if (child is Renderable r)
            {
                foreach (var descendant in r.GetFocusableDescendants())
                {
                    yield return descendant;
                }
            }
            else if (child.Focusable)
            {
                yield return child;
            }
        }
    }
}
