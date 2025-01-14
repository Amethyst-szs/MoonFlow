using System.Collections.Generic;
using Godot;

namespace MoonFlow.Project;

public class BlockMetadata
{
    public BlockMetadata() { }

    public Vector2 Position = Vector2.Zero;
    public Vector2 Size = Vector2.Zero;

    public string Label = "Group";
    public float Hue = 0f;
}