using System.Collections.Generic;
using System.Linq;

namespace Nindot.Al.EventFlow;

public class NodeParams : Dictionary<object, object>
{
    public NodeParams() { }
    public NodeParams(Dictionary<object, object> iter) : base(iter) {
        // Scan for any keys in the iterator that can be replaced with NodeMessageResolverData
        foreach (var obj in iter.Values)
        {
            if (obj == null || obj.GetType() != typeof(Dictionary<object, object>))
                continue;
            
            var param = (Dictionary<object, object>)obj;
            if (!param.ContainsKey("MessageData"))
                continue;
            
            var messageData = (Dictionary<object, object>)param["MessageData"];
            param["MessageData"] = new NodeMessageResolverData(messageData);
        }
    }
}