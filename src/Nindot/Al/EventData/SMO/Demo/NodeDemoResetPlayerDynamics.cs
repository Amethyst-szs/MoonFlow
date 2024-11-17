using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeDemoResetPlayerDynamics : Node
{
    public NodeDemoResetPlayerDynamics(Dictionary<object, object> dict) : base(dict) { }
    public NodeDemoResetPlayerDynamics(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeDemoResetPlayerDynamics(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override NodeOptionType GetNodeNameOptions(out string[] options)
    {
        options = [];
        return NodeOptionType.NO_OPTIONS;
    }
    public override NodeOptionType GetSupportedParams(out Dictionary<string, Type> paramInfo)
    {
        paramInfo = new Dictionary<string, Type>() {
            { "DelayStep", typeof(int) },
        };
        return NodeOptionType.PRESET_LIST;
    }
}