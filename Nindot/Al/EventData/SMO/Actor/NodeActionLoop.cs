using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeActionLoop : Node
{
    public NodeActionLoop(Dictionary<object, object> dict) : base(dict) { }
    public NodeActionLoop(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeActionLoop(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }
    public NodeActionLoop(string factoryType, int id) : base(factoryType, id) { }

    public override NodeOptionType GetNodeNameOptions(out string[] options)
    {
        options = [];
        return NodeOptionType.NO_OPTIONS;
    }
    public override NodeOptionType GetSupportedParams(out Dictionary<string, Type> paramInfo)
    {
        paramInfo = new Dictionary<string, Type>() {
            { "ActionName", typeof(string) },
            { "IsStartRandomFrame", typeof(bool) },
            { "MaxStep", typeof(int) },
            { "ActionFrameRate", typeof(float) },
        };
        return NodeOptionType.PRESET_LIST;
    }
}