using System.Collections.Generic;
using Godot;

namespace MoonFlow.Scene.EditorEvent;

public class NodeMetadata
{
    public NodeMetadata() { }

    public Vector2 Position = Vector2.Zero;
    public string Comment = "";
    public List<string> Tags = [];

    public bool IsOverrideColor = false;
    public Color OverrideColor;
}