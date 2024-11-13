using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nindot.LMS.Msbp;

public partial class MsbpFile : FileBase
{
    public bool Tag_IsFileContainData()
    {
        return TagGroups.IsValid() && Tags.IsValid() && TagParams.IsValid() && TagArrayParams.IsValid();
    }

    // ====================================================== //
    // ================= Tag Group Utilities ================ //
    // ====================================================== //

    public int TagGroup_GetCount()
    {
        if (!TagGroups.IsValid()) return 0;
        return TagGroups.GetCount();
    }
    public ReadOnlyCollection<TagGroupInfo> TagGroup_GetList()
    {
        if (!TagGroups.IsValid()) return new ReadOnlyCollection<TagGroupInfo>([]);
        return TagGroups.GetList();
    }
    public TagGroupInfo TagGroup_Get(int idx)
    {
        if (!TagGroups.IsValid()) return null;
        return TagGroups.GetGroup(idx);
    }
    public TagGroupInfo TagGroup_Get(string label)
    {
        if (!TagGroups.IsValid()) return null;
        return TagGroups.GetGroup(label);
    }

    // ====================================================== //
    // ==================== Tag Utilities =================== //
    // ====================================================== //

    public int Tag_GetCount(TagGroupInfo tagGroup)
    {
        if (!Tags.IsValid()) return 0;
        return tagGroup.ListingIndexList.Count;
    }
    public ReadOnlyCollection<TagInfo> Tag_GetList(TagGroupInfo tagGroup)
    {
        if (!Tags.IsValid()) return new ReadOnlyCollection<TagInfo>([]);
        return Tags.GetTagsInGroup(tagGroup);
    }
    public TagInfo Tag_Get(int idx)
    {
        if (!Tags.IsValid()) return null;
        return Tags.GetTag(idx);
    }
    public TagInfo Tag_Get(string label)
    {
        if (!Tags.IsValid()) return null;
        return Tags.GetTag(label);
    }
    public int Tag_GetIndex(string label)
    {
        if (!Tags.IsValid()) return -1;
        return Tags.GetTagIndex(label);
    }

    // ====================================================== //
    // ================= Tag Param Utilities ================ //
    // ====================================================== //

    public int TagParam_GetCount(TagInfo tag)
    {
        if (!TagParams.IsValid()) return 0;
        return TagParams.GetParamCount(tag);
    }
    public ReadOnlyCollection<TagParamInfo> TagParam_GetList(TagInfo tag)
    {
        if (!TagParams.IsValid()) return new ReadOnlyCollection<TagParamInfo>([]);
        return TagParams.GetParamsForTag(tag);
    }

    public TagParamInfo TagParam_Get(int idx)
    {
        if (!Tags.IsValid()) return null;
        return TagParams.GetParam(idx);
    }
}