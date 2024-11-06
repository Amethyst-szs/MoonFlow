using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeActionLoop : Node
{
    public NodeActionLoop(Dictionary<object, object> dict) : base(dict) { }
    public NodeActionLoop(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeActionLoop(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override string[] GetNodeTypeOptions() { return []; }
    public override Dictionary<string, Type> GetSupportedParams()
    {
        return new Dictionary<string, Type>() {
            { "ActionName", typeof(string) },
            { "IsStartRandomFrame", typeof(bool) },
            { "MaxStep", typeof(int) },
            { "ActionFrameRate", typeof(float) },
        };
    }
}