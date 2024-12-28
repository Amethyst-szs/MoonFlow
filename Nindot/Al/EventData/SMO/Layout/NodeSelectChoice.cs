using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeSelectChoice : Node
{
    public NodeSelectChoice(Dictionary<object, object> dict) : base(dict) { }
    public NodeSelectChoice(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeSelectChoice(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override bool IsAllowOutgoingEdges() { return true; }
    public override bool IsUseMultipleOutgoingEdges() { return true; }
    public override bool IsForceOutgoingEdgeCount() { return false; }
    public override int GetMaxOutgoingEdges() { return 4; }

    public override NodeOptionType GetNodeNameOptions(out string[] options)
    {
        options = [];
        return NodeOptionType.NO_OPTIONS;
    }
    public override NodeOptionType GetSupportedParams(out Dictionary<string, Type> paramInfo)
    {
        paramInfo = new Dictionary<string, Type>() {
            { "Text", typeof(NodeMessageResolverData) },
            { "CancelIndex", typeof(int) },
            { "IsContinuePreSelectIndex", typeof(bool) },
        };
        return NodeOptionType.PRESET_LIST;
    }

    internal override bool TryWriteBuild(out Dictionary<string, object> build)
    {
        var result = base.TryWriteBuild(out build);
        if (!result) return false;

        if (!build.ContainsKey("CaseEventList")) return false;

        var list = build["CaseEventList"] as List<Dictionary<string, object>>;

        var newList = new List<Dictionary<string, object>>();
        var index = 0;

        foreach (var item in list)
        {
            if (!item.TryGetValue("NextId", out object next))
                return false;
            
            var nextI = (int)next;
            
            if (nextI != int.MinValue)
            {
                item["Index"] = index;
                newList.Add(item);

                index++;
                continue;
            }
        }

        build["CaseEventList"] = newList;
        return true;
    }
}