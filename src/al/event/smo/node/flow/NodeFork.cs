using System;
using System.Collections.Generic;
using System.Linq;

namespace Nindot.Al.EventFlow.Smo;

public class NodeFork : Node
{
    // ====================================================== //
    // ====== Initilization and Standard Virtual Config ===== //
    // ====================================================== //

    private int[] NextIdList = new int[2];

    public NodeFork(Dictionary<object, object> dict) : base(dict)
    {
        ResetIdList();
        if (!dict.ContainsKey("NextIdList"))
            return;

        var list = (List<object>)dict["NextIdList"];
        for (int i = 0; i < list.Count; i++)
        {
            NextIdList[i] = (int)list[i];
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
    public override int GetMinOutgoingEdges() { return 2; }
    public override int GetMaxOutgoingEdges() { return 2; }

    public override string[] GetNodeTypeOptions() { return []; }
    public override Dictionary<string, Type> GetSupportedParams() { return []; }

    // ====================================================== //
    // ================ Additional Overrides ================ //
    // ====================================================== //

    internal override bool TryWriteBuild(out Dictionary<string, object> build)
    {
        if (!base.TryWriteBuild(out build))
            return false;
        
        build["NextIdList"] = NextIdList.Clone();
        return true;
    }

    public override int[] GetNextIds() { return NextIdList; }
    public override int GetNextIdCount() {return NextIdList.Length; }
    public override Node GetNextNode(Graph graph) { return null; }
    public override Node GetNextNode(Graph graph, int edgeIndex)
    {
        if (edgeIndex < 0 || edgeIndex > NextIdList.Length)
            return null;
        
        return graph.GetNode(NextIdList[edgeIndex]);
    }
    public override void RemoveNextNode() { return; }
    public override void RemoveNextNode(int edgeIndex)
    {
        if (edgeIndex < 0 || edgeIndex > NextIdList.Length)
            return;
        
        NextIdList[edgeIndex] = int.MinValue;
    }

    public override bool TrySetNextNode(Node next) { return false; }
    public override bool TrySetNextNode(Node next, int edgeIndex)
    {
        if (edgeIndex < 0 || edgeIndex > NextIdList.Length)
            return false;
        
        NextIdList[edgeIndex] = next.GetId();
        return true;
    }

    // ====================================================== //
    // ================ Additional Utilities ================ //
    // ====================================================== //

    private void ResetIdList()
    {
        for (int i = 0; i < NextIdList.Length; i++)
            NextIdList[i] = int.MinValue;
    }
}