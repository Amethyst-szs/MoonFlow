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
    public Dictionary<string, GraphMetaBucketNode> EntryPoints = [];

    [JsonInclude]
    public Dictionary<string, GraphMetaBucketBlock> Blocks = [];

    #region Utility (Node)

    public GraphMetaBucketNode RenameNode(int oldId, int newId)
    {
        if (!Nodes.TryGetValue(oldId, out GraphMetaBucketNode instance))
        {
            var n = new GraphMetaBucketNode();
            Nodes.Add(newId, n);
            return n;
        }

        Nodes.Remove(oldId);
        Nodes.Add(newId, instance);

        return instance;
    }
    public GraphMetaBucketNode RenameEntryPoint(string oldName, string newName)
    {
        if (!EntryPoints.TryGetValue(oldName, out GraphMetaBucketNode instance))
        {
            var n = new GraphMetaBucketNode();
            EntryPoints.Add(newName, n);
            return n;
        }

        EntryPoints.Remove(oldName);
        EntryPoints.Add(newName, instance);

        return instance;
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