using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeDemoForceStart : Node
{
    public NodeDemoForceStart(Dictionary<object, object> dict) : base(dict) { }
    public NodeDemoForceStart(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeDemoForceStart(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override NodeOptionType GetNodeNameOptions(out string[] options)
    {
        options = [
            "DemoForceStartOnGround",
            "DemoForceStart",
            "DemoOnlyRequesterForceStartOnGround",
            "KeepHackDemoForceStartOnGround",
        ];
        return NodeOptionType.PRESET_LIST;
    }
    public override NodeOptionType GetSupportedParams(out Dictionary<string, Type> paramInfo)
    {
        paramInfo = [];
        return NodeOptionType.NO_OPTIONS;
    }
}