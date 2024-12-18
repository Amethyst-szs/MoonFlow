using System;
using System.Collections.Generic;
using System.Linq;

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
        if (!Name.Contains("MapUnit"))
        {
            paramInfo = new Dictionary<string, Type>() {
                { "Text", typeof(NodeMessageResolverData) },
            };
        }
        else
        {
            paramInfo = new Dictionary<string, Type>() {
                { "ParameterName", typeof(string) },
            };
        }

        paramInfo = paramInfo.Concat(new Dictionary<string, Type>() {
            { "StartActionName", typeof(string) },
            { "LoopActionName", typeof(string) },
            { "DemoActionName", typeof(string) },
            { "StartActionFrameRate", typeof(float) },
            { "LoopActionFrameRate", typeof(float) },
            { "IconAppearDelayStep", typeof(int) },
        }).ToDictionary();

        return NodeOptionType.PRESET_LIST;
    }
}