using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Godot;
using Nindot.LMS.Msbp;
using Nindot.LMS.Msbt.TagLib;

namespace Nindot.LMS.Msbt;

public class MsbtFileEntry
{

}

public class MsbtFile(MsbpFile project, TagLibraryHolder.Type tagLibraryType, byte[] data) : FileBase(data)
{
    // ====================================================== //
    // ============ Parameters and Initilization ============ //
    // ====================================================== //
    public MsbpFile Project { get; private set; } = project;
    public TagLibraryHolder.Type TagLibrary { get; private set; } = tagLibraryType;
    public Dictionary<string, MsbtEntry> Content { get; private set; } = [];
    private BlockAttributeData _blockATR = null;

    public override void Init(byte[] data, Dictionary<string, int> blockKeys)
    {
        // Read raw data into temporary blocks, will reformat into fields later in init
        var blockLBL = new BlockHashTable(data, "LBL1", blockKeys.GetValueOrDefault("LBL1", -1));
        var blockTXT = new BlockText(data, "TXT2", blockKeys.GetValueOrDefault("TXT2", -1), Header.GetStrEncoding());
        var blockTSY = new BlockStyleIndex(data, "TSY1", blockKeys.GetValueOrDefault("TSY1", -1), blockTXT.GetCount());

        // This block is stored as a field because, as of now, the ATR1 support is limited
        // only to data preservation. No data will be lost if a file already has ATR1 support,
        // but it cannot easily be read or modified.
        _blockATR = new BlockAttributeData(data, "ATR1", blockKeys.GetValueOrDefault("ATR1", -1));

        // Build entry list using block data
        ReadOnlyCollection<BlockHashTable.HashTableLabel> labels = blockLBL.GetRawLabelList();

        foreach (var label in labels)
        {
            if (Content.ContainsKey(label.Label))
            {
                GD.PushWarning("Duplicate key in MSBT!");
                continue;
            }
            
            byte[] txtData = blockTXT.TextData[(int)label.ItemIndex];
            
            uint styleIdx = 0xFFFFFFFF;
            if (blockTSY.IsValid())
                styleIdx = blockTSY.StyleIndexList[(int)label.ItemIndex];

            Content[label.Label] = new MsbtEntry(this, txtData, styleIdx);
        }

        return;
    }

    // Ensure that the binary file being read contains this magic, otherwise it isn't an MSBT!
    public override string GetFileMagic()
    {
        return "MsgStdBn";
    }

    public override bool WriteFile(MemoryStream stream)
    {
        throw new NotImplementedException();
    }
}