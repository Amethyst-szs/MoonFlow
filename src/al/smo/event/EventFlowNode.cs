using System;
using System.Collections.Generic;
using System.Linq;

namespace Nindot.Al.EventFlow;

public struct NodeParamInfo(string name, Type type)
{
    public string Name = name;
    public Type Type = type;
}

public class NodeBase
{
    // ====================================================== //
    // ============ Parameters and Initilization ============ //
    // ====================================================== //

    protected string Type = null;
    protected string TypeBase = null;

    protected int Id = int.MinValue;
    protected List<int> EdgeIds = [];

    protected Dictionary<object, object> Params = [];

    public NodeBase(Dictionary<object, object> dict)
    {
        if (dict.ContainsKey("Type")) Type = (string)dict["Type"];
        if (dict.ContainsKey("Base")) TypeBase = (string)dict["Base"];
        if (dict.ContainsKey("Id")) Id = (int)dict["Id"];
        if (dict.ContainsKey("NextId")) EdgeIds.Add((int)dict["NextId"]);

        if (dict.ContainsKey("Param")) Params = (Dictionary<object, object>)dict["Param"];
    }
    public NodeBase(Graph graph, string factoryType)
    {
        Id = graph.GetNextUnusedNodeId();
        Type = factoryType;
    }
    public NodeBase(Graph graph, string typeBase, string type)
    {
        Id = graph.GetNextUnusedNodeId();
        TypeBase = typeBase;
        Type = type;
    }

    // ====================================================== //
    // =============== Virtual Configurations =============== //
    // ====================================================== //

    public virtual bool IsHaveEdges() { return true; }
    public virtual int GetMinEdgeCount() { return 1; }
    public virtual int GetMaxEdgeCount() { return 1; }

    public virtual string[] GetValidTypeList() { return []; }
    public virtual NodeParamInfo[] GetParamInfo() { return []; }

    // ====================================================== //
    // ================== Reading Utilities ================= //
    // ====================================================== //

    public bool IsNodeOrphan(Graph graph) { return graph.IsNodeOrphan(this); }
    public bool IsRequireMultipleEdges() { return GetMaxEdgeCount() > 1 && GetMinEdgeCount() > 1; }

    public int GetId() { return Id; }
    public string GetFactoryType()
    {
        if (TypeBase != null) return TypeBase;
        if (Type != null) return Type;
        return null;
    }
    public NodeBase GetNextNode(Graph graph)
    {
        // You cannot access THE next node when multiple edges exist, or no edges exist
        if (IsRequireMultipleEdges() || EdgeIds.Count == 0)
            return null;
        
        if (graph.IsNodeIdValid(EdgeIds[0]))
            return graph.GetNode(EdgeIds[0]);

        return null;
    }
    public NodeBase GetNextNode(Graph graph, int edgeIndex)
    {
        // If this node doesn't have multiple edges, use standard utility
        if (!IsRequireMultipleEdges())
            return GetNextNode(graph);
        
        // Ensure edgeIndex is within the bounds of the edge count
        if (EdgeIds.Count >= edgeIndex)
            return null;
        
        if (graph.IsNodeIdValid(EdgeIds[edgeIndex]))
            return graph.GetNode(EdgeIds[edgeIndex]);

        return null;
    }
    public List<int> GetNextNodeList() { return EdgeIds; }
    public List<NodeBase> GetNextNodeList(Graph graph)
    {
        List<NodeBase> list = [];

        foreach (var id in EdgeIds)
        {
            if (graph.IsNodeIdValid(id))
                list.Add(graph.GetNode(id));
        }

        return list;
    }
    public int GetNextNodeCount() { return EdgeIds.Count; }

    // ====================================================== //
    // ================== Editing Utilities ================= //
    // ====================================================== //

    public bool TrySetNextNode(NodeBase next)
    {
        // You cannot set THE next node when multiple edges exist
        if (IsRequireMultipleEdges())
            return false;
        
        EdgeIds.Clear();
        EdgeIds.Add(next.Id);
        return true;
    }
    public bool TrySetNextNode(NodeBase next, int edgeIndex)
    {
        // If this node doesn't use multiple edges, use standard utility
        if (!IsRequireMultipleEdges())
            return TrySetNextNode(next);
        
        // Ensure the edge index is below the max edge count
        if (edgeIndex >= GetMaxEdgeCount())
            return false;
        
        // Ensure the EdgeIds list is large enough to store this value
        while (EdgeIds.Count < edgeIndex)
        {
            EdgeIds.Add(int.MinValue);
        }
        
        EdgeIds[edgeIndex] = next.Id;
        return true;
    }

    public void RemoveNextNode()
    {
        // You cannot remove THE next node when multiple edges exist
        if (IsRequireMultipleEdges())
            return;
        
        EdgeIds.Clear();
    }
    public void RemoveNextNode(int edgeIndex)
    {
        // If this node doesn't use multiple edges, use standard utility
        if (!IsRequireMultipleEdges())
        {
            RemoveNextNode();
            return;
        }
        
        // Ensure the edge index is below the max edge count
        if (edgeIndex >= GetMaxEdgeCount())
            return;
        
        // Ensure the EdgeIds list is large enough to store this value
        while (EdgeIds.Count < edgeIndex)
        {
            EdgeIds.Add(int.MinValue);
        }
        
        EdgeIds[edgeIndex] = int.MinValue;
    }

    public void ReassignId(Graph graph)
    {
        Id = graph.GetNextUnusedNodeId();
    }
    public void SetTypeWithoutChangingBaseType(string newType)
    {
        TypeBase ??= Type;
        Type = newType;
    }
}