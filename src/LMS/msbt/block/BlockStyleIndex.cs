using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.HighPerformance;
using Godot;

namespace Nindot.LMS.Msbt;

public class BlockStyleIndex : Block
{
    public List<uint> StyleIndexList = [];
    private uint _initStyleCount = 0;

    public BlockStyleIndex(byte[] data, string name, int offset, uint styleCount) : base(data, name, offset)
    {
        _initStyleCount = styleCount;

        // Parse rest of file data
        int pointer = offset + TYPE_NAME_SIZE;
        uint dataSize = BitConverter.ToUInt32(data, pointer);
        pointer += sizeof(uint);

        // Offset pointer by PADDING_SIZE to reach raw data
        pointer += PADDING_SIZE;

        // Read raw data
        int rawDataEnd = (int)(pointer + dataSize);
        byte[] rawData = data[pointer..rawDataEnd];

        InitBlock(rawData);
    }

    public BlockStyleIndex(List<object> list, string name) : base(list, name)
    {
    }

    protected override void InitBlock(byte[] data)
    {
        if (_initStyleCount == 0)
            return;
        
        for (int i = 0; i < _initStyleCount; i++)
        {
            StyleIndexList.Add(BitConverter.ToUInt32(data, i * sizeof(uint)));
        }
    }

    protected override void InitBlockWithList(List<object> list)
    {
        if (list.GetType() != typeof(List<uint>))
        {
            GD.PushError("Invalid list type in BlockStyleIndex - InitBlockWithList!");
            return;
        }

        StyleIndexList = list.Cast<uint>().ToList();
    }

    protected override uint CalcDataSize()
    {
        return (uint)(StyleIndexList.Count * 4);
    }

    protected override void WriteBlockData(MemoryStream stream)
    {
        stream.Write((uint)StyleIndexList.Count);
        foreach (var styIdx in StyleIndexList)
        {
            stream.Write(styIdx);
        }
    }
}