using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeDemoCamera : Node
{
    public NodeDemoCamera(Dictionary<object, object> dict) : base(dict) { }
    public NodeDemoCamera(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeDemoCamera(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override NodeOptionType GetNodeNameOptions(out string[] options)
    {
        options = [];
        return NodeOptionType.NO_OPTIONS;
    }
    public override NodeOptionType GetSupportedParams(out Dictionary<string, Type> paramInfo)
    {
        paramInfo = new Dictionary<string, Type>() {
            { "CameraAnimName", typeof(string) },
        };
        return NodeOptionType.PRESET_LIST;
    }
}