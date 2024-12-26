using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow;

public abstract class Node
{
    // ====================================================== //
    // ============ Parameters and Initilization ============ //
    // ====================================================== //

    public enum NodeOptionType : int
    {
        NO_OPTIONS,
        PRESET_LIST,
        ANY_VALUE,
    }

    public string Name = null;
    public string TypeBase { get; protected set; } = null;

    public int Id { get; protected set; } = int.MinValue;
    protected int NextId { get; private set; } = int.MinValue;

    public NodeParams Params { get; protected set; } = null;
    public NodeCaseEventList CaseEventList = null;

    public Node(Dictionary<object, object> dict)
    {
        // Assign node type and id
        if (dict.ContainsKey("Type")) Name = (string)dict["Type"];
        if (dict.ContainsKey("Base")) TypeBase = (string)dict["Base"];
        if (dict.ContainsKey("Id")) Id = (int)dict["Id"];

        // Copy the value in Type to Base if base wasn't definied in iter
        TypeBase ??= Name;

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
        Name = factoryType;
        TypeBase ??= Name;

        Params = [];
        
        if (IsUseMultipleOutgoingEdges())
            CaseEventList = new();
    }
    public Node(Graph graph, string factoryName, string name)
    {
        Id = graph.GetNextUnusedNodeId();
        TypeBase = factoryName;
        Name = name;

        if (GetSupportedParams(out _) != NodeOptionType.NO_OPTIONS)
            Params = [];
        if (IsUseMultipleOutgoingEdges())
            CaseEventList = new();
    }

    public virtual T Clone<T>() where T : Node
    {
        return ObjectExtensions.Copy((T)this);
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

        if (Name == null) return false;
        build["Type"] = Name;

        if (TypeBase != Name) build["Base"] = TypeBase;

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
    public virtual bool IsForceOutgoingEdgeCount() { return true; }
    public virtual int GetMaxOutgoingEdges() { return 1; }

    public abstract NodeOptionType GetNodeNameOptions(out string[] options);
    public abstract NodeOptionType GetSupportedParams(out Dictionary<string, Type> paramInfo);

    // ====================================================== //
    // ================== Reading Utilities ================= //
    // ====================================================== //

    public bool IsNodeOrphanSolo(Graph graph) { return graph.IsNodeOrphanSolo(this); }

    public int GetId() { return Id; }
    public virtual int[] GetNextIds()
    {
        if (!IsAllowOutgoingEdges())
            return [];
        
        if (!IsUseMultipleOutgoingEdges() || CaseEventList == null)
            return [NextId];

        return CaseEventList.GetCaseNextIdList();
    }
    public virtual int GetNextIdCount()
    {
        if (!IsAllowOutgoingEdges())
            return 0;
        
        if (!IsUseMultipleOutgoingEdges() || CaseEventList == null)
            return 1;

        return CaseEventList.GetCaseCount();
    }
    public string GetFactoryType()
    {
        if (TypeBase != null) return TypeBase;
        if (Name != null) return Name;
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
        
        if (next == null)
            NextId = int.MinValue;
        else
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
    public void SetName(int nameOptionIndex)
    {
        NodeOptionType type = GetNodeNameOptions(out string[] list);
        if (nameOptionIndex < 0 || nameOptionIndex > list.Length) return;
        if (type == NodeOptionType.PRESET_LIST)
            Name = list[nameOptionIndex];
    }
    public void SetName(string name)
    {
        NodeOptionType type = GetNodeNameOptions(out string[] list);
        if (type == NodeOptionType.ANY_VALUE)
            Name = name;
    }

    public void SetIdUnsafe(int id)
    {
        Id = id;
    }

    // ====================================================== //
    // ================= Parameter Utilities ================ //
    // ====================================================== //

    public bool IsParamDefined(string param)
    {
        return Params.ContainsKey(param);
    }
    public bool TryGetParamType(string param, out Type value)
    {
        NodeOptionType paramInfoType = GetSupportedParams(out Dictionary<string, Type> paramInfo);
        if (paramInfoType == NodeOptionType.PRESET_LIST)
        {
            value = paramInfo[param];
            return true;
        }

        value = null;
        return false;
    }
    public bool TryGetParam<T>(string param, out T value)
    {
        // Setup default value for out
        value = default;

        // Get the param info of this node
        NodeOptionType paramInfoType = GetSupportedParams(out Dictionary<string, Type> paramInfo);

        // If this node type doesn't support having params, return early
        if (paramInfoType == NodeOptionType.NO_OPTIONS)
            return false;

        // If this node type can have any param, directly access Params instead of paramInfo
        if (paramInfoType == NodeOptionType.ANY_VALUE)
        {
            if (!Params.TryGetValue(param, out object data))
                return false;

            value = (T)data;
            return true;
        }

        // This node type must set params from the preset list, check that this param is in the list
        if (!paramInfo.TryGetValue(param, out Type requiredType))
            return false;

        // Ensure the provided type T matches the supported entry
        if (requiredType != typeof(T))
            return false;

        // Ensure that the Params dictionary contains the param requested
        if (!Params.TryGetValue(param, out object v))
            return false;

        value = (T)v;
        return true;
    }

    public bool TrySetParam<T>(string param, T value)
    {
        // Get the param info of this node
        NodeOptionType paramInfoType = GetSupportedParams(out Dictionary<string, Type> paramInfo);

        // If this node type doesn't support having params, return early
        if (paramInfoType == NodeOptionType.NO_OPTIONS)
            return false;

        // If this node type can have any param, directly access Params instead of paramInfo
        if (paramInfoType == NodeOptionType.ANY_VALUE)
        {
            Params[param] = value;
            return true;
        }

        // This node type must set params from the preset list, check that this param is in the list
        if (!paramInfo.TryGetValue(param, out Type requiredType))
            return false;

        // Ensure the provided type T matches the supported entry
        if (requiredType != typeof(T))
            return false;

        Params[param] = value;
        return true;
    }
    public bool TrySetParamMessageData(string param, NodeMessageResolverData data)
    {
        // Get the param info of this node
        NodeOptionType paramInfoType = GetSupportedParams(out Dictionary<string, Type> paramInfo);

        // If this node type doesn't support having params, return early
        if (paramInfoType == NodeOptionType.NO_OPTIONS)
            return false;

        // If this node type can have any param, directly access Params instead of paramInfo
        if (paramInfoType == NodeOptionType.ANY_VALUE)
        {
            Params[param] = data;
            return true;
        }

        // This node type must set params from the preset list, check that this param is in the list
        if (!paramInfo.TryGetValue(param, out Type requiredType))
            return false;

        // Ensure the provided type T matches the supported entry
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