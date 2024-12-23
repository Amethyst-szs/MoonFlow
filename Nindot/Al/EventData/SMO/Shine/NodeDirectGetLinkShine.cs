using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeDirectGetLinkShine : Node
{
    public NodeDirectGetLinkShine(Dictionary<object, object> dict) : base(dict) { }
    public NodeDirectGetLinkShine(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeDirectGetLinkShine(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override NodeOptionType GetNodeNameOptions(out string[] options)
    {
        options = [
            "DirectGetLinkShine",
            "DirectGetLinkShineTimeBalloon",
        ];
        return NodeOptionType.PRESET_LIST;
    }
    public override NodeOptionType GetSupportedParams(out Dictionary<string, Type> paramInfo)
    {
        paramInfo = new Dictionary<string, Type>() {
            { "LinkName", typeof(string) }, // Defaults to "ShineActor"
            { "IsMiniGame", typeof(bool) },
        };
        return NodeOptionType.PRESET_LIST;
    }
}