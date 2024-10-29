using System;
using System.Collections.Generic;
using System.IO;
using Godot;

namespace Nindot.LMS.Msbp;

public class BlockTagArrayParams : Block
{
    List<string> NameList = [];

    public BlockTagArrayParams(byte[] data, string listingName, int offset) : base(data, listingName, offset)
    {
    }

    protected override void InitBlock(byte[] data)
    {
        // Read how many names are in array
        ushort nameCount = BitConverter.ToUInt16(data, 0);

        // Iterate over all name offset entries
        for (int i = 0; i < nameCount; i++)
        {
            // Get the offset for the current name in block
            int offset = (int)BitConverter.ToUInt32(data, (i * 4) + 4);

            // Calculate the end offset of the name segment
            int endOffset = offset + sizeof(int);
            while (endOffset < data.Length)
            {
                endOffset++;

                if (data[endOffset - 1] == 0x00)
                    break;
            }

            // Create array segment and append name to list
            NameList.Add(data[offset..endOffset].GetStringFromUtf8());
        }

        return;
    }

    protected override uint CalcDataSize()
    {
        uint size = 0x4; // Tag param count and padding

        foreach (var p in NameList)
        {
            // Count and Padding - String Length - Null Terminator
            size += (uint)(0x4 + p.Length + 0x1);
        }

        return size;
    }

    protected override void WriteBlockData(ref MemoryStream stream)
    {
        throw new NotImplementedException();
    }
}