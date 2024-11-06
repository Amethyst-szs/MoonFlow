using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeTurnToPlayerActionOneTime : Node
{
    public NodeTurnToPlayerActionOneTime(Dictionary<object, object> dict) : base(dict) { }
    public NodeTurnToPlayerActionOneTime(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeTurnToPlayerActionOneTime(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override NodeOptionType GetNodeNameOptions(out string[] options)
    {
        options = [];
        return NodeOptionType.NO_OPTIONS;
    }
    public override NodeOptionType GetSupportedParams(out Dictionary<string, Type> paramInfo)
    {
        paramInfo = new Dictionary<string, Type>() {
            { "ActionName", typeof(string) },
        };
        return NodeOptionType.PRESET_LIST;
    }
}