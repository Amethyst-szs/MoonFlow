using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MoonFlow.Scene.EditorEvent;

public class GraphMetadata
{
    public bool IsFirstOpen = true;
    public Dictionary<int, NodeMetadata> Nodes = [];
    public Dictionary<string, NodeMetadata> EntryPoints = [];
}