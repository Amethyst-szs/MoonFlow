using System;
using System.IO;

using CommunityToolkit.HighPerformance;
using Godot;

namespace Nindot.LMS.Msbt.TagLib;

public class MsbtTagElement : MsbtBaseElement
{
    public const ushort BYTECODE_TAG = 0x0E;
    public const ushort BYTECODE_TAG_CLOSE = 0x0F;
    public const int TAG_HEADER_SIZE = 0x08;

    protected ushort GroupName = 0xFFFF;
    protected ushort TagName = 0xFFFF;
    protected ushort DataSize = 0x0;

    public MsbtTagElement(ref int pointer, byte[] buffer)
    {
        // If the pointer is pointing at an 0x0E or 0x0F, jump ahead to bytes to align with tag group
        ushort startValue = BitConverter.ToUInt16(buffer, pointer);
        if (startValue == BYTECODE_TAG || startValue == BYTECODE_TAG_CLOSE)
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
        int size = TAG_HEADER_SIZE + DataSize;
        MemoryStream value = new(size);

        // Write header properties into stream
        value.Write(BYTECODE_TAG);
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

    public virtual ushort GetDataSizeBase()
    {
        return 0x0;
    }

    public override bool IsValid()
    {
        if (!IsFixedDataSize())
            return (DataSize % 2) == 0;

        bool result = DataSize == GetDataSizeBase();
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

    public override void WriteBytes(MemoryStream stream)
    {
        stream.Write(GetBytes());
    }

    public virtual string GetTagNameStr()
    {
        return "Unknown";
    }
}

public class MsbtTagElementWithTextData : MsbtTagElement
{
    protected ushort TextDataLength = 0;
    protected bool IsTextDataInvalid = false;

    protected string _textData = "";
    public string TextData
    {
        get { return _textData; }
        set
        {
            byte[] valueBuf = value.ToUtf16Buffer();

            TextDataLength = (ushort)valueBuf.Length;
            DataSize = (ushort)(TextDataLength + GetDataSizeBase());
            IsTextDataInvalid = false;

            _textData = value;
        }
    }

    public MsbtTagElementWithTextData(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
    {
        if (GetType() == typeof(MsbtTagElementWithTextData))
            throw new NotImplementedException();
    }

    public bool ReadTextData(ref int pointer, byte[] buffer)
    {
        TextDataLength = BitConverter.ToUInt16(buffer, pointer);
        pointer += 0x2;

        // Ensure validity before reading the TextData
        if (!IsValid())
        {
            // If not valid, set all properties to defaults with an empty string 
            DataSize = GetDataSizeBase();
            TextDataLength = 0x0;

            IsTextDataInvalid = true;
            return false;
        }

        // Now we can safely read the string out
        int endPointer = pointer + TextDataLength;
        TextData = buffer[pointer..endPointer].GetStringFromUtf16();

        pointer = endPointer;
        return true;
    }

    public void WriteTextData(MemoryStream stream)
    {
        stream.Write(TextDataLength);

        if (TextData != null && TextDataLength > 0)
            stream.Write(TextData.ToUtf16Buffer());
    }

    public override bool IsValid()
    {
        if (IsTextDataInvalid)
            return false;

        if (DataSize % 2 != 0 || TextDataLength % 2 != 0)
            return false;

        if (DataSize - GetDataSizeBase() != TextDataLength)
            return false;

        return true;
    }

    public override bool IsFixedDataSize()
    {
        return false;
    }
}