using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Godot;
using Nindot.Byml;
using System.Collections.ObjectModel;

namespace Nindot.Al.EventFlow;

public partial class Graph
{
    protected Dictionary<string, Node> EntryPoints = [];
    protected Dictionary<int, Node> Nodes = [];
    protected List<ItemEntry> ItemList = [];

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
    public List<Node> GetNodeList()
    {
        return [.. Nodes.Values];
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
    public List<Node> GetEntryPointNodes()
    {
        return [.. EntryPoints.Values];
    }

    public ReadOnlyCollection<ItemEntry> GetItemList()
    {
        return new ReadOnlyCollection<ItemEntry>(ItemList);
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
    public void RemoveNode(Node node)
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
            if (itemName != cmp.GetName())
                continue;
            
            GD.PushWarning("Cannot add two items with the same name/type, ignoring");
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