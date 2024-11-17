using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeActionOneTime : Node
{
    public NodeActionOneTime(Dictionary<object, object> dict) : base(dict) { }
    public NodeActionOneTime(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeActionOneTime(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override NodeOptionType GetNodeNameOptions(out string[] options)
    {
        options = [];
        return NodeOptionType.NO_OPTIONS;
    }
    public override NodeOptionType GetSupportedParams(out Dictionary<string, Type> paramInfo)
    {
        paramInfo = new Dictionary<string, Type>() {
            { "ActionName", typeof(string) },
            { "ActionFrameRate", typeof(float) },
        };
        return NodeOptionType.PRESET_LIST;
    }
}