using System.Collections.Generic;
using System.Linq;
using Godot;
using Nindot.LMS;

namespace Nindot.LMS.Msbp;

public class MsbpFile : FileBase
{
    private BlockColor Color = null; // CLR1
    private BlockHashTable ColorLabels = null; // CLB1

    private Block AttributeInfo = null; // ATI2
    private BlockHashTable AttributeInfoLabels = null; // ALB1
    private Block AttributeLists = null; // ALI2

    private BlockTagListing TagGroups = null; // TGG2
    private BlockTagListing Tags = null; // TAG2
    private BlockTagParams TagParams = null; // TGP2
    private BlockTagArrayParams TagArrayParams = null; // TGL2

    private Block Styles = null; // SYL3
    private BlockHashTable StyleLabels = null; // SLB1

    private Block Project = null; // CTI1

    public MsbpFile(byte[] data) : base(data)
    {
    }

    public override void Init(byte[] data, Dictionary<string, int> blockKeys)
    {
        Color = new BlockColor(data, "CLR1", blockKeys.GetValueOrDefault("CLR1", -1));
        ColorLabels = new BlockHashTable(data, "CLB1", blockKeys.GetValueOrDefault("CLB1", -1));

        TagGroups = new BlockTagListing(data, "TGG2", blockKeys.GetValueOrDefault("TGG2", -1));
        Tags = new BlockTagListing(data, "TAG2", blockKeys.GetValueOrDefault("TAG2", -1));
        TagParams = new BlockTagParams(data, "TGP2", blockKeys.GetValueOrDefault("TGP2", -1));
        TagArrayParams = new BlockTagArrayParams(data, "TGL2", blockKeys.GetValueOrDefault("TGL2", -1));
        return;
    }

    public override string GetFileMagic()
    {
        return "MsgPrjBn";
    }
}