using System.Collections.Generic;
using System.Linq;
using Godot;
using Nindot.LMS;

namespace Nindot.LMS.Msbp;

public class MsbpFile : FileBase
{
    private BlockColor Color = null; // CLR1
    private BlockHashTable ColorLabels = null; // CLB1

    public MsbpFile(byte[] data) : base(data)
    {
    }

    public override void Init(byte[] data, Dictionary<string, int> blockKeys)
    {
        ColorLabels = new BlockHashTable(data, "CLB1", blockKeys.GetValueOrDefault("CLB1", -1));
        Color = new BlockColor(data, "CLR1", blockKeys.GetValueOrDefault("CLR1", -1));
        return;
    }

    public override string GetFileMagic()
    {
        return "MsgPrjBn";
    }

    public Block GetBlockFromTypeName(string typeName)
    {
        return typeName switch
        {
            "CLR1" => Color,
            _ => null,
        };
    }
}