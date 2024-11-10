using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeSelectChoice : Node
{
    public NodeSelectChoice(Dictionary<object, object> dict) : base(dict) { }
    public NodeSelectChoice(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeSelectChoice(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override bool IsAllowOutgoingEdges() { return true; }
    public override bool IsUseMultipleOutgoingEdges() { return true; }
    public override bool IsForceOutgoingEdgeCount() { return false; }
    public override int GetMaxOutgoingEdges() { return 4; }

    public override NodeOptionType GetNodeNameOptions(out string[] options)
    {
        options = [];
        return NodeOptionType.NO_OPTIONS;
    }
    public override NodeOptionType GetSupportedParams(out Dictionary<string, Type> paramInfo)
    {
        paramInfo = new Dictionary<string, Type>() {
            { "Text", typeof(NodeMessageResolverData) },
            { "CancelIndex", typeof(int) },
            { "IsContinuePreSelectIndex", typeof(bool) },
        };
        return NodeOptionType.PRESET_LIST;
    }
}