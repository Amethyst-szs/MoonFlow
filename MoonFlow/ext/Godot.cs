using System;
using Godot;

namespace MoonFlow.Ext;

public static partial class Extension
{
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
}