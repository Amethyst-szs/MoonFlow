using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nindot.LMS.Msbp;

public partial class MsbpFile : FileBase
{
    // ====================================================== //
    // ================== Getter Utilities ================== //
    // ====================================================== //

    public bool Color_IsFileContainData() { return ColorLabels.IsValid() && Color.IsValid(); }
    public int Color_GetCount()
    {
        if (!ColorLabels.IsValid()) return 0;
        return ColorLabels.CalcLabelCount();
    }
    public ReadOnlyCollection<string> Color_GetLabelList()
    {
        if (!ColorLabels.IsValid()) return new ReadOnlyCollection<string>([]);
        return ColorLabels.GetLabelList();
    }
    public ReadOnlyCollection<BlockColor.Entry> Color_GetList()
    {
        if (!Color.IsValid()) return null;
        return Color.GetColorList();
    }
    public BlockColor.Entry Color_Get(string labelName)
    {
        if (!Color_IsFileContainData()) return null;

        int idx = ColorLabels.GetItemIndex(labelName);
        if (idx == -1) return null;

        return Color.GetColor(idx);
    }
    public BlockColor.Entry Color_Get(int idx)
    {
        if (!Color_IsFileContainData()) return null;

        return Color.GetColor(idx);
    }
    public int Color_GetIndex(string color)
    {
        if (!Color_IsFileContainData()) return -1;

        return ColorLabels.GetItemIndex(color);
    }
    public string Color_GetLabel(int idx)
    {
        if (!Color_IsFileContainData()) return null;

        return ColorLabels.GetLabelList()[idx];
    }

    // ====================================================== //
    // ================== Setter Utilities ================== //
    // ====================================================== //

    public void Color_AddNew(string name, byte r, byte g, byte b, byte a)
    {
        if (!Color_IsFileContainData()) return;
        BlockColor.Entry entry = new(r, g, b, a);
        Color_AddNew(name, entry);
    }
    public void Color_AddNew(string name, BlockColor.Entry color)
    {
        if (!Color_IsFileContainData()) return;
        int idx = Color.AddColor(color);
        ColorLabels.AddItem(name, idx);
    }
    public void Color_MoveIndex(string name, int newIndex)
    {
        if (!Color_IsFileContainData()) return;
        int oldIndex = ColorLabels.GetItemIndex(name);
        Color.MoveColor(oldIndex, newIndex);
        ColorLabels.MoveItem(name, newIndex);
    }
    public void Color_MoveIndexByOffset(string name, int offset)
    {
        if (!Color_IsFileContainData()) return;
        int oldIndex = ColorLabels.GetItemIndex(name);
        Color.MoveColor(oldIndex, oldIndex + offset);
        ColorLabels.MoveItemByOffset(name, offset);
    }
    public void Color_Remove(string name)
    {
        if (!Color_IsFileContainData()) return;

        int idx = ColorLabels.RemoveItem(name);
        Color.RemoveColor(idx);
    }
}