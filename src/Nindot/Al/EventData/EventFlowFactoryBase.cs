using System;
using System.Collections.Generic;

namespace Nindot.Al.EventFlow;

public class EventFlowFactoryBase
{
    public virtual Node CreateNode(Dictionary<object, object> dict)
    {
        return new NodeGeneric(dict);
    }

    protected static string GetNodeType(Dictionary<object, object> dict)
    {
        string type = "";

        if (dict.ContainsKey("Base")) type = (string)dict["Base"];
        else if (dict.ContainsKey("Type")) type = (string)dict["Type"];

        return type;
    }
}