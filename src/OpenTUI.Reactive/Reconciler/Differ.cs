using OpenTUI.Core.Renderables;

namespace OpenTUI.Reactive.Reconciler;

/// <summary>
/// Represents a change to be applied to the renderable tree.
/// </summary>
public abstract record Patch
{
    /// <summary>The path to the node being modified.</summary>
    public int[] Path { get; init; } = Array.Empty<int>();
}

/// <summary>Patch to create a new node.</summary>
public record CreatePatch(VNode Node) : Patch;

/// <summary>Patch to remove a node.</summary>
public record RemovePatch : Patch;

/// <summary>Patch to update an existing node.</summary>
public record UpdatePatch(VNode Node) : Patch;

/// <summary>Patch to replace a node with a different type.</summary>
public record ReplacePatch(VNode Node) : Patch;

/// <summary>Patch to reorder children.</summary>
public record ReorderPatch(int[] OldIndices, int[] NewIndices) : Patch;

/// <summary>
/// Calculates differences between virtual node trees.
/// </summary>
public static class Differ
{
    /// <summary>
    /// Diffs two virtual node trees and returns a list of patches.
    /// </summary>
    public static List<Patch> Diff(VNode? oldNode, VNode? newNode, int[] path)
    {
        var patches = new List<Patch>();

        // Handle null cases
        if (oldNode == null && newNode == null)
        {
            return patches;
        }

        if (oldNode == null && newNode != null)
        {
            patches.Add(new CreatePatch(newNode) { Path = path });
            return patches;
        }

        if (oldNode != null && newNode == null)
        {
            patches.Add(new RemovePatch { Path = path });
            return patches;
        }

        // Both are non-null
        if (oldNode!.NodeType != newNode!.NodeType)
        {
            // Different types - replace entirely
            patches.Add(new ReplacePatch(newNode) { Path = path });
            return patches;
        }

        // Same type - check if update is needed
        if (oldNode.Key != newNode.Key)
        {
            patches.Add(new ReplacePatch(newNode) { Path = path });
            return patches;
        }

        // Update the node
        patches.Add(new UpdatePatch(newNode) { Path = path });

        // Diff children for fragments
        if (oldNode is FragmentNode oldFragment && newNode is FragmentNode newFragment)
        {
            DiffChildren(oldFragment.Children, newFragment.Children, path, patches);
        }

        return patches;
    }

    private static void DiffChildren(
        List<VNode> oldChildren,
        List<VNode> newChildren,
        int[] parentPath,
        List<Patch> patches)
    {
        var maxLen = Math.Max(oldChildren.Count, newChildren.Count);

        for (int i = 0; i < maxLen; i++)
        {
            var childPath = parentPath.Append(i).ToArray();
            var oldChild = i < oldChildren.Count ? oldChildren[i] : null;
            var newChild = i < newChildren.Count ? newChildren[i] : null;

            patches.AddRange(Diff(oldChild, newChild, childPath));
        }

        // Check for reordering based on keys
        if (oldChildren.Any(c => c.Key != null) && newChildren.Any(c => c.Key != null))
        {
            var reorderPatch = DetectReorder(oldChildren, newChildren, parentPath);
            if (reorderPatch != null)
            {
                patches.Add(reorderPatch);
            }
        }
    }

    private static ReorderPatch? DetectReorder(
        List<VNode> oldChildren,
        List<VNode> newChildren,
        int[] parentPath)
    {
        var oldKeys = oldChildren
            .Select((c, i) => (Key: c.Key, Index: i))
            .Where(x => x.Key != null)
            .ToDictionary(x => x.Key!, x => x.Index);

        var newKeys = newChildren
            .Select((c, i) => (Key: c.Key, Index: i))
            .Where(x => x.Key != null)
            .ToList();

        if (newKeys.Count == 0) return null;

        var oldIndices = new List<int>();
        var newIndices = new List<int>();

        foreach (var (key, newIndex) in newKeys)
        {
            if (oldKeys.TryGetValue(key, out var oldIndex) && oldIndex != newIndex)
            {
                oldIndices.Add(oldIndex);
                newIndices.Add(newIndex);
            }
        }

        if (oldIndices.Count > 0)
        {
            return new ReorderPatch(oldIndices.ToArray(), newIndices.ToArray()) { Path = parentPath };
        }

        return null;
    }
}
