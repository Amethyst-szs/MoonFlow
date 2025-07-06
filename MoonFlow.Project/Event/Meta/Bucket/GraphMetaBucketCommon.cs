using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Godot;

namespace MoonFlow.Project;

public class GraphMetaBucketCommon : IProjectFileFormatDataRoot
{
    [JsonInclude]
    public string ArchiveName = "";
    [JsonInclude]
    public string FileName = "";

    [JsonInclude]
    public bool IsFirstOpen = true;

    [JsonInclude]
    public Dictionary<int, GraphMetaBucketNode> Nodes = [];
    [JsonInclude]
    public Dictionary<string, GraphMetaBucketEntryPoint> EntryPoints = [];

    [JsonInclude]
    public Dictionary<string, GraphMetaBucketBlock> Blocks = [];

    #region Utility (Entry Points)

    public GraphMetaBucketEntryPoint GetEntryPointByUid(string uid)
    {
        if (uid == null)
            return null;
        
        EntryPoints.TryGetValue(uid, out GraphMetaBucketEntryPoint data);
        return data;
    }
    public GraphMetaBucketEntryPoint GetEntryPointByName(string name)
    {
        foreach (var item in EntryPoints.Values)
        {
            if (item.Name == name)
                return item;
        }

        // If name lookup failed, use name as uid to account for legacy mfgraph versions
        var backup = GetEntryPointByUid(name);
        if (backup != null)
        {
            // If a node was found that uses the legacy keying system, assign it a uid and update its properties
            backup.TryAssignUid();
            backup.Name = name;

            // And make sure to fix entry point key for this item
            EntryPoints.Remove(name);
            EntryPoints.Add(backup.Uid, backup);

            return backup;
        }

        return null;
    }

    public GraphMetaBucketEntryPoint RenameEntryPoint(string uid, string newName)
    {
        // Attempt to lookup node by uid
        if (EntryPoints.TryGetValue(uid, out GraphMetaBucketEntryPoint instance))
        {
            instance.Name = newName;
            return instance;
        }

        return null;
    }

    #endregion

    #region Utility (Block)

    public string CreateBlockId()
    {
        while (true)
        {
            var id = Guid.NewGuid().ToString().Left(6).ToUpper();

            if (Blocks.ContainsKey(id))
                continue;

            return id;
        }
    }

    public GraphMetaBucketBlock GetBlockMetadata(string id)
    {
        if (Blocks.TryGetValue(id, out GraphMetaBucketBlock block))
            return block;

        var newBlock = new GraphMetaBucketBlock();
        Blocks.Add(id, newBlock);
        return newBlock;
    }

    #endregion
}