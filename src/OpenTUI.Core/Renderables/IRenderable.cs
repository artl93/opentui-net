using OpenTUI.Core.Layout;
using OpenTUI.Core.Rendering;

namespace OpenTUI.Core.Renderables;

/// <summary>
/// Interface for objects that can be rendered to a frame buffer.
/// </summary>
public interface IRenderable
{
    /// <summary>Unique identifier for this renderable.</summary>
    string? Id { get; set; }

    /// <summary>The layout node for this renderable.</summary>
    FlexNode Layout { get; }

    /// <summary>Parent renderable.</summary>
    IRenderable? Parent { get; }

    /// <summary>Child renderables.</summary>
    IReadOnlyList<IRenderable> Children { get; }

    /// <summary>Whether this renderable is visible.</summary>
    bool Visible { get; set; }

    /// <summary>Whether this renderable can receive focus.</summary>
    bool Focusable { get; set; }

    /// <summary>Whether this renderable currently has focus.</summary>
    bool IsFocused { get; }

    /// <summary>
    /// Adds a child renderable.
    /// </summary>
    void Add(IRenderable child);

    /// <summary>
    /// Removes a child renderable.
    /// </summary>
    void Remove(IRenderable child);

    /// <summary>
    /// Renders this element to the given buffer.
    /// </summary>
    void Render(FrameBuffer buffer, int offsetX, int offsetY);

    /// <summary>
    /// Gives focus to this renderable.
    /// </summary>
    void Focus();

    /// <summary>
    /// Removes focus from this renderable.
    /// </summary>
    void Blur();

    /// <summary>
    /// Called when the renderable receives focus.
    /// </summary>
    void OnFocus();

    /// <summary>
    /// Called when the renderable loses focus.
    /// </summary>
    void OnBlur();
}
