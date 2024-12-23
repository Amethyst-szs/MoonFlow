using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MoonFlow.Scene.EditorEvent;

public class GraphMetadata
{
    public string ArchiveName = "";
    public string FileName = "";
    
    public bool IsFirstOpen = true;

    public Dictionary<int, NodeMetadata> Nodes = [];
    public Dictionary<string, NodeMetadata> EntryPoints = [];

    public NodeMetadata RenameNode(int oldId, int newId)
    {
        if (!Nodes.TryGetValue(oldId, out NodeMetadata instance))
        {
            var n = new NodeMetadata();
            Nodes.Add(newId, n);
            return n;
        }

        Nodes.Remove(oldId);
        Nodes.Add(newId, instance);

        return instance;
    }
    public NodeMetadata RenameEntryPoint(string oldName, string newName)
    {
        if (!EntryPoints.TryGetValue(oldName, out NodeMetadata instance))
        {
            var n = new NodeMetadata();
            EntryPoints.Add(newName, n);
            return n;
        }

        EntryPoints.Remove(oldName);
        EntryPoints.Add(newName, instance);

        return instance;
    }
}