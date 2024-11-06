using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeJumpEntry : Node
{
    // ====================================================== //
    // ====== Initilization and Standard Virtual Config ===== //
    // ====================================================== //

    private string JumpEntryName = null;

    public NodeJumpEntry(Dictionary<object, object> dict) : base(dict)
    {
        if (!dict.ContainsKey("JumpEntryName"))
            return;
        
        JumpEntryName = (string)dict["JumpEntryName"];
    }

    public NodeJumpEntry(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeJumpEntry(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override bool IsAllowOutgoingEdges() { return false; }
    public override bool IsUseMultipleOutgoingEdges() { return false; }
    public override int GetMinOutgoingEdges() { return 0; }
    public override int GetMaxOutgoingEdges() { return 0; }

    public override string[] GetNodeTypeOptions() { return []; }
    public override Dictionary<string, Type> GetSupportedParams() { return []; }

    // ====================================================== //
    // ================ Additional Overrides ================ //
    // ====================================================== //

    internal override bool TryWriteBuild(out Dictionary<string, object> build)
    {
        if (!base.TryWriteBuild(out build))
            return false;
        
        build["JumpEntryName"] = JumpEntryName;
        return true;
    }
}