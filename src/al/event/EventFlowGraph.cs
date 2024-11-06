using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Godot;
using Nindot.Byml;

namespace Nindot.Al.EventFlow;

public class Graph
{
    // ====================================================== //
    // ============ Parameters and Initilization ============ //
    // ====================================================== //

    private Dictionary<string, NodeBase> EntryPoints = [];
    private Dictionary<int, NodeBase> Nodes = [];
    private readonly bool _isValid = false;

    public Graph(BymlFile byml, EventFlowFactoryBase nodeFactory)
    {
        // Get access to the two keys on the top of the byml
        if (!byml.TryGetValue(out List<object> entryPointList, "EntryList")) return;
        if (!byml.TryGetValue(out List<object> nodeList, "NodeList")) return;

        if (!InitNodes(nodeList, nodeFactory)) return;
        if (!InitEntryPoints(entryPointList)) return;

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
            NodeBase node = nodeFactory.CreateNode(dict);

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
            NodeBase node = null;

            // Get a copy of the name and node reference
            if (dict.ContainsKey("Name"))
                name = (string)dict["Name"];

            if (dict.ContainsKey("NodeId"))
            {
                int nodeID = (int)dict["NodeId"];
                if (Nodes.TryGetValue(nodeID, out NodeBase value))
                    node = value;
            }

            // Assuming both placeholders got set to non-null values, push to EntryPoints list
            if (name == null || node == null)
                return false;

            EntryPoints.Add(name, node);
        }

        return true;
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

    // ====================================================== //
    // ================== Reading Utilities ================= //
    // ====================================================== //

    public bool IsValid()
    {
        return _isValid;
    }
    public bool IsNodeIdValid(int id)
    {
        return Nodes.ContainsKey(id);
    }
    public bool IsNodeOrphanSolo(NodeBase node)
    {
        int id = node.GetId();

        foreach (var cmp in Nodes.Values)
        {
            if (cmp.GetNextNodeList().Contains(id))
                return false;
        }

        return true;
    }

    public NodeBase GetNode(int id)
    {
        return Nodes[id];
    }
    public int GetNextUnusedNodeId()
    {
        int maxValue = Nodes.Keys.Max();
        return Nodes.Keys.ToList().IndexOf(maxValue);
    }

    // ====================================================== //
    // ================== Editing Utilities ================= //
    // ====================================================== //

    public void AddNode(NodeBase node)
    {
        // Ensure this Id isn't already in the dictionary
        // If so, reassign the Id before adding
        int id = node.GetId();
        if (Nodes.ContainsKey(id))
            node.ReassignId(this);
        
        Nodes.Add(id, node);
    }

    public void DestroyNode(NodeBase node)
    {
        if (!Nodes.ContainsValue(node))
            return;
        
        // Remove all connections to this node from other nodes in graph
        int id = node.GetId();
        foreach (var cmp in Nodes.Values)
        {
            int cmpCount = cmp.GetNextNodeCount();
            for (int i = 0; i < cmpCount; i++)
            {
                NodeBase cmpNext = cmp.GetNextNode(this, i);
                if (cmpNext == node)
                    cmp.RemoveNextNode(i);
            }
        }

        // Remove node from list
        Nodes.Remove(id);
    }
}