using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeActionOneTime : Node
{
    public NodeActionOneTime(Dictionary<object, object> dict) : base(dict) { }
    public NodeActionOneTime(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeActionOneTime(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override string[] GetNodeTypeOptions() { return []; }
    public override Dictionary<string, Type> GetSupportedParams()
    {
        return new Dictionary<string, Type>() {
            { "ActionName", typeof(string) },
            { "ActionFrameRate", typeof(float) },
        };
    }
}