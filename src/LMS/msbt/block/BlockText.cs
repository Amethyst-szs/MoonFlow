using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.HighPerformance;
using Godot;

namespace Nindot.LMS.Msbt;

public class BlockText : Block
{
    private readonly FileHeader.StringEncoding encoding;
    public List<byte[]> TextData = [];

    public BlockText(byte[] data, string name, int offset, FileHeader.StringEncoding enc) : base(data, name, offset)
    {
        encoding = enc;
    }

    public BlockText(List<object> list, string name, FileHeader.StringEncoding enc) : base(list, name)
    {
        encoding = enc;
    }

    protected override void InitBlock(byte[] data)
    {
        // Read how many mstxt keys are in project
        ushort nameCount = BitConverter.ToUInt16(data, 0);

        // Iterate over every single key (oh boy we in for a long one)
        for (int i = 0; i < nameCount; i++)
        {
            // Get the offset for the current name in block
            int offset = (int)BitConverter.ToUInt32(data, (i * 4) + 4);

            // Calculate the end offset of the name segment
            int endPointer;
            if (i < nameCount - 1)
            {
                endPointer = (int)BitConverter.ToUInt32(data, ((i + 1) * 4) + 4) - 1;
            }
            else
            {
                endPointer = data.Length - 1;
                while (data[endPointer] == 0x00)
                {
                    endPointer--;
                }
                endPointer++;
            }

            // Create array segment and append name to list
            TextData.Add(data[offset..endPointer]);
            continue;
        }

        return;
    }

    protected override void InitBlockWithList(List<object> list)
    {
        if (list.GetType() != typeof(List<byte[]>))
        {
            GD.PushError("Invalid list type in BlockText - InitBlockWithList!");
            return;
        }

        TextData = list.Cast<byte[]>().ToList();
    }

    protected override uint CalcDataSize()
    {
        uint size = 0x4; // Content count

        foreach (var p in TextData)
        {
            // Offset + String Length + Null Terminator
            size += (uint)(0x4 + p.Length + 0x1);
        }

        return size;
    }

    protected override void WriteBlockData(MemoryStream stream)
    {
        stream.Write((uint)TextData.Count);

        uint offset = (uint)(0x4 + (TextData.Count * 0x4));
        foreach (var s in TextData)
        {
            stream.Write(offset);
            offset += (uint)(s.Length + 0x1); // Null terminator included
        }

        foreach (var s in TextData)
        {
            stream.Write(s);
            stream.Write([0x00]); // Null terminator
        }
    }

    public uint GetCount() { return (uint)TextData.Count; }
}