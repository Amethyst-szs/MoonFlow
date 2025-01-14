using System;
using System.Collections.Generic;
using Godot;

namespace MoonFlow.Project;

public class GraphMetadata
{
    public string ArchiveName = "";
    public string FileName = "";

    public bool IsFirstOpen = true;

    public Dictionary<int, NodeMetadata> Nodes = [];
    public Dictionary<string, NodeMetadata> EntryPoints = [];

    public Dictionary<string, BlockMetadata> Blocks = [];

    #region Utility (Node)

    public NodeMetadata RenameNode(int oldId, int newId)
    {
        if (!Nodes.TryGetValue(oldId, out NodeMetadata instance))
        {
            var n = new NodeMetadata();
            Nodes.Add(newId, n);
            return n;
        }

        Nodes.Remove(oldId);
        Nodes.Add(newId, instance);

        return instance;
    }
    public NodeMetadata RenameEntryPoint(string oldName, string newName)
    {
        if (!EntryPoints.TryGetValue(oldName, out NodeMetadata instance))
        {
            var n = new NodeMetadata();
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
            GD.Print(id);

            if (Blocks.ContainsKey(id))
                continue;

            return id;
        }
    }

    public BlockMetadata GetBlockMetadata(string id)
    {
        if (Blocks.TryGetValue(id, out BlockMetadata block))
            return block;

        var newBlock = new BlockMetadata();
        Blocks.Add(id, newBlock);
        return newBlock;
    }

    #endregion
}