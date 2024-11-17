using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbp;

public class BlockTagArrayParams(byte[] data, string listingName, int offset, MsbpFile parent) : Block(data, listingName, offset, parent)
{
    List<string> NameList = [];

    protected override void InitBlock(byte[] data)
    {
        // Read how many names are in array
        ushort nameCount = BitConverter.ToUInt16(data, 0);

        if (nameCount == 0)
            return;

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
            NameList.Add(Encoding.UTF8.GetString(data[offset..endOffset]));
        }
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

    protected override void WriteBlockData(MemoryStream stream)
    {
        stream.Write((ushort)(NameList.Count * sizeof(ushort)));
        stream.Write((ushort)0x0000); // Padding

        int offset = 0x4 + (NameList.Count * sizeof(uint));

        foreach (var item in NameList)
        {
            stream.Write(offset);
            offset += item.Length + 0x1; // + Null Terminator

            while (offset % TagParamInfo.PARAM_ALIGNMENT_SIZE != 0)
            {
                offset += 1;
            }
        }

        foreach (var item in NameList)
        {
            stream.Write(Encoding.UTF8.GetBytes(item));
            stream.Write((byte)0x00); // Null Terminator

            // Align stream to GROUP_ALIGNMENT_SIZE
            while (stream.Position % TagParamInfo.PARAM_ALIGNMENT_SIZE != 0)
            {
                stream.Write([0x00]);
            }
        }
    }
}