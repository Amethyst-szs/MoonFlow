using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeCheckWaitSwitch : Node
{
    public NodeCheckWaitSwitch(Dictionary<object, object> dict) : base(dict) { }
    public NodeCheckWaitSwitch(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeCheckWaitSwitch(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override NodeNameOptionType GetNodeNameOptions(out string[] options)
    {
        options = [];
        return NodeNameOptionType.NO_OPTIONS;
    }
    public override Dictionary<string, Type> GetSupportedParams()
    {
        return new Dictionary<string, Type>() {
            { "SwitchName", typeof(string) },
        };
    }
}