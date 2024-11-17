using System;
using System.Collections.Generic;
using System.IO;
using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt;

public class BlockStyleIndex : Block
{
    public List<uint> StyleIndexList = [];
    private uint _initStyleCount = 0;

    public BlockStyleIndex(byte[] data, string name, int offset, uint styleCount, MsbtFile parent) : base(data, name, offset, parent)
    {
        if (!IsValid())
            return;

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

    protected override void InitBlock(byte[] data)
    {
        if (_initStyleCount == 0)
            return;

        for (int i = 0; i < _initStyleCount; i++)
        {
            StyleIndexList.Add(BitConverter.ToUInt32(data, i * sizeof(uint)));
        }
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

    public void UpdateBlock(MsbtEntry[] msbtContents)
    {
        // Ensure the block has a valid header
        if (!IsBlockHeaderOK)
            return;
        
        // Reset index list
        StyleIndexList.Clear();

        // Copy index data from the entry list to the internal StyleIndexList
        foreach (var item in msbtContents)
        {
            StyleIndexList.Add(item.StyleIndex);
        }
    }
}