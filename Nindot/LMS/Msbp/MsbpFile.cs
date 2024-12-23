using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nindot.LMS.Msbp;

public partial class MsbpFile(byte[] data, string name) : FileBase(data, name)
{
    // ====================================================== //
    // ============ Parameters and Initilization ============ //
    // ====================================================== //

    private BlockColor Color = null; // CLR1
    private BlockHashTable ColorLabels = null; // CLB1

    private BlockAttributeInfo AttributeInfo = null; // ATI2
    private BlockHashTable AttributeInfoLabels = null; // ALB1
    private BlockAttributeLists AttributeLists = null; // ALI2

    private BlockTagCommon TagGroups = null; // TGG2
    private BlockTagCommon Tags = null; // TAG2
    private BlockTagParams TagParams = null; // TGP2
    private BlockTagArrayParams TagArrayParams = null; // TGL2

    private BlockStyles Styles = null; // SYL3
    private BlockHashTable StyleLabels = null; // SLB1

    public BlockProject Project = null; // CTI1

    // Initalize every kind of MSBP block using their 4 byte names

    public override void Init(byte[] data, Dictionary<string, int> blockKeys)
    {
        Color = new BlockColor(data, "CLR1", blockKeys.GetValueOrDefault("CLR1", -1), this);
        Blocks.Add(Color);
        ColorLabels = new BlockHashTable(data, "CLB1", blockKeys.GetValueOrDefault("CLB1", -1), this);
        Blocks.Add(ColorLabels);

        AttributeInfo = new BlockAttributeInfo(data, "ATI2", blockKeys.GetValueOrDefault("ATI2", -1), this);
        Blocks.Add(AttributeInfo);
        AttributeInfoLabels = new BlockHashTable(data, "ALB1", blockKeys.GetValueOrDefault("ALB1", -1), this);
        Blocks.Add(AttributeInfoLabels);
        AttributeLists = new BlockAttributeLists(data, "ALI2", blockKeys.GetValueOrDefault("ALI2", -1), this);
        Blocks.Add(AttributeLists);

        TagGroups = new BlockTagCommon(data, "TGG2", blockKeys.GetValueOrDefault("TGG2", -1), this);
        Blocks.Add(TagGroups);
        Tags = new BlockTagCommon(data, "TAG2", blockKeys.GetValueOrDefault("TAG2", -1), this);
        Blocks.Add(Tags);
        TagParams = new BlockTagParams(data, "TGP2", blockKeys.GetValueOrDefault("TGP2", -1), this);
        Blocks.Add(TagParams);
        TagArrayParams = new BlockTagArrayParams(data, "TGL2", blockKeys.GetValueOrDefault("TGL2", -1), this);
        Blocks.Add(TagArrayParams);

        Styles = new BlockStyles(data, "SYL3", blockKeys.GetValueOrDefault("SYL3", -1), this);
        Blocks.Add(Styles);
        StyleLabels = new BlockHashTable(data, "SLB1", blockKeys.GetValueOrDefault("SLB1", -1), this);
        Blocks.Add(StyleLabels);

        Project = new BlockProject(data, "CTI1", blockKeys.GetValueOrDefault("CTI1", -1), this);
        Blocks.Add(Project);
    }

    // Ensure that the binary file being read contains this magic, otherwise it isn't an MSBP!
    public override string GetFileMagic()
    {
        return "MsgPrjBn";
    }

    // ====================================================== //
    // ================ Constructor Utilities =============== //
    // ====================================================== //

    static public MsbpFile FromFilePath(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException(path);

        byte[] bytes;

        try
        {
            bytes = File.ReadAllBytes(path);
        }
        catch
        {
            throw new FileLoadException(path);
        }

        return FromBytes(bytes, path.Split(['/', '\\']).Last());
    }

    static public MsbpFile FromBytes(byte[] data, string name)
    {
        MsbpFile file;

        try { file = new(data, name); }
        catch { throw new LMSException("Failed to parse MSBP file"); }

        return file;
    }
}