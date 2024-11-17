using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeMessageTalk : Node
{
    public NodeMessageTalk(Dictionary<object, object> dict) : base(dict) { }
    public NodeMessageTalk(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeMessageTalk(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override NodeOptionType GetNodeNameOptions(out string[] options)
    {
        options = [
            "MessageTalk",
            "MessageTalkMapUnit",
            "MessageTalkTutorial",

            "MessageTalkDemo",
            "MessageTalkDemoMapUnit",

            "MessageTalkSystem",
            "MessageTalkInvalidIconA",
            "MessageTalkPeachNextHint",
        ]; 

        return NodeOptionType.PRESET_LIST;
    }
    public override NodeOptionType GetSupportedParams(out Dictionary<string, Type> paramInfo)
    {
        paramInfo = new Dictionary<string, Type>() {
            { "Text", typeof(NodeMessageResolverData) }, // For non-MapUnit
            { "ParameterName", typeof(string) }, // For MapUnit
            { "DemoActionName", typeof(string) },
            { "StartActionName", typeof(string) },
            { "StartActionFrameRate", typeof(float) },
            { "LoopActionName", typeof(string) },
            { "LoopActionFrameRate", typeof(float) },
            { "IconAppearDelayStep", typeof(int) },
        };
        return NodeOptionType.PRESET_LIST;
    }
}