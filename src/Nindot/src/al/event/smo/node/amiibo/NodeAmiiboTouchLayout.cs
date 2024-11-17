using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeAmiiboTouchLayout : Node
{
    public enum CaseType : int
    {
        SUCCESSFUL = 0,
        SUCCESSFUL_WITHOUT_MSBTXT = 1,
        SUCCESSFUL_WITH_MSBTXT = 2,
        AMIIBO_BUSY_TRY_AGAIN_LATER = 3,
        CANCEL = 4,
        ENUM_SIZE = 5,
    }

    public NodeAmiiboTouchLayout(Dictionary<object, object> dict) : base(dict) { }
    public NodeAmiiboTouchLayout(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeAmiiboTouchLayout(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override bool IsUseMultipleOutgoingEdges() { return true; }
    public override bool IsAllowOutgoingEdges() { return true; }
    public override int GetMaxOutgoingEdges() { return (int)CaseType.ENUM_SIZE; }

    public override NodeOptionType GetNodeNameOptions(out string[] options)
    {
        options = [];
        return NodeOptionType.NO_OPTIONS;
    }
    public override NodeOptionType GetSupportedParams(out Dictionary<string, Type> paramInfo)
    {
        paramInfo = [];
        return NodeOptionType.NO_OPTIONS;
    }
}