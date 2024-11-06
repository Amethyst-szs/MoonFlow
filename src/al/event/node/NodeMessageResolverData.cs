using System.Collections.Generic;

namespace Nindot.Al.EventFlow;

public class NodeMessageResolverData
{
    public string MessageArchive;
    public string MessageFile;
    public string LabelName;

    public NodeMessageResolverData(Dictionary<object, object> dict)
    {
        if (dict.ContainsKey("MessageType")) MessageArchive = (string)dict["MessageType"];
        if (dict.ContainsKey("MessageFileName")) MessageFile = (string)dict["MessageFileName"];
        if (dict.ContainsKey("LabelName")) LabelName = (string)dict["LabelName"];
    }

    public NodeMessageResolverData(string archive, string file, string label)
    {
        MessageArchive = archive;
        MessageFile = file;
        LabelName = label;
    }
}