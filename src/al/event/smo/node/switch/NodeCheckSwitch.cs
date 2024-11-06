using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeCheckSwitch : Node
{
    public NodeCheckSwitch(Dictionary<object, object> dict) : base(dict) { }
    public NodeCheckSwitch(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeCheckSwitch(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override bool IsAllowOutgoingEdges() { return true; }
    public override bool IsUseMultipleOutgoingEdges() { return true; }
    public override int GetMaxOutgoingEdges() { return 2; }

    public override NodeNameOptionType GetNodeNameOptions(out string[] options)
    {
        options = [];
        return NodeNameOptionType.ANY_VALUE;
    }
    public override Dictionary<string, Type> GetSupportedParams() { return []; }
}