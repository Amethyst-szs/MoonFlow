using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeGetSearchAmiibo : Node
{
    public NodeGetSearchAmiibo(Dictionary<object, object> dict) : base(dict) { }
    public NodeGetSearchAmiibo(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeGetSearchAmiibo(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override bool IsUseMultipleOutgoingEdges() { return true; }
    public override bool IsAllowOutgoingEdges() { return true; }
    public override int GetMaxOutgoingEdges() { return 4; }

    public override NodeOptionType GetNodeNameOptions(out string[] options)
    {
        options = [];
        return NodeOptionType.NO_OPTIONS;
    }
    public override NodeOptionType GetSupportedParams(out Dictionary<string, Type> paramInfo)
    {
        paramInfo = new Dictionary<string, Type>() {
            { "Type", typeof(string) },
        };
        return NodeOptionType.PRESET_LIST;
    }
}