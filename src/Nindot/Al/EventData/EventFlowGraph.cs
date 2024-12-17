using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace Nindot.Al.EventFlow;

public partial class Graph
{
    public Dictionary<string, Node> EntryPoints { get; private set; } = [];
    public Dictionary<int, Node> Nodes { get; private set; } = [];
    public List<ItemEntry> ItemList { get; private set; } = [];

    public string Name { get; private set; } = null;

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
        int id = node.Id;

        foreach (var cmp in Nodes.Values)
        {
            if (cmp.GetNextIds().Contains(id))
                return false;
        }

        return true;
    }
    public bool IsNodeEntryPoint(Node node)
    {
        return EntryPoints.ContainsValue(node);
    }
    public bool IsContainItem(string itemName)
    {
        foreach (var item in ItemList)
        {
            if (item.GetName() == itemName)
                return true;
        }

        return false;
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

    public string GetNodeEntryPointName(Node node)
    {
        if (!IsNodeEntryPoint(node))
            return null;

        int index = EntryPoints.Values.ToList().IndexOf(node);
        return EntryPoints.Keys.ElementAt(index);
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
        int id = node.Id;
        if (Nodes.ContainsKey(id))
            id = node.ReassignId(this);

        Nodes.Add(id, node);
    }
    public void RemoveNode(Node node)
    {
        if (!Nodes.ContainsValue(node))
            return;

        // Remove all connections to this node from other nodes in graph
        int id = node.Id;
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

    public void SetNodeEntryPointName(Node node, string entryPointName)
    {
        if (IsNodeEntryPoint(node))
        {
            string n = GetNodeEntryPointName(node);
            EntryPoints.Remove(n);
        }

        EntryPoints[entryPointName] = node;
    }
    public void RemoveNodeEntryPointName(Node node)
    {
        if (!IsNodeEntryPoint(node))
            return;

        string n = GetNodeEntryPointName(node);
        EntryPoints.Remove(n);
    }

    public void AddItem(ItemEntry item)
    {
        var itemName = item.GetName();
        foreach (var cmp in ItemList)
        {
            if (itemName == cmp.GetName())
                continue;

            Console.Error.WriteLine(string.Format("EventFlow already contains {0} item!", item.GetDisplayName()));
            return;
        }

        ItemList.Add(item);
    }
    public void RemoveItem(string name)
    {
        foreach (var item in ItemList)
        {
            if (item.GetName() == name)
            {
                ItemList.Remove(item);
                return;
            }
        }
    }
}

public class EventFlowException(string error) : Exception(error);