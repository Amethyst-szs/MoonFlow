using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow;

public abstract class Node
{
    // ====================================================== //
    // ============ Parameters and Initilization ============ //
    // ====================================================== //

    protected string Type = null;
    protected string TypeBase = null;

    protected int Id = int.MinValue;
    protected int NextId = int.MinValue;

    protected NodeParams Params = null;
    public NodeCaseEventList CaseEventList = null;

    public Node(Dictionary<object, object> dict)
    {
        // Assign node type and id
        if (dict.ContainsKey("Type")) Type = (string)dict["Type"];
        if (dict.ContainsKey("Base")) TypeBase = (string)dict["Base"];
        if (dict.ContainsKey("Id")) Id = (int)dict["Id"];

        // Copy the value in Type to Base if base wasn't definied in iter
        TypeBase ??= Type;

        // If the node contains a NextId, assign that. Otherwise, attempt to use CaseEventList
        if (dict.ContainsKey("NextId")) NextId = (int)dict["NextId"];
        if (dict.ContainsKey("CaseEventList"))
        {
            List<object> list = (List<object>)dict["CaseEventList"];
            CaseEventList = new(list);
        }

        // If a params key exists, load it. Otherwise create an empty default
        if (dict.ContainsKey("Param")) Params = new((Dictionary<object, object>)dict["Param"]);
        else Params = [];
    }
    public Node(Graph graph, string factoryType)
    {
        Id = graph.GetNextUnusedNodeId();
        Type = factoryType;
    }
    public Node(Graph graph, string typeBase, string type)
    {
        Id = graph.GetNextUnusedNodeId();
        TypeBase = typeBase;
        Type = type;
    }

    // ====================================================== //
    // =========== Writing and Dictionary Packing =========== //
    // ====================================================== //

    internal virtual bool TryWriteBuild(out Dictionary<string, object> build)
    {
        // Setup build dictionary
        build = [];

        // Setup default common data
        if (Id == int.MinValue) return false;
        build["Id"] = Id;

        if (Type == null) return false;
        build["Type"] = Type;

        if (TypeBase != Type) build["Base"] = TypeBase;

        // Write in NextId or the CaseEventList
        if (CaseEventList != null) // If using a CaseEventList
            build["CaseEventList"] = CaseEventList.WriteBuild();
        else if (NextId != int.MinValue) // If using NextId
            build["NextId"] = NextId;
        
        // Write in parameters
        var paramBuild = Params.WriteBuild();
        if (paramBuild.Count != 0)
            build["Param"] = paramBuild;
        
        return true;
    }

    // ====================================================== //
    // ================ Virtual Configuration =============== //
    // ====================================================== //

    public virtual bool IsAllowOutgoingEdges() { return true; }
    public virtual bool IsUseMultipleOutgoingEdges() { return false; }
    public virtual int GetMinOutgoingEdges() { return 1; }
    public virtual int GetMaxOutgoingEdges() { return 1; }

    public abstract string[] GetNodeTypeOptions();
    public abstract Dictionary<string, Type> GetSupportedParams();

    // ====================================================== //
    // ================== Reading Utilities ================= //
    // ====================================================== //

    public bool IsNodeOrphanSolo(Graph graph) { return graph.IsNodeOrphanSolo(this); }

    public int GetId() { return Id; }
    public virtual int[] GetNextIds()
    {
        if (!IsUseMultipleOutgoingEdges() || CaseEventList == null)
            return [NextId];

        return CaseEventList.GetCaseNextIdList();
    }
    public virtual int GetNextIdCount()
    {
        if (!IsUseMultipleOutgoingEdges() || CaseEventList == null)
            return 1;
        
        return CaseEventList.GetCaseCount();
    }
    public string GetFactoryType()
    {
        if (TypeBase != null) return TypeBase;
        if (Type != null) return Type;
        return null;
    }

