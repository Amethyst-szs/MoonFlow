using System.Collections.Generic;
using Godot;
using Nindot.LMS;

namespace Nindot.LMS.Msbp;

public class MsbpFile : FileBase
{
    private BlockColor Color;

    public MsbpFile(byte[] data) : base(data)
    {
    }

    public override void Init(Dictionary<string, int> blockKeys)
    {
        foreach(var x in blockKeys)
        {
            GetBlockFromTypeName(x.Key)
            GD.Print(string.Format("{0}: {1:X}", x.Key, x.Value));
        }
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