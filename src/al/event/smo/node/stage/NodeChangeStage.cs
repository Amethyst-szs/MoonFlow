using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow.Smo;

public class NodeChangeStage : Node
{
    public NodeChangeStage(Dictionary<object, object> dict) : base(dict) { }
    public NodeChangeStage(Graph graph, string factoryType) : base(graph, factoryType) { }
    public NodeChangeStage(Graph graph, string typeBase, string type) : base(graph, typeBase, type) { }

    public override NodeOptionType GetNodeNameOptions(out string[] options)
    {
        options = [];
        return NodeOptionType.NO_OPTIONS;
    }
    public override NodeOptionType GetSupportedParams(out Dictionary<string, Type> paramInfo)
    {
        paramInfo = new Dictionary<string, Type>() {
            { "StageName", typeof(string) }, // If set to "現在のステージ", the current stage will be used
            { "PlayerStartId", typeof(string) }, // If set to "指定しない", no start Id will be used
            { "ScenarioNo", typeof(int) },
            { "IsReturnPrevStage", typeof(bool) },
            { "IsSubScenario", typeof(bool) },
        };
        return NodeOptionType.PRESET_LIST;
    }
}