using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeBgmCtrl : Node
{
    public NodeBgmCtrl(Dictionary<object, object> dict) : base(dict) { }
    public NodeBgmCtrl(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeBgmCtrl(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override NodeOptionType GetNodeNameOptions(out string[] options)
    {
        options = [];
        return NodeOptionType.NO_OPTIONS;
    }
    public override NodeOptionType GetSupportedParams(out Dictionary<string, Type> paramInfo)
    {
        paramInfo = new Dictionary<string, Type>() {
            { "BgmPlayName", typeof(string) },
            { "BgmSituationName", typeof(string) },
            { "IsBgmPlayStop", typeof(bool) },
            { "IsBgmSituationEnd", typeof(bool) },
            { "StartDelayFrameNum", typeof(int) },
        };
        return NodeOptionType.PRESET_LIST;
    }
}