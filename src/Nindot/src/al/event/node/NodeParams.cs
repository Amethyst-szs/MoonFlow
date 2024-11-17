using System;
using System.Collections.Generic;
using System.Linq;

namespace Nindot.Al.EventFlow;

public class NodeParams : Dictionary<object, object>
{
    public NodeParams() { }
    public NodeParams(Dictionary<object, object> iter) : base(iter)
    {
        // Scan for any keys in the iterator that can be replaced with NodeMessageResolverData(OnlyLabel)
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
        var build = this.ToDictionary(entry => entry.Key, entry => entry.Value);

        foreach (var param in this)
        {
            if (param.Value == null)
                continue;
            
            // If a NodeMessageResolverData or NodeMessageResolverDataOnlyLabel, expand structure to original format
            Type type = param.Value.GetType();
            if (type == typeof(NodeMessageResolverData))
            {
                WriteBuildMessageData(ref build, param);
                continue;
            }
            else if (type == typeof(NodeMessageResolverDataOnlyLabel))
            {
                WriteBuildMessageDataOnlyLabel(ref build, param);
                continue;
            }

            // If previous conditions failed, just append standard param to build
            build[param.Key] = param.Value;
        }

        return build;
    }

    private static void WriteBuildMessageData(ref Dictionary<object, object> build, KeyValuePair<object, object> param)
    {
        var msgData = (NodeMessageResolverData)param.Value;
        var newParam = new Dictionary<string, Dictionary<string, string>>
        {
            { "MessageData", msgData.WriteBuild() }
        };

        build[param.Key] = newParam;
    }

    private static void WriteBuildMessageDataOnlyLabel(ref Dictionary<object, object> build, KeyValuePair<object, object> param)
    {
        var msgData = (NodeMessageResolverDataOnlyLabel)param.Value;
        var newParam = new Dictionary<string, Dictionary<string, string>>
        {
            { "MessageData", msgData.WriteBuild() }
        };

        build[param.Key] = newParam;
    }
}