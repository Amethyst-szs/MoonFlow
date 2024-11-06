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

    public Dictionary<object, object> WriteBuild()
    {
        var build = new Dictionary<object, object>();

        foreach (var param in this)
        {
            // If the current param is a NodeMessageResolverData, expand structure to original format
            if (param.Value != null && param.Value.GetType() == typeof(NodeMessageResolverData))
            {
                var msgData = (NodeMessageResolverData)param.Value;
                var newParam = new Dictionary<string, Dictionary<string, string>>
                {
                    { "MessageData", msgData.WriteBuild() }
                };

                build.Add(param.Key, newParam);
                continue;
            }

            // If previous conditions failed, just append standard param to build
            build[param.Key] = param.Value;
        }

        return build;
    }
}