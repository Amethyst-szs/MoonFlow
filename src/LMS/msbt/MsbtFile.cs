using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Util;
using Godot;
using Nindot.LMS.Msbt.TagLib;

namespace Nindot.LMS.Msbt;

public class MsbtFileEntry
{

}

public class MsbtFile(TagLibraryHolder.Type tagLibraryType, byte[] data) : FileBase(data)
{
    // ====================================================== //
    // ============ Parameters and Initilization ============ //
    // ====================================================== //
    public readonly TagLibraryHolder.Type TagLibrary = tagLibraryType;
    public OrderedDictionary<string, MsbtEntry> Content { get; private set; } = [];

    private BlockHashTable BlockLabels = null;
    private BlockText BlockText = null;
    private BlockStyleIndex BlockStyleIndex = null;
    private BlockAttributeData BlockAttributes = null;

    public override void Init(byte[] data, Dictionary<string, int> blockKeys)
    {
        // Read raw data into blocks, will reformat into fields later in init
        BlockLabels = new BlockHashTable(data, "LBL1", blockKeys.GetValueOrDefault("LBL1", -1));
        Blocks.Add(BlockLabels);
        BlockText = new BlockText(data, "TXT2", blockKeys.GetValueOrDefault("TXT2", -1), Header.GetCharSize());
        Blocks.Add(BlockText);
        BlockStyleIndex = new BlockStyleIndex(data, "TSY1", blockKeys.GetValueOrDefault("TSY1", -1), BlockText.GetCount());
        Blocks.Add(BlockStyleIndex);
        BlockAttributes = new BlockAttributeData(data, "ATR1", blockKeys.GetValueOrDefault("ATR1", -1));
        Blocks.Add(BlockAttributes);

        // Build entry list using block data
        ReadOnlyCollection<BlockHashTable.HashTableLabel> labels = BlockLabels.GetRawLabelList();

        foreach (var label in labels)
        {
            if (Content.ContainsKey(label.Label))
            {
                GD.PushWarning("Duplicate key in MSBT!");
                continue;
            }
            
            byte[] txtData = BlockText.TextData[(int)label.ItemIndex];
            
            uint styleIdx = 0xFFFFFFFF;
            if (BlockStyleIndex.IsValid())
                styleIdx = BlockStyleIndex.StyleIndexList[(int)label.ItemIndex];

            Content.Add(label.Label, new MsbtEntry(TagLibrary, txtData, styleIdx));
        }
    }

    // Ensure that the binary file being read contains this magic, otherwise it isn't an MSBT!
    public override string GetFileMagic()
    {
        return "MsgStdBn";
    }

    public override bool WriteFile(MemoryStream stream)
    {
        // Write header to stream
        if (!Header.WriteHeader(stream))
            return false;

        // Update all block content's using Content dictionary
        BlockLabels.RebuildTable([.. Content.Keys]);
        BlockText.UpdateBlock([.. Content.Values]);
        BlockStyleIndex.UpdateBlock([.. Content.Values]);
        
        // Write all blocks into stream
        foreach (var b in Blocks)
        {
            if (!b.WriteBlock(stream))
                return false;
        }

        return true;
    }
}