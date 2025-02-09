using System.Collections.Generic;
using System.IO;
using Nindot.Byml;

namespace Nindot.Al.EventFlow;

public partial class Graph
{
    public bool WriteFile(string path)
    {
        // Attempt to load in bytes from WriteBytes
        if (!IsValid()) return false;
        if (!WriteBytes(out byte[] data)) return false;

        // Write to disk
        File.WriteAllBytes(path, data);
        return true;
    }

    public bool WriteBytes(out byte[] data)
    {
        // Setup default value for data
        data = [];

        // Attempt to get build dictionary
        if (!IsValid()) return false;
        if (!WriteBuild(out Dictionary<string, object> build)) return false;

        MemoryStream stream = new();
        if (!BymlFileAccess.WriteFile(stream, new BymlFile(build, _bymlVersion))) return false;

        data = stream.ToArray();
        return true;
    }

    public bool WriteBuild(out Dictionary<string, object> build)
    {
        // Prepare dictionary for storing content
        build = [];

        // Ensure graph validity
        if (!IsValid()) return false;

        // Build the two main data structures of the byml dictionary
        build["EntryList"] = WriteBuildEntryPointList();
        build["NodeList"] = WriteBuildNodeList();

        var itemList = WriteBuildItemList();
        if (itemList.Count > 0)
            build["ItemList"] = itemList;

        return true;
    }

    private List<Dictionary<string, object>> WriteBuildEntryPointList()
    {
        var list = new List<Dictionary<string, object>>();

        foreach (var enter in EntryPoints)
        {
            int id = -1;
            if (enter.Value != null)
                id = enter.Value.Id;
            
            list.Add(new Dictionary<string, object>
            {
                ["Name"] = enter.Key,
                ["NodeId"] = id
            });
        }

        return list;
    }

    private List<Dictionary<string, object>> WriteBuildNodeList()
    {
        var list = new List<Dictionary<string, object>>();

        foreach (var node in Nodes.Values)
        {
            if (!node.TryWriteBuild(out Dictionary<string, object> build))
                throw new EventFlowException(string.Format("Failed to construct node! (ID: {0})", node.GetId()));

            list.Add(build);
        }

        return list;
    }

    private List<string> WriteBuildItemList()
    {
        var list = new List<string>();

        foreach (var item in ItemList)
        {
            list.Add(item.GetName());
        }

        return list;
    }
}