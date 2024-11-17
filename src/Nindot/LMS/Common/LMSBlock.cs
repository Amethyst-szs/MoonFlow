using System;
using System.IO;
using System.Linq;

using CommunityToolkit.HighPerformance;
using System.Text;

namespace Nindot.LMS;

public abstract class Block
{
    public const ushort TYPE_NAME_SIZE = 0x4;
    public const ushort PADDING_SIZE = 0x8;
    public const ushort BLOCK_ALIGNMENT_SIZE = 0x10;

    public readonly string TypeName;
    protected readonly bool IsBlockHeaderOK = false;

    private readonly FileBase Parent;

    public Block(byte[] data, string typeName, int offset, FileBase parent)
    {
        // Assign parent
        Parent = parent;

        // Setup pointer at start of block
        TypeName = typeName;
        int pointer = offset;

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
        IsBlockHeaderOK = true;
        InitBlock(rawData);
    }

    public bool WriteBlock(MemoryStream stream)
    {
        if (TypeName.Length != TYPE_NAME_SIZE)
            return false;

        // This means that the block doesn't exist in the file, just skip it and move on
        if (!IsBlockHeaderOK)
            return true;

        // Write generic block header
        stream.Write(Encoding.Unicode.GetBytes(TypeName));

        uint dataSize = CalcDataSize();
        stream.Write(dataSize);

        // Write padding to reach block data
        byte[] pad = new byte[PADDING_SIZE];
        stream.Write(pad);

        // Write block data with abstract virtual
        long priorToDataPosition = stream.Position;
        WriteBlockData(stream);

        if (stream.Position - priorToDataPosition != dataSize)
            throw new LMSException("LMSBlock failed! Data and DataSize do not match!\n");

        // Pad out stream to align next block with 0x10 grid
        if (stream.Length % BLOCK_ALIGNMENT_SIZE != 0)
        {
            int endPadLength = BLOCK_ALIGNMENT_SIZE - (int)(stream.Length % BLOCK_ALIGNMENT_SIZE);
            byte[] endPad = Enumerable.Repeat((byte)0xAB, endPadLength).ToArray();
            stream.Write(endPad);
        }

        return true;
    }

    protected virtual void InitBlock(byte[] data) { throw new NotImplementedException(); }
    protected abstract uint CalcDataSize();
    protected abstract void WriteBlockData(MemoryStream stream);

    public int LookupBlockOffset(byte[] data)
    {
        int offset = 0;
        while (offset < data.Length)
        {
            int endOffset = offset + (TYPE_NAME_SIZE - 1);
            if (Encoding.UTF8.GetString(data[offset..endOffset]) == TypeName)
            {
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

        if (!IsBlockHeaderOK)
            return false;

        return true;
    }
}