using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeSwitchOn : Node
{
    public NodeSwitchOn(Dictionary<object, object> dict) : base(dict) { }
    public NodeSwitchOn(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeSwitchOn(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override NodeOptionType GetNodeNameOptions(out string[] options)
    {
        options = [];
        return NodeOptionType.ANY_VALUE;
    }
    public override NodeOptionType GetSupportedParams(out Dictionary<string, Type> paramInfo)
    {
        paramInfo = [];
        return NodeOptionType.NO_OPTIONS;
    }
}