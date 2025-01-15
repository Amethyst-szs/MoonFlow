using System.Collections.Generic;
using System.Text.Json.Serialization;
using Godot;

namespace MoonFlow.Project;

public class GraphMetaBucketNode
{
    public GraphMetaBucketNode() { }

    [JsonInclude]
    public Vector2 Position = Vector2.Zero;
    [JsonInclude]
    public string Comment = "";
    [JsonInclude]
    public List<string> Tags = [];

    [JsonInclude]
    public Color OverrideColor = Colors.White;
    [JsonInclude]
    public bool IsOverrideColor = false;
}