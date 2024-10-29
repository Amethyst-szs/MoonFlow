using System;
using System.Collections.Generic;
using System.IO;
using Godot;

namespace Nindot.LMS.Msbp;

public class BlockTagListing : Block
{
    public struct Listing
    {
        public ushort ListingCount;
        public List<ushort> ListingIndexList = [];
        public string Name;

        public Listing(byte[] listingData)
        {
            int pointer = 0;

            // Read listing count from buffer
            ListingCount = BitConverter.ToUInt16(listingData, pointer);
            pointer += 2;

            // Read all listing indexes in the table used to access another block's keys
            while (pointer < (ListingCount * 2) + sizeof(ushort))
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
            //     ListingCount     Byte size of ListingIndexList               Length of name plus null terminator
            return sizeof(ushort) + (ListingIndexList.Count * sizeof(ushort)) + Name.Length + sizeof(byte);
        }

        public void WriteLabel(ref MemoryStream stream)
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
            ListingList.Add(new Listing(segment));
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

    protected override void WriteBlockData(ref MemoryStream stream)
    {
        throw new NotImplementedException();
    }
}