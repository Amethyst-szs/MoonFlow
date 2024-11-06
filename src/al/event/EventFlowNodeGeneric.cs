using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow;

public class EventFlowNodeGeneric : NodeBase
{
    public EventFlowNodeGeneric(Dictionary<object, object> dict) : base(dict) { }
    public EventFlowNodeGeneric(Graph graph, string factoryType) : base(graph, factoryType) { }
    public EventFlowNodeGeneric(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override bool IsSupportEdges() { return true; }
    public override int GetMaxEdgeCount() { return 1; }
    public override int GetMinEdgeCount() { return 1; }

    public override string[] GetNodeTypeOptions() { return []; }
    public override Dictionary<string, Type> GetSupportedParams()
    {
        return [];
    }
}