    public virtual Node GetNextNode(Graph graph)
    {
        // You cannot access THE next node when multiple edges exist, or no edges exist
        if (IsUseMultipleOutgoingEdges())
            return null;

        if (graph.IsNodeIdValid(NextId))
            return graph.GetNode(NextId);

        return null;
    }
    public virtual Node GetNextNode(Graph graph, int edgeIndex)
    {
        // If this node doesn't have multiple edges, use standard utility
        if (!IsUseMultipleOutgoingEdges() || CaseEventList == null)
            return GetNextNode(graph);

        // Ensure edgeIndex is within the bounds of the edge count
        if (CaseEventList.GetCaseCount() >= edgeIndex)
            return null;

        int nextId = CaseEventList.GetCaseNextId(edgeIndex);
        if (graph.IsNodeIdValid(nextId))
            return graph.GetNode(nextId);

        return null;
    }

    // ====================================================== //
    // ================== Editing Utilities ================= //
    // ====================================================== //

    public virtual bool TrySetNextNode(Node next)
    {
        // You cannot set THE next node when multiple edges exist or if edges are disabled
        if (IsUseMultipleOutgoingEdges() || !IsAllowOutgoingEdges())
            return false;

        NextId = next.Id;
        return true;
    }
    public virtual bool TrySetNextNode(Node next, int edgeIndex)
    {
        // If this node doesn't use multiple edges, use standard utility
        if (!IsUseMultipleOutgoingEdges() || CaseEventList == null)
            return TrySetNextNode(next);

        // Ensure this node type supports having edges
        if (!IsAllowOutgoingEdges())
            return false;

        // Ensure the edge index is below the max edge count
        if (edgeIndex >= GetMaxOutgoingEdges())
            return false;

        CaseEventList.SetNextNodeForCase(next, edgeIndex);
        return true;
    }

    public virtual void RemoveNextNode()
    {
        // You cannot remove THE next node when multiple edges exist
        if (IsUseMultipleOutgoingEdges())
            return;

        NextId = int.MinValue;
    }
    public virtual void RemoveNextNode(int edgeIndex)
    {
        // If this node doesn't use multiple edges, use standard utility
        if (!IsUseMultipleOutgoingEdges() || CaseEventList == null)
        {
            RemoveNextNode();
            return;
        }

        // Ensure the edge index is below the max edge count
        if (edgeIndex >= GetMaxOutgoingEdges())
            return;

        CaseEventList.RemoveNextNodeForCase(edgeIndex);
    }

    public int ReassignId(Graph graph)
    {
        Id = graph.GetNextUnusedNodeId();
        return Id;
    }
    public void SetType(int typeOptionIndex)
    {
        string[] list = GetNodeTypeOptions();
        if (typeOptionIndex < 0 || typeOptionIndex > list.Length)
            return;

        Type = list[typeOptionIndex];
    }

    // ====================================================== //
    // ================= Parameter Utilities ================ //
    // ====================================================== //

    public bool IsParamDefined(string param)
    {
        return Params.ContainsKey(param);
    }
    public bool TryGetParam<T>(string param, out T value)
    {
        // Setup default value for out
        value = default;

        // Ensure this param name is supported by this node
        var supportList = GetSupportedParams();
        if (!supportList.TryGetValue(param, out Type requiredType))
            return false;

        // Ensure the provided type T matches the supported entry
        if (requiredType != typeof(T))
            return false;

        value = (T)Params[param];
        return true;
    }

    public bool TrySetParam<T>(string param, T value)
    {
        // Ensure this param name is supported by this node
        var supportList = GetSupportedParams();
        if (!supportList.TryGetValue(param, out Type requiredType))
            return false;

        // Ensure the provided type T matches the supported entry
        if (requiredType != typeof(T))
            return false;

        Params[param] = value;
        return true;
    }
    public bool TrySetParamMessageData(string param, NodeMessageResolverData data)
    {
        // Ensure this param name is supported by this node
        var supportList = GetSupportedParams();
        if (!supportList.TryGetValue(param, out Type requiredType))
            return false;

        // Ensure the requiredType value is equal to NodeMessageResolverData
        if (requiredType != typeof(NodeMessageResolverData))
            return false;

        Params[param] = data;
        return true;
    }

    public bool TryRemoveParam(string param)
    {
        return Params.Remove(param);
    }
}