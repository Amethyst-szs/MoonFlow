using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nindot.LMS.Msbp;

public partial class MsbpFile : FileBase
{
    // ====================================================== //
    // ================== Getter Utilities ================== //
    // ====================================================== //

    public bool Style_IsFileContainData() { return StyleLabels.IsValid() && Styles.IsValid(); }
    public int Style_GetCount()
    {
        if (!StyleLabels.IsValid()) return 0;
        return StyleLabels.CalcLabelCount();
    }
    public ReadOnlyCollection<string> Style_GetLabelList()
    {
        if (!StyleLabels.IsValid()) return new ReadOnlyCollection<string>([]);
        return StyleLabels.GetLabelList();
    }
    public ReadOnlyCollection<BlockStyles.Style> Style_GetList()
    {
        if (!Styles.IsValid()) return null;
        return Styles.GetStyleList();
    }
    public BlockStyles.Style Style_Get(string labelName)
    {
        if (!Style_IsFileContainData()) return null;

        int idx = StyleLabels.GetItemIndex(labelName);
        if (idx == -1) return null;

        return Styles.GetStyle(idx);
    }
    public uint Style_GetIndex(string labelName)
    {
        if (!Style_IsFileContainData()) return 0xFFFFFFFF;

        int idx = StyleLabels.GetItemIndex(labelName);
        if (idx == -1)
            return 0xFFFFFFFF;

        return (uint)idx;
    }

    // ====================================================== //
    // ================== Setter Utilities ================== //
    // ====================================================== //

    public void Style_AddNew(string name, uint width = 300, uint lines = 1, uint fontIdx = 12, uint colorIdx = 0xFFFFFFFF)
    {
        if (!Style_IsFileContainData()) return;
        BlockStyles.Style entry = new(width, lines, fontIdx, colorIdx);
        Style_AddNew(name, entry);
    }
    public void Style_AddNew(string name, BlockStyles.Style style)
    {
        if (!Style_IsFileContainData()) return;
        int idx = Styles.AddStyle(style);
        StyleLabels.AddItem(name, idx);
    }
    public void Style_MoveIndex(string name, int newIndex)
    {
        if (!Style_IsFileContainData()) return;
        int oldIndex = StyleLabels.GetItemIndex(name);
        Styles.MoveStyle(oldIndex, newIndex);
        StyleLabels.MoveItem(name, newIndex);
    }
    public void Style_MoveIndexByOffset(string name, int offset)
    {
        if (!Style_IsFileContainData()) return;
        int oldIndex = StyleLabels.GetItemIndex(name);
        Styles.MoveStyle(oldIndex, oldIndex + offset);
        StyleLabels.MoveItemByOffset(name, offset);
    }
    public void Style_Remove(string name)
    {
        if (!Style_IsFileContainData()) return;
        int idx = StyleLabels.RemoveItem(name);
        Styles.RemoveStyle(idx);
    }
}