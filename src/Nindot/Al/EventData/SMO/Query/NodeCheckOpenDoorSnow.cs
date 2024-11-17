using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeCheckOpenDoorSnow : Node
{
    public NodeCheckOpenDoorSnow(Dictionary<object, object> dict) : base(dict) { }
    public NodeCheckOpenDoorSnow(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeCheckOpenDoorSnow(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override bool IsUseMultipleOutgoingEdges() { return true; }
    public override bool IsAllowOutgoingEdges() { return true; }
    public override int GetMaxOutgoingEdges() { return 5; } // Each edge index is the number of open doors

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
}