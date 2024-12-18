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

    public override bool IsAllowOutgoingEdges() { return true; }
    public override bool IsUseMultipleOutgoingEdges() { return false; }
    public override int GetMaxOutgoingEdges() { return 1; }

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

        build["JumpEntryName"] = JumpEntryName;
        return true;
    }
}