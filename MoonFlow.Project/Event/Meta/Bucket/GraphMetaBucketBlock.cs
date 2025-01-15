using System.Collections.Generic;
using System.Text.Json.Serialization;
using Godot;

namespace MoonFlow.Project;

public class GraphMetaBucketBlock
{
    public GraphMetaBucketBlock() { }

    [JsonInclude]
    public Vector2 Position = Vector2.Zero;
    [JsonInclude]
    public Vector2 Size = Vector2.Zero;

    [JsonInclude]
    public string Label = "Group";
    [JsonInclude]
    public float Hue = 0f;
}