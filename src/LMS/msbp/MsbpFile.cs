using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Godot;

namespace Nindot.LMS.Msbp;

public class MsbpFile : FileBase
{
    // ====================================================== //
    // ============ Parameters and Initilization ============ //
    // ====================================================== //

    private BlockColor Color = null; // CLR1
    private BlockHashTable ColorLabels = null; // CLB1

    private BlockAttributeInfo AttributeInfo = null; // ATI2
    private BlockHashTable AttributeInfoLabels = null; // ALB1
    private BlockAttributeLists AttributeLists = null; // ALI2

    private BlockTagListing TagGroups = null; // TGG2
    private BlockTagListing Tags = null; // TAG2
    private BlockTagParams TagParams = null; // TGP2
    private BlockTagArrayParams TagArrayParams = null; // TGL2

    private Block Styles = null; // SYL3
    private BlockHashTable StyleLabels = null; // SLB1

    private Block Project = null; // CTI1

    public MsbpFile(byte[] data) : base(data) { }

    // Initalize every kind of MSBP block using their 4 byte names
    public override void Init(byte[] data, Dictionary<string, int> blockKeys)
    {
        Color = new BlockColor(data, "CLR1", blockKeys.GetValueOrDefault("CLR1", -1));
        Blocks.Add(Color);
        ColorLabels = new BlockHashTable(data, "CLB1", blockKeys.GetValueOrDefault("CLB1", -1));
        Blocks.Add(ColorLabels);

        AttributeInfo = new BlockAttributeInfo(data, "ATI2", blockKeys.GetValueOrDefault("ATI2", -1));
        Blocks.Add(AttributeInfo);
        AttributeInfoLabels = new BlockHashTable(data, "ALB1", blockKeys.GetValueOrDefault("ALB1", -1));
        Blocks.Add(AttributeInfoLabels);
        AttributeLists = new BlockAttributeLists(data, "ALI2", blockKeys.GetValueOrDefault("ALI2", -1));
        Blocks.Add(AttributeLists);

        TagGroups = new BlockTagListing(data, "TGG2", blockKeys.GetValueOrDefault("TGG2", -1));
        Blocks.Add(TagGroups);
        Tags = new BlockTagListing(data, "TAG2", blockKeys.GetValueOrDefault("TAG2", -1));
        Blocks.Add(Tags);
        TagParams = new BlockTagParams(data, "TGP2", blockKeys.GetValueOrDefault("TGP2", -1));
        Blocks.Add(TagParams);
        TagArrayParams = new BlockTagArrayParams(data, "TGL2", blockKeys.GetValueOrDefault("TGL2", -1));
        Blocks.Add(TagArrayParams);

        Styles = new BlockStyles(data, "SYL3", blockKeys.GetValueOrDefault("SYL3", -1));
        Blocks.Add(Styles);
        StyleLabels = new BlockHashTable(data, "SLB1", blockKeys.GetValueOrDefault("SLB1", -1));
        Blocks.Add(StyleLabels);

        Project = new BlockProject(data, "CTI1", blockKeys.GetValueOrDefault("CTI1", -1));
        Blocks.Add(Project);
    }

    // Ensure that the binary file being read contains this magic, otherwise it isn't an MSBP!
    public override string GetFileMagic()
    {
        return "MsgPrjBn";
    }

    // ====================================================== //
    // =================== Color Utilities ================== //
    // ====================================================== //

    public bool ColorIsFileContainData()
    {
        return ColorLabels.IsValid() && Color.IsValid();
    }

    public int ColorGetCount()
    {
        if (!ColorLabels.IsValid())
            return 0;
        
        return ColorLabels.CalcLabelCount();
    }

    public string[] ColorGetLabelList()
    {
        if (!ColorLabels.IsValid())
            return [];
        
        return ColorLabels.GetLabelList();
    }

    public ReadOnlyCollection<BlockColor.Entry> ColorGetList()
    {
        if (!Color.IsValid())
            return null;
        
        return Color.GetColorList();
    }

    public BlockColor.Entry ColorGet(string labelName)
    {
        if (!ColorIsFileContainData())
            return null;
        
        int idx = ColorLabels.GetItemIndex(labelName);
        if (idx == -1)
            return null;
        
        return Color.GetColor(idx);
    }

    public void ColorAddNew(string name, byte r, byte g, byte b, byte a)
    {
        if (!ColorIsFileContainData())
            return;
        
        BlockColor.Entry entry = new(r, g, b, a);
        ColorAddNew(name, entry);
    }

    public void ColorAddNew(string name, BlockColor.Entry color)
    {
        if (!ColorIsFileContainData())
            return;
        
        int idx = Color.AddColor(color);
        ColorLabels.AddItem(name, idx);
    }

