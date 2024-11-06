using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow;

// This generic class is used for any node type with no subtypes, parameters, or special implementation
public class NodeGeneric : Node
{
    public NodeGeneric(Dictionary<object, object> dict) : base(dict) { }
    public NodeGeneric(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeGeneric(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override string[] GetNodeTypeOptions() { return []; }
    public override Dictionary<string, Type> GetSupportedParams() { return []; }
}