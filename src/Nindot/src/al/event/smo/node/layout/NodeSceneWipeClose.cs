using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeSceneWipeClose : Node
{
    public NodeSceneWipeClose(Dictionary<object, object> dict) : base(dict) { }
    public NodeSceneWipeClose(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeSceneWipeClose(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override NodeOptionType GetNodeNameOptions(out string[] options)
    {
        options = [];
        return NodeOptionType.NO_OPTIONS;
    }
    public override NodeOptionType GetSupportedParams(out Dictionary<string, Type> paramInfo)
    {
        paramInfo = new Dictionary<string, Type>() {
            { "SituationName", typeof(string) },
            { "DelayStep", typeof(int) },
        };
        return NodeOptionType.PRESET_LIST;
    }
}