    public void ColorMoveIndex(string name, int newIndex)
    {
        if (!ColorIsFileContainData())
            return;
        
        int oldIndex = ColorLabels.GetItemIndex(name);
        Color.MoveColor(oldIndex, newIndex);
        ColorLabels.MoveItem(name, newIndex);
    }

    public void ColorMoveIndexByOffset(string name, int offset)
    {
        if (!ColorIsFileContainData())
            return;
        
        int oldIndex = ColorLabels.GetItemIndex(name);
        Color.MoveColor(oldIndex, oldIndex + offset);
        ColorLabels.MoveItemByOffset(name, offset);
    }

    public void ColorRemove(string name)
    {
        if (!ColorIsFileContainData())
            return;
        
        int idx = ColorLabels.RemoveItem(name);
        Color.RemoveColor(idx);
    }

    // ====================================================== //
    // ================= Attribute Utilities ================ //
    // ====================================================== //

    public bool AttributeIsFileContainData()
    {
        return AttributeInfo.IsValid() && AttributeInfoLabels.IsValid();
    }

    public int AttributeGetCount()
    {
        if (!AttributeInfoLabels.IsValid())
            return 0;
        
        return AttributeInfoLabels.CalcLabelCount();
    }

    public string[] AttributeGetLabelList()
    {
        if (!AttributeInfoLabels.IsValid())
            return [];
        
        return AttributeInfoLabels.GetLabelList();
    }

    public ReadOnlyCollection<BlockAttributeInfo.Entry> AttributeGetList()
    {
        if (!AttributeInfo.IsValid())
            return new ReadOnlyCollection<BlockAttributeInfo.Entry>([]);
        
        return AttributeInfo.GetInfoList();
    }

    public BlockAttributeInfo.Entry AttributeGet(string labelName)
    {
        if (!AttributeIsFileContainData())
            return null;
        
        int idx = AttributeInfoLabels.GetItemIndex(labelName);
        if (idx == -1)
            return null;
        
        return AttributeInfo.GetAttribute(idx);
    }

    public ReadOnlyCollection<string> AttributeGetContentArrayList(BlockAttributeInfo.Entry attribute)
    {
        if (attribute.Type != 0x9)
            return new ReadOnlyCollection<string>([]);
        
        return AttributeLists.GetList(attribute.ListIndex);
    }

    // ====================================================== //
    // ============== Tag Group & Tag Utilities ============= //
    // ====================================================== //

    public bool TagIsFileContainData()
    {
        return TagGroups.IsValid() && Tags.IsValid() && TagParams.IsValid() && TagArrayParams.IsValid();
    }

    public int TagGroupGetCount()
    {
        if (!TagGroups.IsValid())
            return 0;
        
        return TagGroups.GetListingCount();
    }

    public ReadOnlyCollection<BlockTagListing.Listing> TagGroupGetList()
    {
        if (!TagGroups.IsValid())
            return new ReadOnlyCollection<BlockTagListing.Listing>([]);
        
        return TagGroups.GetListings();
    }

    public BlockTagListing.Listing TagGroupGet(int idx)
    {
        if (!TagGroups.IsValid())
            return null;
        
        return TagGroups.GetListing(idx);
    }

    public int TagGetCount(BlockTagListing.Listing tagGroup)
    {
        if (!Tags.IsValid())
            return 0;
        
        return tagGroup.ListingIndexList.Count;
    }

    public ReadOnlyCollection<BlockTagListing.Listing> TagGetList(BlockTagListing.Listing tagGroup)
    {
        if (!Tags.IsValid())
            return new ReadOnlyCollection<BlockTagListing.Listing>([]);
        
        return Tags.GetTagsInGroup(tagGroup);
    }

    public BlockTagListing.Listing TagGet(int idx)
    {
        if (!Tags.IsValid())
            return null;
        
        return Tags.GetListing(idx);
    }

    public int TagParamGetCount(BlockTagListing.Listing tag)
    {
        if (!TagParams.IsValid())
            return 0;
        
        return TagParams.GetParamCount(tag);
    }

    public ReadOnlyCollection<BlockTagParams.ParamInfo> TagParamGetList(BlockTagListing.Listing tag)
    {
        if (!TagParams.IsValid())
            return new ReadOnlyCollection<BlockTagParams.ParamInfo>([]);
        
        return TagParams.GetParamsForTag(tag);
    }

    public BlockTagParams.ParamInfo TagParamGet(int idx)
    {
        if (!Tags.IsValid())
            return null;
        
        return TagParams.GetParam(idx);
    }
}