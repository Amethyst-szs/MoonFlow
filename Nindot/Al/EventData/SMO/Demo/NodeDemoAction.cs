using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeDemoAction : Node
{
    public NodeDemoAction(Dictionary<object, object> dict) : base(dict) { }
    public NodeDemoAction(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeDemoAction(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override NodeOptionType GetNodeNameOptions(out string[] options)
    {
        options = [];
        return NodeOptionType.NO_OPTIONS;
    }
    public override NodeOptionType GetSupportedParams(out Dictionary<string, Type> paramInfo)
    {
        paramInfo = new Dictionary<string, Type>() {
            { "ActionName", typeof(string) },
            { "AudioEventName", typeof(string) },
            { "IsResetPlayerDynamics", typeof(bool) },
        };
        return NodeOptionType.PRESET_LIST;
    }
}