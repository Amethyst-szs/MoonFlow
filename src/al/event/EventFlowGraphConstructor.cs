using System.Collections.Generic;
using Godot;
using Nindot.Byml;

namespace Nindot.Al.EventFlow;

public partial class Graph
{
    public Graph(BymlFile byml, EventFlowFactoryBase nodeFactory)
    {
        // Set byml version
        _bymlVersion = byml.Version;
        
        // Get access to the two keys on the top of the byml
        if (!byml.TryGetValue(out List<object> entryPointList, "EntryList")) return;
        if (!byml.TryGetValue(out List<object> nodeList, "NodeList")) return;

        if (!InitNodes(nodeList, nodeFactory)) return;
        if (!InitEntryPoints(entryPointList)) return;

        byml.TryGetValue(out List<object> itemList, "ItemList");
        InitItemList(itemList);

        _isValid = true;
        return;
    }

    private bool InitNodes(List<object> nodeList, EventFlowFactoryBase nodeFactory)
    {
        foreach (var n in nodeList)
        {
            if (n.GetType() != typeof(Dictionary<object, object>))
            {
                GD.PushError("NodeList element in EventFlowGraph is not a dictionary!");
                return false;
            }

            var dict = (Dictionary<object, object>)n;
            Node node = nodeFactory.CreateNode(dict);

            if (Nodes.ContainsKey(node.GetId()))
            {
                GD.PushWarning("Duplicate ID keys in EventFlowGraph, attempting to correct error...");
                node.ReassignId(this);
            }

            if (node != null)
                Nodes.Add(node.GetId(), node);
        }

        return true;
    }

    private bool InitEntryPoints(List<object> entryPointList)
    {
        // Iterate through the enter points, and create the dictionary entries
        foreach (var enter in entryPointList)
        {
            if (enter.GetType() != typeof(Dictionary<object, object>))
            {
                GD.PushError("EntryList element in EventFlowGraph is not a dictionary!");
                return false;
            }

            // Create a dictionary version of the entry object, and create placeholders for name and node
            var dict = (Dictionary<object, object>)enter;
            string name = null;
            Node node = null;

            // Get a copy of the name and node reference
            if (dict.ContainsKey("Name"))
                name = (string)dict["Name"];

            if (dict.ContainsKey("NodeId"))
            {
                int nodeID = (int)dict["NodeId"];
                if (Nodes.TryGetValue(nodeID, out Node value))
                    node = value;
            }

            // Assuming both placeholders got set to non-null values, push to EntryPoints list
            if (name == null || node == null)
                return false;

            EntryPoints.Add(name, node);
        }

        return true;
    }

    private void InitItemList(List<object> list)
    {
        if (list == null || list.Count == 0)
            return;
        
        foreach (var item in list)
        {
            if (item.GetType() != typeof(string))
            {
                GD.PushWarning("Ignoring element in ItemList that isn't string type!");
                continue;
            }

            var entry = new ItemEntry((string)item);
            ItemList.Add(entry);
        }
    }

    public static Graph FromFilePath(string path, EventFlowFactoryBase nodeFactory)
    {
        if (!BymlFileAccess.ParseFile(out BymlFile file, path))
            return null;

        return new Graph(file, nodeFactory);
    }
    public static Graph FromBytes(byte[] bytes, EventFlowFactoryBase nodeFactory)
    {
        if (!BymlFileAccess.ParseBytes(out BymlFile file, bytes))
            return null;

        return new Graph(file, nodeFactory);
    }
}