using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Godot;
using Nindot.Byml;

namespace Nindot.Al.EventFlow;

public partial class Graph
{
    protected Dictionary<string, Node> EntryPoints = [];
    protected Dictionary<int, Node> Nodes = [];

    private readonly ushort _bymlVersion = 2;
    private readonly bool _isValid = false;

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
    public bool IsNodeOrphanSolo(Node node)
    {
        int id = node.GetId();

        foreach (var cmp in Nodes.Values)
        {
            if (cmp.GetNextIds().Contains(id))
                return false;
        }

        return true;
    }

    public Node GetNode(int id)
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

    public void AddNode(Node node)
    {
        // Ensure this instance isn't already in the dictionary
        if (Nodes.ContainsValue(node))
            return;
        
        // Ensure this Id isn't already in the dictionary
        // If so, reassign the Id before adding
        int id = node.GetId();
        if (Nodes.ContainsKey(id))
            id = node.ReassignId(this);

        Nodes.Add(id, node);
    }

    public void DestroyNode(Node node)
    {
        if (!Nodes.ContainsValue(node))
            return;

        // Remove all connections to this node from other nodes in graph
        int id = node.GetId();
        foreach (var cmp in Nodes.Values)
        {
            int cmpCount = cmp.GetNextIds().Length;

            for (int i = 0; i < cmpCount; i++)
            {
                Node cmpNext = cmp.GetNextNode(this, i);
                if (cmpNext == node)
                    cmp.RemoveNextNode(i);
            }
        }

        // Remove node from list
        Nodes.Remove(id);
    }
}