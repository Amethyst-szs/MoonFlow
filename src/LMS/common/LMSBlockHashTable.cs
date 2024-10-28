using System;
using System.IO;
using System.Linq;
using Godot;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS;

public class BlockHashTable : Block
{
    public BlockHashTable(byte[] data, string typeName) : base(data, typeName)
    {
    }

    protected override uint CalcDataSize()
    {
        throw new NotImplementedException();
    }

    protected override void InitBlock(byte[] data, uint dataSize)
    {
        throw new NotImplementedException();
    }

    protected override void WriteBlockData(ref MemoryStream stream)
    {
        throw new NotImplementedException();
    }

    protected ulong CalcHash(string label, uint slotCount)
    {
        ulong hash = 0;

        foreach (char c in label)
        {
            hash = hash * 0x492 + (byte)c;
        }

        return (hash & 0xFFFFFFFF) % slotCount;
    }
}