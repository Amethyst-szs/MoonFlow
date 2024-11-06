using System.Collections.Generic;
using System.Linq;

namespace Nindot.Al.EventFlow;

public class NodeParams : Dictionary<object, object>
{
    public NodeParams() { }
    public NodeParams(Dictionary<object, object> iter) : base(iter) {
        // Scan for any keys in the iterator that can be replaced with NodeMessageResolverData
        foreach (var obj in this)
        {
            if (obj.Value == null || obj.Value.GetType() != typeof(Dictionary<object, object>))
                continue;
            
            var param = (Dictionary<object, object>)obj.Value;
            if (!param.ContainsKey("MessageData"))
                continue;
            
            var messageData = (Dictionary<object, object>)param["MessageData"];
            this[obj.Key] = new NodeMessageResolverData(messageData);
            continue;
        }
    }
}