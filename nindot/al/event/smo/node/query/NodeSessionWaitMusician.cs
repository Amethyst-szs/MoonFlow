using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeSessionWaitMusician : Node
{
    public NodeSessionWaitMusician(Dictionary<object, object> dict) : base(dict) { }
    public NodeSessionWaitMusician(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeSessionWaitMusician(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

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
            { "Count", typeof(int) },
        };
        return NodeOptionType.PRESET_LIST;
    }
}