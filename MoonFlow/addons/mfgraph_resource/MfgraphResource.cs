using System.Linq;
using Godot;
using Godot.Collections;

using MoonFlow.Project;

namespace MoonFlow.Addons;

[GlobalClass, Tool]
public partial class MfgraphResource() : Resource()
{
    [Export]
    public string ArchiveName = "";
    [Export]
    public string FileName = "";
    [Export]
    public string HashName = "";
    [Export]
    public Dictionary<int, string> Nodes = [];
    [Export]
    public Array EntryPoints = [];

    public void InitResource(string path)
    {
        var data = new GraphMetaHolder(path).Data;

        HashName = path.Split(['/', '\\']).Last();
        ArchiveName = data.ArchiveName;
        FileName = data.FileName;

        foreach (var node in data.Nodes)
            Nodes.Add(node.Key, node.Value.Comment);
        
        foreach (var enter in data.EntryPoints)
            EntryPoints.Add(enter.Key);
    }

    public override void _ValidateProperty(Dictionary property)
    {
        var usage = property["usage"].As<PropertyUsageFlags>() | PropertyUsageFlags.ReadOnly;
        property["usage"] = (int)usage;
    }
}