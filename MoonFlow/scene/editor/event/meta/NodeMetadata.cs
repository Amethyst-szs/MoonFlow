using System.Collections.Generic;
using Godot;

using static MoonFlow.Ext.Extension;

namespace MoonFlow.Scene.EditorEvent;

public class NodeMetadata
{
    public NodeMetadata() { }

    public Vector2 Position = Vector2.Zero;
    public string Comment = "";
    public List<string> Tags = [];

    public Color OverrideColor = Colors.White;
    public bool IsOverrideColor = false;
}