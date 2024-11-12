using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Util;
using Godot;
using Nindot.LMS.Msbt.TagLib;

namespace Nindot.LMS.Msbt;

public class MsbtFile(TagLibraryHolder.Type tagLibraryType, byte[] data) : FileBase(data)
{
    // ====================================================== //
    // ============ Parameters and Initilization ============ //
    // ====================================================== //
    public readonly TagLibraryHolder.Type TagLibrary = tagLibraryType;
    public OrderedDictionary<string, MsbtEntry> Content = [];

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

    // ====================================================== //
    // ================ Constructor Utilities =============== //
    // ====================================================== //

    static public MsbtFile FromFilePath(string path, TagLibraryHolder.Type tagLib)
    {
        if (!Godot.FileAccess.FileExists(path))
        {
            GD.PushError("No file exists at the path ", path);
            return null;
        }

        var bytes = Godot.FileAccess.GetFileAsBytes(path);
        if (bytes.Length == 0)
        {
            GD.PushError("An error occured opening file ", path, " - ", Godot.FileAccess.GetOpenError());
            return null;
        }

        return FromBytes(bytes, tagLib);
    }

    static public MsbtFile FromBytes(byte[] data, TagLibraryHolder.Type tagLib)
    {
        MsbtFile file = new(tagLib, data);
        if (!file.IsValid())
        {
            GD.PushError("File at is not a valid MSBT!");
            return null;
        }

        return file;
    }

    // ====================================================== //
    // ================== Getter Utilities ================== //
    // ====================================================== //

    public bool IsContainKey(string label)
    {
        return Content.ContainsKey(label);
    }
    public int GetEntryCount()
    {
        return Content.Count;
    }
    public MsbtEntry GetEntry(int idx)
    {
        if (idx >= Content.Count)
            return null;
        
        return Content.Values.ElementAt(idx);
    }
    public MsbtEntry GetEntry(string label)
    {
        if (!Content.ContainsKey(label))
            return null;
        
        return Content[label];
    }
    public ReadOnlyCollection<string> GetEntryLabels()
    {
        return new ReadOnlyCollection<string>([.. Content.Keys]);
    }

    // ====================================================== //
    // ========= File Content Modification Utilities ======== //
    // ====================================================== //
    
    public MsbtEntry AddNew(string label)
    {
        MsbtEntry entry = new(TagLibrary);
        Content.Add(label, entry);
        return entry;
    }
    public MsbtEntry AddNew(string label, string textContent)
    {
        MsbtEntry entry = new(TagLibrary, textContent);
        Content.Add(label, entry);
        return entry;
    }

    public bool RemoveEntry(string label)
    {
        if (!Content.ContainsKey(label))
            return false;
        
        Content.Remove(label);
        return true;
    }
}