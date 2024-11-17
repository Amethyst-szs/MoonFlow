using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbp;

public class TagGroupInfo
{
    private const uint GROUP_ALIGNMENT_SIZE = 0x4;
    private readonly bool IsGroupInfo = false;

    public readonly ushort GroupID;
    public readonly List<ushort> ListingIndexList = [];
    public readonly string Name;

    public TagGroupInfo(byte[] listingData)
    {
        int pointer = 0;

        // Check to see if this info block is a group or tag
        if (GetType() != typeof(TagInfo)) {
            IsGroupInfo = true;

            // Get the GroupID number, equal to the index of this group in the block's table
            GroupID = BitConverter.ToUInt16(listingData, pointer);
            pointer += sizeof(ushort);
        }

        // Read tag count from buffer
        ushort count = BitConverter.ToUInt16(listingData, pointer);
        pointer += sizeof(ushort);

        // Read all tag indexes in the table, used to access TAG2 block's tags
        int indexTableOffset = pointer;
        while (pointer < (count * 2) + indexTableOffset)
        {
            ListingIndexList.Add(BitConverter.ToUInt16(listingData, pointer));
            pointer += 2;
        }

        // Read remainder of data into the name, removing the null terminator at the end
        int strLen = listingData.Length - 1;
        Name = Encoding.UTF8.GetString(listingData[pointer..strLen]);
    }

    public uint CalcSizeBytes(int position)
    {
        // Tag Count, index list size in bytes, name length, null terminator
        int size = sizeof(ushort) + (ListingIndexList.Count * sizeof(ushort)) + Name.Length + 0x1;

        // If this is a tag group and not a tag, append an additional two bytes for GroupID
        if (IsGroupInfo)
            size += sizeof(ushort);

        // The size must align onto a GROUP_ALIGNMENT_SIZE grid, use the position and size to calculate
        while ((position + size) % GROUP_ALIGNMENT_SIZE != 0)
        {
            size += 1;
        }

        return (uint)size;
    }

    public void Write(MemoryStream stream)
    {
        if (IsGroupInfo)
            stream.Write(GroupID);
        
        stream.Write((ushort)ListingIndexList.Count);

        foreach (var idx in ListingIndexList)
        {
            stream.Write(idx);
        }

        stream.Write(Encoding.UTF8.GetBytes(Name));
        stream.Write((byte)0x00); // Null Terminator

        // Align stream to GROUP_ALIGNMENT_SIZE
        while (stream.Position % GROUP_ALIGNMENT_SIZE != 0)
        {
            stream.Write([0x00]);
        }
    }

    public bool IsGroup() { return IsGroupInfo; }
    public bool IsTag() { return !IsGroupInfo; }
}

public class TagInfo(byte[] listingData) : TagGroupInfo(listingData)
{
}