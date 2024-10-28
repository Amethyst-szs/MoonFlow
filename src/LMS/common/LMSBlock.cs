using System;
using System.IO;
using System.Linq;
using Godot;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS;

public abstract class Block
{
    public const ushort TYPE_NAME_SIZE = 0x4;
    public const ushort PADDING_SIZE = 0x8;
    public const ushort BLOCK_ALIGNMENT_SIZE = 0x10;
    
    public readonly string TypeName;

    public Block(byte[] data, string typeName)
    {
        // Setup pointer at start of block
        TypeName = typeName;
        int pointer = LookupBlockOffset(data);
        if (pointer == -1)
            return;
        
        // Offset pointer by TYPE_NAME_SIZE because we already have the type name from the args
        pointer += TYPE_NAME_SIZE;

        // Parse rest of file data
        uint dataSize = BitConverter.ToUInt32(data, pointer);
        pointer += sizeof(uint);

        // Offset pointer by PADDING_SIZE to reach raw data
        pointer += PADDING_SIZE;

        // Read raw data
        int rawDataEnd = (int)(pointer + dataSize);
        byte[] rawData = data[pointer..rawDataEnd];

        // This method must be overridden by every inheriting class to init the block-specific data
        InitBlock(rawData);
    }

    public bool WriteBlock(ref MemoryStream stream)
    {
        if (!IsValid())
            return false;

        // Write generic block header
        stream.Write(TypeName.ToUtf8Buffer());

        uint dataSize = CalcDataSize();
        stream.Write(dataSize);

        // Write padding to reach block data
        byte[] pad = new byte[PADDING_SIZE];
        stream.Write(pad);

        // Write block data with abstract virtual
        long priorToDataPosition = stream.Position;
        WriteBlockData(ref stream);

        if (stream.Position - priorToDataPosition != dataSize) {
            GD.PushError("LMSBlock didn't write correctly! Data and DataSize do not match!");
            return false;
        }

        // Pad out stream to align next block with 0x10 grid
        if (stream.Length % BLOCK_ALIGNMENT_SIZE != 0) {
            int endPadLength = BLOCK_ALIGNMENT_SIZE - (int)(stream.Length % BLOCK_ALIGNMENT_SIZE);
            byte[] endPad = Enumerable.Repeat((byte)0xAB, endPadLength).ToArray();
            stream.Write(endPad);
        }

        return true;
    }

    protected abstract void InitBlock(byte[] data);
    protected abstract uint CalcDataSize();
    protected abstract void WriteBlockData(ref MemoryStream stream);

    public int LookupBlockOffset(byte[] data)
    {
        int offset = 0;
        while (offset < data.Length)
        {
            int endOffset = offset + (TYPE_NAME_SIZE - 1);
            if (data[offset..endOffset].GetStringFromUtf8() == TypeName) {
                return offset;
            }

            offset += BLOCK_ALIGNMENT_SIZE;
        }

        return -1;
    }

    public virtual bool IsValid()
    {
        if (TypeName.Length != TYPE_NAME_SIZE)
            return false;
        
        return true;
    }
}