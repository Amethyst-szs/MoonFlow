using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbp;

public class BlockTagCommon(byte[] data, string listingName, int offset, MsbpFile parent) : Block(data, listingName, offset, parent)
{
    public enum BlockType : byte
    {
        TGG2, // Tag Group
        TAG2, // Tag
    }

    private BlockType Type;
    private List<TagGroupInfo> List = [];

    protected override void InitBlock(byte[] data)
    {
        // Setup the BlockType property
        switch (TypeName)
        {
            case "TGG2": Type = BlockType.TGG2; break;
            case "TAG2": Type = BlockType.TAG2; break;
            default: return;
        }

        // Read how many groups are in project
        ushort groupCount = BitConverter.ToUInt16(data, 0);

        // Iterate over all group offset entries
        for (int i = 0; i < groupCount; i++)
        {
            // Get the offset for the current group entry in block
            int offset = (int)BitConverter.ToUInt32(data, (i * 4) + 4);

            // Get the amount of elements in the current group (will be needed to generate data segment)
            int entryCountOffset = offset;
            if (Type == BlockType.TGG2)
                entryCountOffset += sizeof(ushort);
            
            ushort entryCount = BitConverter.ToUInt16(data, entryCountOffset);

            // Calculate the end offset of this section
            int endOffset = offset + (entryCount * 2) + sizeof(ushort);
            if (Type == BlockType.TGG2)
                endOffset += sizeof(ushort);

            while (endOffset < data.Length)
            {
                endOffset++;

                if (data[endOffset - 1] == 0x00)
                    break;
            }

            // Create array segment and generate listing data
            byte[] segment = data[offset..endOffset];

            switch (Type)
            {
                case BlockType.TGG2: List.Add(new TagGroupInfo(segment)); break;
                case BlockType.TAG2: List.Add(new TagInfo(segment)); break;
            }
        }

        return;
    }

    protected override uint CalcDataSize()
    {
        uint size = 0x4; // Count and padding

        // Add size of tag offset table
        size += (uint)(List.Count * sizeof(uint));

        // Add size of all group structs
        foreach (var item in List)
        {
            size += item.CalcSizeBytes((int)size);
        }

        return size;
    }

    protected override void WriteBlockData(MemoryStream stream)
    {
        stream.Write((ushort)List.Count);
        stream.Write((ushort)0x0000); // Padding

        // Create offset table before the group info
        uint offset = (uint)(0x4 + (List.Count * sizeof(uint)));

        foreach (var item in List)
        {
            stream.Write(offset);
            offset += item.CalcSizeBytes((int)offset);
        }

        // Now actually write each group's information
        foreach (var item in List)
        {
            item.Write(stream);
        }
    }

    public int GetCount()
    {
        return List.Count;
    }

    public ReadOnlyCollection<TagGroupInfo> GetList()
    {
        return new ReadOnlyCollection<TagGroupInfo>(List);
    }

    public TagGroupInfo GetGroup(int idx)
    {
        if (idx >= List.Count)
            return null;

        return List[idx];
    }

    public TagGroupInfo GetGroup(string label)
    {
        foreach (var tag in List)
        {
            if (tag.Name == label)
                return tag;
        }

        return null;
    }

    public TagInfo GetTag(int idx)
    {
        if (idx >= List.Count)
            return null;

        return (TagInfo)List[idx];
    }

    public TagInfo GetTag(string label)
    {
        foreach (var tag in List)
        {
            if (tag.Name == label)
                return (TagInfo)tag;
        }

        return null;
    }

    public int GetTagIndexInGroup(string label, TagGroupInfo groupTag)
    {
        if (Type != BlockType.TAG2)
            return -1;
        
        for (int i = 0; i < groupTag.ListingIndexList.Count; i++)
        {
            if (List[groupTag.ListingIndexList[i]].Name == label)
                return i;
        }

        return -1;
    }

    internal ReadOnlyCollection<TagInfo> GetTagsInGroup(TagGroupInfo groupTag)
    {
        if (Type != BlockType.TAG2)
            return new ReadOnlyCollection<TagInfo>([]);
        
        int tagCount = groupTag.ListingIndexList.Count;
        TagInfo[] tagList = new TagInfo[tagCount];

        for (int idx = 0; idx < tagCount; idx++)
        {
            tagList[idx] = (TagInfo)List[groupTag.ListingIndexList[idx]];
        }

        return new ReadOnlyCollection<TagInfo>(tagList);
    }
}