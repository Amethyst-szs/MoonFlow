using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeCheckCostumePair : Node
{
    public NodeCheckCostumePair(Dictionary<object, object> dict) : base(dict) { }
    public NodeCheckCostumePair(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeCheckCostumePair(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override bool IsAllowOutgoingEdges() { return true; }
    public override bool IsUseMultipleOutgoingEdges() { return true; }
    public override int GetMaxOutgoingEdges() { return 2; }

    public override NodeOptionType GetNodeNameOptions(out string[] options)
    {
        options = [];
        return NodeOptionType.NO_OPTIONS;
    }
    public override NodeOptionType GetSupportedParams(out Dictionary<string, Type> paramInfo)
    {
        paramInfo = new Dictionary<string, Type>() {
            { "PatternName", typeof(string) },
        };
        return NodeOptionType.PRESET_LIST;
    }
}