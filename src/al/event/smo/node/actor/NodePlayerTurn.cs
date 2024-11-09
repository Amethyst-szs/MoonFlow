using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodePlayerTurn : Node
{
    public NodePlayerTurn(Dictionary<object, object> dict) : base(dict) { }
    public NodePlayerTurn(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodePlayerTurn(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override NodeOptionType GetNodeNameOptions(out string[] options)
    {
        options = [
            "PlayerTurn",
            "TurnToPlayerFaceToFace",
        ];
        return NodeOptionType.NO_OPTIONS;
    }
    public override NodeOptionType GetSupportedParams(out Dictionary<string, Type> paramInfo)
    {
        paramInfo = [];
        return NodeOptionType.NO_OPTIONS;
    }
}