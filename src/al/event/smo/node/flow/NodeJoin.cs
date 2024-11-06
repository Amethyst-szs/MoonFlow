using System;
using System.Collections.Generic;
using System.Linq;

namespace Nindot.Al.EventFlow.Smo;

public class NodeJoin : Node
{
    // ====================================================== //
    // ====== Initilization and Standard Virtual Config ===== //
    // ====================================================== //

    private int[] PreIdList = new int[2];

    public NodeJoin(Dictionary<object, object> dict) : base(dict)
    {
        ResetPreIdList();
        if (!dict.ContainsKey("PreIdList"))
            return;

        var list = (List<object>)dict["PreIdList"];
        for (int i = 0; i < list.Count; i++)
        {
            PreIdList[i] = (int)list[i];
        }
    }
    public NodeJoin(Graph graph, string factoryType) : base(graph, factoryType)
    {
        ResetPreIdList();
    }
    public NodeJoin(Graph graph, string typeBase, string type) : base(graph, typeBase, type)
    {
        ResetPreIdList();
    }

    public override bool IsAllowOutgoingEdges() { return true; }
    public override bool IsUseMultipleOutgoingEdges() { return false; }
    public override int GetMinOutgoingEdges() { return 1; }
    public override int GetMaxOutgoingEdges() { return 1; }

    public override string[] GetNodeTypeOptions() { return []; }
    public override Dictionary<string, Type> GetSupportedParams() { return []; }

    // ====================================================== //
    // ================ Additional Overrides ================ //
    // ====================================================== //

    internal override bool TryWriteBuild(out Dictionary<string, object> build)
    {
        if (!base.TryWriteBuild(out build))
            return false;
        
        build["PreIdList"] = PreIdList.Clone();
        return true;
    }

    // ====================================================== //
    // ================ Additional Utilities ================ //
    // ====================================================== //

    private void ResetPreIdList()
    {
        for (int i = 0; i < PreIdList.Length; i++)
            PreIdList[i] = int.MinValue;
    }
}