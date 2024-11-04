using System.Collections.Generic;

namespace Nindot.Al.EventFlow;

public class NodeBase
{
    public string Type { get; protected set; } = null;
    public string TypeBase { get; protected set; } = null;

    public int Id { get; protected set; } = int.MinValue;
    protected int NextId = int.MinValue;

    public NodeBase(Dictionary<object, object> dict)
    {
        if (dict.ContainsKey("Type")) Type = (string)dict["Type"];
        if (dict.ContainsKey("Base")) TypeBase = (string)dict["Base"];
        if (dict.ContainsKey("Id")) Id = (int)dict["Id"];
        if (dict.ContainsKey("NextId")) NextId = (int)dict["NextId"];
    }
}