using System.Collections.Generic;
using System.IO;
using Godot;
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
        var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Write);
        if (file == null)
        {
            GD.PushError(string.Format("Failed to write EventFlowGraph file to {0} ({1})",
                path,
                Godot.FileAccess.GetOpenError()
            ));
            return false;
        }

        file.StoreBuffer(data);
        file.Close();
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

        return true;
    }

    public List<Dictionary<string, object>> WriteBuildEntryPointList()
    {
        var list = new List<Dictionary<string, object>>();

        foreach (var enter in EntryPoints)
        {
            list.Add(new Dictionary<string, object>
            {
                ["Name"] = enter.Key,
                ["NodeId"] = enter.Value.GetId()
            });
        }

        return list;
    }

    public List<Dictionary<string, object>> WriteBuildNodeList()
    {
        var list = new List<Dictionary<string, object>>();

        foreach (var node in Nodes.Values)
        {
            if (!node.TryWriteBuild(out Dictionary<string, object> build))
            {
                GD.PushWarning(string.Format("Failed to construct node! (ID: {0})", node.GetId()));
                continue;
            }

            list.Add(build);
        }

        return list;
    }
}