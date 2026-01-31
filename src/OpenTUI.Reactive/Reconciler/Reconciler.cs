using OpenTUI.Core.Renderables;

namespace OpenTUI.Reactive.Reconciler;

/// <summary>
/// Applies patches to a renderable tree.
/// </summary>
public class Reconciler
{
    private VNode? _currentTree;
    private IRenderable? _rootRenderable;

    /// <summary>The current root renderable.</summary>
    public IRenderable? Root => _rootRenderable;

    /// <summary>
    /// Reconciles the tree with a new virtual node tree.
    /// Returns the list of patches that were applied.
    /// </summary>
    public List<Patch> Reconcile(VNode newTree)
    {
        var patches = Differ.Diff(_currentTree, newTree, Array.Empty<int>());
        
        ApplyPatches(patches);
        _currentTree = newTree;

        return patches;
    }

    /// <summary>
    /// Initial render - creates the renderable tree from scratch.
    /// </summary>
    public IRenderable Render(VNode tree)
    {
        _currentTree = tree;
        _rootRenderable = tree.CreateRenderable();
        return _rootRenderable;
    }

    private void ApplyPatches(List<Patch> patches)
    {
        foreach (var patch in patches)
        {
            ApplyPatch(patch);
        }
    }

    private void ApplyPatch(Patch patch)
    {
        switch (patch)
        {
            case CreatePatch create:
                ApplyCreate(create);
                break;
            case RemovePatch remove:
                ApplyRemove(remove);
                break;
            case UpdatePatch update:
                ApplyUpdate(update);
                break;
            case ReplacePatch replace:
                ApplyReplace(replace);
                break;
            case ReorderPatch reorder:
                ApplyReorder(reorder);
                break;
        }
    }

    private void ApplyCreate(CreatePatch patch)
    {
        if (patch.Path.Length == 0)
        {
            // Creating root
            _rootRenderable = patch.Node.CreateRenderable();
            return;
        }

        var parent = NavigateToParent(patch.Path);
        if (parent != null)
        {
            var newRenderable = patch.Node.CreateRenderable();
            parent.Add(newRenderable);
        }
    }

    private void ApplyRemove(RemovePatch patch)
    {
        if (patch.Path.Length == 0)
        {
            _rootRenderable = null;
            return;
        }

        var parent = NavigateToParent(patch.Path);
        var index = patch.Path[^1];
        
        if (parent != null && index < parent.Children.Count)
        {
            var child = parent.Children[index];
            parent.Remove(child);
        }
    }

    private void ApplyUpdate(UpdatePatch patch)
    {
        var renderable = NavigateTo(patch.Path);
        if (renderable != null)
        {
            patch.Node.UpdateRenderable(renderable);
        }
    }

    private void ApplyReplace(ReplacePatch patch)
    {
        if (patch.Path.Length == 0)
        {
            _rootRenderable = patch.Node.CreateRenderable();
            return;
        }

        var parent = NavigateToParent(patch.Path);
        var index = patch.Path[^1];

        if (parent != null && index < parent.Children.Count)
        {
            var oldChild = parent.Children[index];
            parent.Remove(oldChild);
            
            var newRenderable = patch.Node.CreateRenderable();
            // IRenderable only has Add, so we add at end
            // For proper ordering, would need to use Renderable directly
            parent.Add(newRenderable);
        }
    }

    private void ApplyReorder(ReorderPatch patch)
    {
        // Reordering requires cast to Renderable which has Clear/Insert
        var parent = patch.Path.Length == 0 
            ? _rootRenderable 
            : NavigateTo(patch.Path);

        if (parent is not Renderable renderableParent) return;

        var children = parent.Children.ToList();
        
        for (int i = 0; i < patch.OldIndices.Length; i++)
        {
            var oldIndex = patch.OldIndices[i];
            var newIndex = patch.NewIndices[i];

            if (oldIndex < children.Count && newIndex < children.Count)
            {
                (children[oldIndex], children[newIndex]) = (children[newIndex], children[oldIndex]);
            }
        }

        // Rebuild children using Renderable.Clear
        renderableParent.Clear();
        foreach (var child in children)
        {
            renderableParent.Add(child);
        }
    }

    private IRenderable? NavigateTo(int[] path)
    {
        if (_rootRenderable == null) return null;
        if (path.Length == 0) return _rootRenderable;

        var current = _rootRenderable;
        foreach (var index in path)
        {
            if (index >= current.Children.Count)
                return null;
            current = current.Children[index];
        }
        return current;
    }

    private IRenderable? NavigateToParent(int[] path)
    {
        if (path.Length <= 1) return _rootRenderable;
        return NavigateTo(path[..^1]);
    }
}
