using System;
using System.Reflection;

namespace Godot.Extension;

[Obsolete("ScenePath atrribute is no longer used as of Godot 4.4, please replace with SceneUid", false)]
[AttributeUsage(AttributeTargets.Class)]
public class ScenePath(string path) : Attribute
{
    public readonly string Path = path;
}

// Assign a scene UID for this class, allowing the scene to quickly instantiated through SceneInstantiate
[AttributeUsage(AttributeTargets.Class)]
public class SceneUid(string uid) : Attribute
{
    public readonly string Uid = uid;
}

public static class SceneCreator<T>
{
    public static T Create()
    {
        var attr = typeof(T).GetCustomAttribute<SceneUid>()
        ?? throw new Exception("Class does not have SceneUid attribute");

        if (!attr.Uid.StartsWith("uid://"))
            throw new Exception("SceneUid " + attr.Uid + "is not correctly formatted (doesn't start with uid://)");
        
        var scene = GD.Load<PackedScene>(attr.Uid);
        var instance = scene.Instantiate();
        return (T)Convert.ChangeType(instance, typeof(T));
    }
}