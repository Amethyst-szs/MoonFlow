using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeHitReaction : Node
{
    public NodeHitReaction(Dictionary<object, object> dict) : base(dict) { }
    public NodeHitReaction(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeHitReaction(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override NodeOptionType GetNodeNameOptions(out string[] options)
    {
        options = [];
        return NodeOptionType.NO_OPTIONS;
    }
    public override NodeOptionType GetSupportedParams(out Dictionary<string, Type> paramInfo)
    {
        paramInfo = new Dictionary<string, Type>() {
            { "HitReactionName", typeof(string) },
            { "WaitStep", typeof(int) },
        };
        return NodeOptionType.PRESET_LIST;
    }
}