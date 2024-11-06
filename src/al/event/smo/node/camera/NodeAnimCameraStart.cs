using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeAnimCameraStart : Node
{
    public NodeAnimCameraStart(Dictionary<object, object> dict) : base(dict) { }
    public NodeAnimCameraStart(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeAnimCameraStart(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override NodeOptionType GetNodeNameOptions(out string[] options)
    {
        options = [];
        return NodeOptionType.NO_OPTIONS;
    }
    public override NodeOptionType GetSupportedParams(out Dictionary<string, Type> paramInfo)
    {
        paramInfo = new Dictionary<string, Type>() {
            { "CameraName", typeof(string) },
            { "AnimName", typeof(string) },
            { "IsOneTime", typeof(bool) },
            { "InterpoleStep", typeof(int) },
        };
        return NodeOptionType.PRESET_LIST;
    }
}