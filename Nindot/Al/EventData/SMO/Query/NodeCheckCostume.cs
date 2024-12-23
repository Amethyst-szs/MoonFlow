using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeCheckCostume : Node
{
    public NodeCheckCostume(Dictionary<object, object> dict) : base(dict) { }
    public NodeCheckCostume(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeCheckCostume(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override bool IsAllowOutgoingEdges() { return true; }
    public override bool IsUseMultipleOutgoingEdges() { return true; }
    public override int GetMaxOutgoingEdges() { return 2; }

    public override NodeOptionType GetNodeNameOptions(out string[] options)
    {
        options = [
            "CheckCostumeCostume",
            "CheckCostumePattern",
            "CheckCostumeMapUnit",
        ];
        return NodeOptionType.PRESET_LIST;
    }
    public override NodeOptionType GetSupportedParams(out Dictionary<string, Type> paramInfo)
    {
        paramInfo = new Dictionary<string, Type>() {
            { "CostumeName", typeof(string) },
            { "PatternName", typeof(string) },
        };
        return NodeOptionType.PRESET_LIST;
    }
}