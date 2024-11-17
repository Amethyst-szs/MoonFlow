using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeCheckGetLinkShine : Node
{
    public NodeCheckGetLinkShine(Dictionary<object, object> dict) : base(dict) { }
    public NodeCheckGetLinkShine(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeCheckGetLinkShine(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

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
            { "LinkName", typeof(string) }, // Will use "ShineActor" if not defined
        };
        return NodeOptionType.PRESET_LIST;
    }
}