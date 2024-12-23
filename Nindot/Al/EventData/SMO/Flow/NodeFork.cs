using System;
using System.Collections.Generic;
using System.Linq;

namespace Nindot.Al.EventFlow.Smo;

public class NodeFork : Node
{
    // ====================================================== //
    // ====== Initilization and Standard Virtual Config ===== //
    // ====================================================== //

    public List<int> NextIdList = [];

    public NodeFork(Dictionary<object, object> dict) : base(dict)
    {
        ResetIdList();
        if (!dict.ContainsKey("NextIdList"))
            return;

        var list = (List<object>)dict["NextIdList"];
        for (int i = 0; i < list.Count; i++)
        {
            NextIdList.Add((int)list[i]);
        }
    }
    public NodeFork(Graph graph, string factoryType) : base(graph, factoryType)
    {
        ResetIdList();
    }
    public NodeFork(Graph graph, string typeBase, string type) : base(graph, typeBase, type)
    {
        ResetIdList();
    }

    public override bool IsAllowOutgoingEdges() { return true; }
    public override bool IsUseMultipleOutgoingEdges() { return true; }
    public override bool IsForceOutgoingEdgeCount() { return false; }

    public override NodeOptionType GetNodeNameOptions(out string[] options)
    {
        options = [];
        return NodeOptionType.NO_OPTIONS;
    }
    public override NodeOptionType GetSupportedParams(out Dictionary<string, Type> paramInfo)
    {
        paramInfo = [];
        return NodeOptionType.NO_OPTIONS;
    }

    // ====================================================== //
    // ================ Additional Overrides ================ //
    // ====================================================== //

    internal override bool TryWriteBuild(out Dictionary<string, object> build)
    {
        if (!base.TryWriteBuild(out build))
            return false;

        build["NextIdList"] = NextIdList;
        return true;
    }

    public override int[] GetNextIds() { return [.. NextIdList]; }
    public override int GetNextIdCount() { return NextIdList.Count; }
    public override Node GetNextNode(Graph graph) { return null; }
    public override Node GetNextNode(Graph graph, int edgeIndex)
    {
        if (edgeIndex < 0 || edgeIndex > NextIdList.Count)
            return null;

        return graph.GetNode(NextIdList[edgeIndex]);
    }
    public override void RemoveNextNode() { return; }
    public override void RemoveNextNode(int edgeIndex)
    {
        if (edgeIndex < 0 || edgeIndex > NextIdList.Count)
            return;

        NextIdList[edgeIndex] = int.MinValue;
    }

    public override bool TrySetNextNode(Node next) { return false; }
    public override bool TrySetNextNode(Node next, int edgeIndex)
    {
        if (edgeIndex < 0 || edgeIndex > NextIdList.Count)
            return false;

        NextIdList[edgeIndex] = next.GetId();
        return true;
    }

    // ====================================================== //
    // ================ Additional Utilities ================ //
    // ====================================================== //

    private void ResetIdList()
    {
        for (int i = 0; i < NextIdList.Count; i++)
            NextIdList[i] = int.MinValue;
    }
}