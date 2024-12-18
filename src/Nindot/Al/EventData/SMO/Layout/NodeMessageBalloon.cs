using System;
using System.Collections.Generic;
using System.Linq;

namespace Nindot.Al.EventFlow.Smo;

public class NodeMessageBalloon : Node
{
    public NodeMessageBalloon(Dictionary<object, object> dict) : base(dict) { }
    public NodeMessageBalloon(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeMessageBalloon(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override int GetMaxOutgoingEdges()
    {
        if (Name.Contains("MultiDevide"))
            return 2;
        
        return 1;
    }
    public override bool IsUseMultipleOutgoingEdges()
    {
        return GetMaxOutgoingEdges() != 1;
    }

    public override NodeOptionType GetNodeNameOptions(out string[] options)
    {
        // Too many possible combinations to use a preset list
        // Base name is either TalkBalloon or IconBalloon
        // Can be followed with MapUnit, Tutorial, and MultiDevide
        options = [
            "MessageBalloon",
            "MessageBalloonMapUnit",
            "MessageBalloonMiniGame",

            "TalkBalloon",
            "TalkBalloonMapUnit",
            "TalkBalloonTutorial",

            "TalkBalloonMultiDevide",
            "TalkBalloonMultiDevideMapUnit",
            "TalkBalloonMultiDevideTutorial",

            "IconBalloon",
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
            { "EmotionType", typeof(string) },
            { "IsEnableTalkHacking", typeof(bool) },
            { "IsDisableTalkInWater", typeof(bool) },
            { "IsShowAlways", typeof(bool) },
            { "IsShowOnlyFaceToCameraFront", typeof(bool) },
            { "IsHideIfExistTutorial", typeof(bool) },
            { "IsInvalidUiCollisionCheck", typeof(bool) },
        }).ToDictionary();

        return NodeOptionType.PRESET_LIST;
    }
}