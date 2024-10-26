using System;
using System.IO;

using CommunityToolkit.HighPerformance;
using Godot;

namespace Nindot.MsbtTagLibrary.Smo;

public class MsbtTagElement : MsbtBaseElement
{
    protected ushort GroupName = 0xFFFF;
    protected ushort TagName = 0xFFFF;
    protected ushort DataSize = 0x0;

    protected const int TagHeaderSize = 0x08;

    public MsbtTagElement(ref int pointer, byte[] buffer)
    {
        // If the pointer is pointing at an 0x0E or 0x0F, jump ahead to bytes to align with tag group
        ushort startValue = BitConverter.ToUInt16(buffer, pointer);
        if (startValue == Builder.ByteCode_Tag || startValue == Builder.ByteCode_TagClose)
            pointer += 2;
        
        // Setup header
        GroupName = BitConverter.ToUInt16(buffer, pointer);
        pointer += 2;

        TagName = BitConverter.ToUInt16(buffer, pointer);
        pointer += 2;

        DataSize = BitConverter.ToUInt16(buffer, pointer);
        pointer += 2;
    }

    public MemoryStream CreateMemoryStreamWithHeaderData()
    {
        // Create stream to store return
        int size = TagHeaderSize + DataSize;
        MemoryStream value = new(size);

        // Write header properties into stream
        value.Write(Builder.ByteCode_Tag);
        value.Write(GroupName);
        value.Write(TagName);
        value.Write(DataSize);

        return value;
    }

    public ushort GetGroupName()
    {
        return GroupName;
    }

    public ushort GetTagName()
    {
        return TagName;
    }

    public ushort GetDataSize()
    {
        return DataSize;
    }

    public virtual bool IsFixedDataSize()
    {
        return true;
    }

    public virtual int FixedDataSizeValue()
    {
        return 0;
    }

    public override bool IsValid()
    {
        if (!IsFixedDataSize())
            return (DataSize % 2) == 0;
        
        bool result = DataSize == FixedDataSizeValue() && (DataSize % 2) == 0;
        return result;
    }

    public override bool IsTag()
    {
        return true;
    }

    public override string GetText()
    {
        throw new NotImplementedException();
    }

    public override byte[] GetBytes()
    {
        return CreateMemoryStreamWithHeaderData().ToArray();
    }

    public override void WriteBytes(ref MemoryStream stream)
    {
        stream.Write(GetBytes());
    }
}