using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeCapMessage : Node
{
    public NodeCapMessage(Dictionary<object, object> dict) : base(dict) { }
    public NodeCapMessage(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeCapMessage(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override NodeOptionType GetNodeNameOptions(out string[] options)
    {
        options = [];
        return NodeOptionType.NO_OPTIONS;
    }
    public override NodeOptionType GetSupportedParams(out Dictionary<string, Type> paramInfo)
    {
        paramInfo = new Dictionary<string, Type>() {
            { "Text", typeof(NodeMessageResolverData) },
            { "WaitStep", typeof(int) },
            { "IsAppearInfinity", typeof(bool) },
        };
        return NodeOptionType.PRESET_LIST;
    }
}