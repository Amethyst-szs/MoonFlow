using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nindot.LMS.Msbp;

public partial class MsbpFile : FileBase
{
    // ====================================================== //
    // ================== Getter Utilities ================== //
    // ====================================================== //

    public bool Attribute_IsFileContainData() { return AttributeInfo.IsValid() && AttributeInfoLabels.IsValid(); }
    public int Attribute_GetCount()
    {
        if (!AttributeInfoLabels.IsValid()) return 0;
        return AttributeInfoLabels.CalcLabelCount();
    }
    public ReadOnlyCollection<string> Attribute_GetLabelList()
    {
        if (!AttributeInfoLabels.IsValid()) return new ReadOnlyCollection<string>([]);
        return AttributeInfoLabels.GetLabelList();
    }
    public ReadOnlyCollection<BlockAttributeInfo.Entry> Attribute_GetList()
    {
        if (!AttributeInfo.IsValid()) return new ReadOnlyCollection<BlockAttributeInfo.Entry>([]);
        return AttributeInfo.GetInfoList();
    }
    public BlockAttributeInfo.Entry Attribute_Get(string labelName)
    {
        if (!Attribute_IsFileContainData()) return null;

        int idx = AttributeInfoLabels.GetItemIndex(labelName);
        if (idx == -1) return null;

        return AttributeInfo.GetAttribute(idx);
    }
    public ReadOnlyCollection<string> Attribute_GetContentArrayList(BlockAttributeInfo.Entry attribute)
    {
        if (attribute.Type != 0x9) return new ReadOnlyCollection<string>([]);
        return AttributeLists.GetList(attribute.ListIndex);
    }
}