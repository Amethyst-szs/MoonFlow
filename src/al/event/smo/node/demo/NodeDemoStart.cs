using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeDemoStart : Node
{
    public NodeDemoStart(Dictionary<object, object> dict) : base(dict) { }
    public NodeDemoStart(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeDemoStart(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override NodeOptionType GetNodeNameOptions(out string[] options)
    {
        options = [
            "DemoStart",
            "HackKeepDemoStart",
            "ShopDemoStart",
        ];
        return NodeOptionType.PRESET_LIST;
    }
    public override NodeOptionType GetSupportedParams(out Dictionary<string, Type> paramInfo)
    {
        paramInfo = new Dictionary<string, Type>() {
            { "AudioEventName", typeof(string) },
        };
        return NodeOptionType.PRESET_LIST;
    }
}