using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Godot;

namespace Nindot.LMS.Msbp;

public class BlockTagListing : Block
{
    public class Listing
    {
        public readonly int ParentIndex;
        public readonly List<ushort> ListingIndexList = [];
        public readonly string Name;

        public Listing(byte[] listingData, int groupIdx)
        {
            ParentIndex = groupIdx;
            int pointer = 0;

            // Read listing count from buffer
            ushort count = BitConverter.ToUInt16(listingData, pointer);
            pointer += 2;

            // Read all listing indexes in the table used to access another block's keys
            while (pointer < (count * 2) + sizeof(ushort))
            {
                ListingIndexList.Add(BitConverter.ToUInt16(listingData, pointer));
                pointer += 2;
            }

            // Read remainder of data into the name, removing the null terminator at the end
            int strLen = listingData.Length - 1;
            Name = listingData[pointer..strLen].GetStringFromUtf8();
        }

        public int CalcSizeBytes()
        {
            //     Listing Count    Byte size of ListingIndexList               Length of name plus null terminator
            return sizeof(ushort) + (ListingIndexList.Count * sizeof(ushort)) + Name.Length + sizeof(byte);
        }

        public void WriteLabel(MemoryStream stream)
        {
        }
    }

    List<Listing> ListingList = [];

    public BlockTagListing(byte[] data, string listingName, int offset) : base(data, listingName, offset)
    {
    }

    protected override void InitBlock(byte[] data)
    {
        // Read how many listings are in project
        ushort listingCount = BitConverter.ToUInt16(data, 0);

        // Iterate over all listing offset entries
        for (int i = 0; i < listingCount; i++)
        {
            // Get the offset for the current listing entry in block
            int offset = (int)BitConverter.ToUInt32(data, (i * 4) + 4);

            // If this block listing is being used for TGG2 (Tag Groups), add two to the offset to properly align offsets
            if (TypeName == "TGG2")
                offset += 2;

            // Get the amount of tags in the current group (will be needed to generate data segment)
            ushort entryCount = BitConverter.ToUInt16(data, offset);

            // Calculate the end offset of this section
            int endOffset = offset + (entryCount * 2) + sizeof(int);

            while (endOffset < data.Length)
            {
                endOffset++;

                if (data[endOffset - 1] == 0x00)
                    break;
            }

            // Create array segment and generate listing data
            byte[] segment = data[offset..endOffset];
            ListingList.Add(new Listing(segment, i));
        }

        return;
    }

    protected override uint CalcDataSize()
    {
        uint size = 0x4; // Count and padding

        foreach (var g in ListingList)
        {
            size += (uint)(0x4 + g.CalcSizeBytes());
        }

        return size;
    }

    protected override void WriteBlockData(MemoryStream stream)
    {
        throw new NotImplementedException();
    }

    public int GetListingCount()
    {
        return ListingList.Count;
    }

    public ReadOnlyCollection<Listing> GetListings()
    {
        return new ReadOnlyCollection<Listing>(ListingList);
    }

    internal ReadOnlyCollection<Listing> GetTagsInGroup(Listing groupTag)
    {
        int tagCount = groupTag.ListingIndexList.Count;
        Listing[] tagList = new Listing[tagCount];

        for (int idx = 0; idx < tagCount; idx++)
        {
            tagList[idx] = ListingList[groupTag.ListingIndexList[idx]];
        }

        return new ReadOnlyCollection<Listing>(tagList);
    }

    public Listing GetListing(int idx)
    {
        if (idx >= ListingList.Count)
            return null;

        return ListingList[idx];
    }
}