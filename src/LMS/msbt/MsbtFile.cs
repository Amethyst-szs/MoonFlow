using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Godot;

namespace Nindot.LMS.Msbt;

public class MsbtFileEntry
{

}

public class MsbtFile : FileBase
{
    // ====================================================== //
    // ============ Parameters and Initilization ============ //
    // ====================================================== //
    public MsbtFile(byte[] data) : base(data) { }

    public Dictionary<string, EntryContent> Content = [];

    private BlockAttributeData blockATR = null;

    public override void Init(byte[] data, Dictionary<string, int> blockKeys)
    {
        // Read raw data into temporary blocks, will reformat into fields later in init
        var blockLBL = new BlockHashTable(data, "LBL1", blockKeys.GetValueOrDefault("LBL1", -1));
        var blockTXT = new BlockText(data, "TXT2", blockKeys.GetValueOrDefault("TXT2", -1), Header.GetStrEncoding());
        var blockTSY = new BlockStyleIndex(data, "TSY1", blockKeys.GetValueOrDefault("TSY1", -1), blockTXT.GetCount());

        // This block is stored as a field because, as of now, the ATR1 support is limited
        // only to data preservation. No data will be lost if a file already has ATR1 support,
        // but it cannot easily be read or modified.
        blockATR = new BlockAttributeData(data, "ATR1", blockKeys.GetValueOrDefault("ATR1", -1));

        // Build entry list using block data
        ReadOnlyCollection<BlockHashTable.HashTableLabel> labels = blockLBL.GetRawLabelList();

        foreach (var label in labels)
        {
            if (Content.ContainsKey(label.Label))
            {
                GD.PushWarning("Duplicate key in MSBT!");
                continue;
            }

            Content[label.Label] = new EntryContent();
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