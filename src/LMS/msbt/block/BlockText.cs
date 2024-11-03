using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.HighPerformance;
using Godot;

namespace Nindot.LMS.Msbt;

public class BlockText(byte[] data, string name, int offset, int charByteSize) : Block(data, name, offset)
{
    private readonly int _charSize = charByteSize;
    public List<byte[]> TextData = [];

    protected override void InitBlock(byte[] data)
    {
        uint count = BitConverter.ToUInt32(data, 0);

        for (int i = 0; i < count; i++)
        {
            int offset = (int)BitConverter.ToUInt32(data, (i * 4) + 4);

            int endPointer;
            if (i < count - 1)
            {
                endPointer = (int)BitConverter.ToUInt32(data, ((i + 1) * 4) + 4);
            }
            else
            {
                endPointer = data.Length;
            }

            // Create array segment and append name to list
            TextData.Add(data[offset..endPointer]);
        }

        return;
    }

    protected override uint CalcDataSize()
    {
        uint size = 0x4; // Content count

        foreach (var p in TextData)
        {
            // Offset + String Length
            size += (uint)(0x4 + p.Length);
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
            offset += (uint)s.Length;
        }

        foreach (var s in TextData)
        {
            stream.Write(s);
        }
    }

    public void UpdateBlock(MsbtEntry[] msbtContents)
    {
        // Ensure the block has a valid header
        if (!IsBlockHeaderOK)
            return;
        
        // Reset text data list
        TextData.Clear();

        // Copy text data from the entry list to the internal TextData array
        foreach (var item in msbtContents)
        {
            TextData.Add(item.GetBytes());
        }
    }

    public uint GetCount() { return (uint)TextData.Count; }
}