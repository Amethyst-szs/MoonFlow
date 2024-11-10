using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodePopLinkShine : Node
{
    public NodePopLinkShine(Dictionary<object, object> dict) : base(dict) { }
    public NodePopLinkShine(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodePopLinkShine(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override NodeOptionType GetNodeNameOptions(out string[] options)
    {
        options = [];
        return NodeOptionType.NO_OPTIONS;
    }
    public override NodeOptionType GetSupportedParams(out Dictionary<string, Type> paramInfo)
    {
        paramInfo = new Dictionary<string, Type>() {
            { "LinkName", typeof(string) }, // Defaults to ShineActor if not defined
        };
        return NodeOptionType.PRESET_LIST;
    }
}