using System.Collections.Generic;

namespace Nindot.Al.EventFlow;

public class EventFlowFactoryProjectSMO : EventFlowFactoryBase
{
    public static NodeBase CreateNodeSMO(Dictionary<object, object> dict)
    {
        string nType = GetNodeType(dict);
        NodeBase node = CreateNodeAl(nType, dict, false);
        if (node != null)
            return node;
        
        return nType switch {
            _ => new NodeBase(dict),
        };
    }
}