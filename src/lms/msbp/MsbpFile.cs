using System.Collections.Generic;
using System.Collections.ObjectModel;

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

    private BlockTagCommon TagGroups = null; // TGG2
    private BlockTagCommon Tags = null; // TAG2
    private BlockTagParams TagParams = null; // TGP2
    private BlockTagArrayParams TagArrayParams = null; // TGL2

    private BlockStyles Styles = null; // SYL3
    private BlockHashTable StyleLabels = null; // SLB1

    private BlockProject Project = null; // CTI1

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

        TagGroups = new BlockTagCommon(data, "TGG2", blockKeys.GetValueOrDefault("TGG2", -1));
        Blocks.Add(TagGroups);
        Tags = new BlockTagCommon(data, "TAG2", blockKeys.GetValueOrDefault("TAG2", -1));
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
    // == Everything below this point is for Utility Funcs == //
    // ====================================================== //
    // =================== Color Utilities ================== //
    // ====================================================== //

    public bool ColorIsFileContainData() { return ColorLabels.IsValid() && Color.IsValid(); }
    public int ColorGetCount()
    {
        if (!ColorLabels.IsValid()) return 0;
        return ColorLabels.CalcLabelCount();
    }
    public ReadOnlyCollection<string> ColorGetLabelList()
    {
        if (!ColorLabels.IsValid()) return new ReadOnlyCollection<string>([]);
        return ColorLabels.GetLabelList();
    }
    public ReadOnlyCollection<BlockColor.Entry> ColorGetList()
    {
        if (!Color.IsValid()) return null;
        return Color.GetColorList();
    }
    public BlockColor.Entry ColorGet(string labelName)
    {
        if (!ColorIsFileContainData()) return null;

        int idx = ColorLabels.GetItemIndex(labelName);
        if (idx == -1) return null;

        return Color.GetColor(idx);
    }
    public BlockColor.Entry ColorGet(int idx)
    {
        if (!ColorIsFileContainData()) return null;

        return Color.GetColor(idx);
    }
    public string ColorGetLabel(int idx)
    {
        if (!ColorIsFileContainData()) return null;

        return ColorLabels.GetLabelList()[idx];
    }
    public void ColorAddNew(string name, byte r, byte g, byte b, byte a)
    {
        if (!ColorIsFileContainData()) return;
        BlockColor.Entry entry = new(r, g, b, a);
        ColorAddNew(name, entry);
    }
    public void ColorAddNew(string name, BlockColor.Entry color)
    {
        if (!ColorIsFileContainData()) return;
        int idx = Color.AddColor(color);
        ColorLabels.AddItem(name, idx);
    }
    public void ColorMoveIndex(string name, int newIndex)
    {
        if (!ColorIsFileContainData()) return;
        int oldIndex = ColorLabels.GetItemIndex(name);
        Color.MoveColor(oldIndex, newIndex);
        ColorLabels.MoveItem(name, newIndex);
    }
    public void ColorMoveIndexByOffset(string name, int offset)
    {
        if (!ColorIsFileContainData()) return;
        int oldIndex = ColorLabels.GetItemIndex(name);
        Color.MoveColor(oldIndex, oldIndex + offset);
        ColorLabels.MoveItemByOffset(name, offset);
    }
    public void ColorRemove(string name)
    {
        if (!ColorIsFileContainData()) return;

        int idx = ColorLabels.RemoveItem(name);
        Color.RemoveColor(idx);
    }

    // ====================================================== //
    // ================= Attribute Utilities ================ //
    // ====================================================== //

    public bool AttributeIsFileContainData() { return AttributeInfo.IsValid() && AttributeInfoLabels.IsValid(); }
    public int AttributeGetCount()
    {
        if (!AttributeInfoLabels.IsValid()) return 0;
        return AttributeInfoLabels.CalcLabelCount();
    }
    public ReadOnlyCollection<string> AttributeGetLabelList()
    {
        if (!AttributeInfoLabels.IsValid()) return new ReadOnlyCollection<string>([]);
        return AttributeInfoLabels.GetLabelList();
    }
    public ReadOnlyCollection<BlockAttributeInfo.Entry> AttributeGetList()
    {
        if (!AttributeInfo.IsValid()) return new ReadOnlyCollection<BlockAttributeInfo.Entry>([]);
        return AttributeInfo.GetInfoList();
    }
    public BlockAttributeInfo.Entry AttributeGet(string labelName)
    {
        if (!AttributeIsFileContainData()) return null;

        int idx = AttributeInfoLabels.GetItemIndex(labelName);
        if (idx == -1) return null;

        return AttributeInfo.GetAttribute(idx);
    }
    public ReadOnlyCollection<string> AttributeGetContentArrayList(BlockAttributeInfo.Entry attribute)
    {
        if (attribute.Type != 0x9) return new ReadOnlyCollection<string>([]);
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
        if (!TagGroups.IsValid()) return 0;
        return TagGroups.GetCount();
    }
    public ReadOnlyCollection<TagGroupInfo> TagGroupGetList()
    {
        if (!TagGroups.IsValid()) return new ReadOnlyCollection<TagGroupInfo>([]);
        return TagGroups.GetList();
    }
    public TagGroupInfo TagGroupGet(int idx)
    {
        if (!TagGroups.IsValid()) return null;
        return TagGroups.GetGroup(idx);
    }
    public TagGroupInfo TagGroupGet(string label)
    {
        if (!TagGroups.IsValid()) return null;
        return TagGroups.GetGroup(label);
    }
    public int TagGetCount(TagGroupInfo tagGroup)
    {
        if (!Tags.IsValid()) return 0;
        return tagGroup.ListingIndexList.Count;
    }
    public ReadOnlyCollection<TagInfo> TagGetList(TagGroupInfo tagGroup)
    {
        if (!Tags.IsValid()) return new ReadOnlyCollection<TagInfo>([]);
        return Tags.GetTagsInGroup(tagGroup);
    }
    public TagInfo TagGet(int idx)
    {
        if (!Tags.IsValid()) return null;
        return Tags.GetTag(idx);
    }
    public TagInfo TagGet(string label)
    {
        if (!Tags.IsValid()) return null;
        return Tags.GetTag(label);
    }
    public int TagGetIndex(string label)
    {
        if (!Tags.IsValid()) return -1;
        return Tags.GetTagIndex(label);
    }
    public int TagParamGetCount(TagInfo tag)
    {
        if (!TagParams.IsValid()) return 0;
        return TagParams.GetParamCount(tag);
    }
    public ReadOnlyCollection<TagParamInfo> TagParamGetList(TagInfo tag)
    {
        if (!TagParams.IsValid()) return new ReadOnlyCollection<TagParamInfo>([]);
        return TagParams.GetParamsForTag(tag);
    }

    public TagParamInfo TagParamGet(int idx)
    {
        if (!Tags.IsValid()) return null;
        return TagParams.GetParam(idx);
    }

    // ====================================================== //
    // =================== Style Utilities ================== //
    // ====================================================== //

    public bool StyleIsFileContainData() { return StyleLabels.IsValid() && Styles.IsValid(); }
    public int StyleGetCount()
    {
        if (!StyleLabels.IsValid()) return 0;
        return StyleLabels.CalcLabelCount();
    }
    public ReadOnlyCollection<string> StyleGetLabelList()
    {
        if (!StyleLabels.IsValid()) return new ReadOnlyCollection<string>([]);
        return StyleLabels.GetLabelList();
    }
    public ReadOnlyCollection<BlockStyles.Style> StyleGetList()
    {
        if (!Styles.IsValid()) return null;
        return Styles.GetStyleList();
    }
    public BlockStyles.Style StyleGet(string labelName)
    {
        if (!StyleIsFileContainData()) return null;

        int idx = StyleLabels.GetItemIndex(labelName);
        if (idx == -1) return null;

        return Styles.GetStyle(idx);
    }
    public uint StyleGetIndex(string labelName)
    {
        if (!StyleIsFileContainData()) return 0xFFFFFFFF;

        int idx = StyleLabels.GetItemIndex(labelName);
        if (idx == -1)
            return 0xFFFFFFFF;
        
        return (uint)idx;
    }
    public void StyleAddNew(string name, uint width = 300, uint lines = 1, uint fontIdx = 12, uint colorIdx = 0xFFFFFFFF)
    {
        if (!StyleIsFileContainData()) return;
        BlockStyles.Style entry = new(width, lines, fontIdx, colorIdx);
        StyleAddNew(name, entry);
    }
    public void StyleAddNew(string name, BlockStyles.Style style)
    {
        if (!StyleIsFileContainData()) return;
        int idx = Styles.AddStyle(style);
        StyleLabels.AddItem(name, idx);
    }
    public void StyleMoveIndex(string name, int newIndex)
    {
        if (!StyleIsFileContainData()) return;
        int oldIndex = StyleLabels.GetItemIndex(name);
        Styles.MoveStyle(oldIndex, newIndex);
        StyleLabels.MoveItem(name, newIndex);
    }
    public void StyleMoveIndexByOffset(string name, int offset)
    {
        if (!StyleIsFileContainData()) return;
        int oldIndex = StyleLabels.GetItemIndex(name);
        Styles.MoveStyle(oldIndex, oldIndex + offset);
        StyleLabels.MoveItemByOffset(name, offset);
    }
    public void StyleRemove(string name)
    {
        if (!StyleIsFileContainData()) return;
        int idx = StyleLabels.RemoveItem(name);
        Styles.RemoveStyle(idx);
    }

    // ====================================================== //
    // ================== Project Utilities ================= //
    // ====================================================== //

    public bool ProjectIsFileContainData() { return Project.IsValid(); }
    public int ProjectGetSize() { return Project.GetSize(); }
    public ReadOnlyCollection<string> ProjectGetContent() { return Project.GetContent(); }
    public string ProjectGetElement(int idx) { return Project.GetElement(idx); }

    public void ProjectAddElement(string value) { Project.AddElement(value); }
    public void ProjectRemoveElement(string value) { Project.RemoveElement(value); }
}