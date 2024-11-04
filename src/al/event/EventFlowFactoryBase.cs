using System.Collections.Generic;

namespace Nindot.Al.EventFlow;

public class EventFlowFactoryBase
{
    public static NodeBase CreateNodeBase(Dictionary<object, object> dict)
    {
        string nType = GetNodeType(dict);
        return CreateNodeAl(nType, dict, true);
    }

    protected static string GetNodeType(Dictionary<object, object> dict)
    {
        string type = "";

        if (dict.ContainsKey("Base")) type = (string)dict["Base"];
        else if (dict.ContainsKey("Type")) type = (string)dict["Type"];

        return type;
    }

    protected static NodeBase CreateNodeAl(string type, Dictionary<object, object> dict, bool isBaseClassCall)
    {
        return type switch {
            _ => CreateNodeNoMatchFallback(dict, isBaseClassCall),
        };
    }

    private static NodeBase CreateNodeNoMatchFallback(Dictionary<object, object> dict, bool isBaseClassCall)
    {
        if (isBaseClassCall)
            return new NodeBase(dict);
        
        return null;
    }
}