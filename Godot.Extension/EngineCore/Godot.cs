using System;
using Godot;

namespace Godot.Extension;

public static partial class Extension
{
    public static bool IsAnyChildVisible(this Node self, bool recursive = false)
    {
        foreach (var child in self.GetChildren())
        {
            if (child is not CanvasItem item)
                continue;

            if (item.Visible)
                return true;

            if (recursive && IsAnyChildVisible(item))
                return true;
        }

        return false;
    }

    public static T FindChildByType<T>(this Node root) where T : Node
    {
        foreach (var child in root.GetChildren())
        {
            if (child is T type)
                return type;

            var result = FindChildByType<T>(child);
            if (result != null)
                return result;
        }

        return null;
    }

    public static T FindChildByType<T>(this Node root, Func<Node, bool> condition) where T : Node
    {
        foreach (var child in root.GetChildren())
        {
            if (condition.Invoke(child) && child is T type)
                return type;

            var result = FindChildByType<T>(child);
            if (result != null)
                return result;
        }

        return null;
    }

    public static T FindParentByType<T>(this Node root) where T : Node
    {
        T parent = null;
        Node nextParent = root;

        while (parent == null)
        {
            nextParent = nextParent.GetParent();
            if (!GodotObject.IsInstanceValid(nextParent))
                throw new NullReferenceException("Node is not a child of " + typeof(T).Name);

            if (nextParent.GetType() == typeof(T))
                parent = nextParent as T;
        }

        return parent;
    }

    public static T FindParentBySubclass<T>(this Node root) where T : Node
    {
        T parent = null;
        Node nextParent = root;

        while (parent == null)
        {
            nextParent = nextParent.GetParent();
            if (!GodotObject.IsInstanceValid(nextParent))
                throw new NullReferenceException("Node is not a child of " + typeof(T).Name);

            var pt = nextParent.GetType();
            if (pt == typeof(T) || pt.IsSubclassOf(typeof(T)))
                parent = nextParent as T;
        }

        return parent;
    }

    public static void QueueFreeAllChildren(this Node self, bool isRemoveFromTree = true)
    {
        foreach (var child in self.GetChildren())
        {
            if (isRemoveFromTree)
                self.RemoveChild(child);

            child.QueueFree();
        }
    }
}