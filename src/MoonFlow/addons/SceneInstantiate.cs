using System;
using System.Reflection;

namespace Godot;

// Assign a scene path for this class, allowing the scene to quickly instantiated through SceneInstantiate
[AttributeUsage(AttributeTargets.Class)]
public class ScenePath(string path) : Attribute
{
    public readonly string Path = path;
}

public static class SceneCreator<T>
{
    public static T Create()
    {
        var attr = typeof(T).GetCustomAttribute<ScenePath>();
        if (attr == null)
            throw new Exception("Class does not have scene path attribute");
        
        var scene = GD.Load<PackedScene>(attr.Path);
        var instance = scene.Instantiate();
        return (T)Convert.ChangeType(instance, typeof(T));
    }
}