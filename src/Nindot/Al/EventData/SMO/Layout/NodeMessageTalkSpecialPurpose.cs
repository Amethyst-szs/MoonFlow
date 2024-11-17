using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeMessageTalkSpecialPurpose : Node
{
    public NodeMessageTalkSpecialPurpose(Dictionary<object, object> dict) : base(dict) { }
    public NodeMessageTalkSpecialPurpose(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeMessageTalkSpecialPurpose(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override NodeOptionType GetNodeNameOptions(out string[] options)
    {
        options = [
            "MessageTalkPeachNextHint",
        ]; 

        return NodeOptionType.PRESET_LIST;
    }
    public override NodeOptionType GetSupportedParams(out Dictionary<string, Type> paramInfo)
    {
        paramInfo = [];
        return NodeOptionType.NO_OPTIONS;
    }
}