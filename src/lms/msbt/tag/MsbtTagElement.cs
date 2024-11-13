using System;
using System.IO;

using CommunityToolkit.HighPerformance;
using Godot;

namespace Nindot.LMS.Msbt.TagLib;

public abstract class MsbtTagElement : MsbtBaseElement
{
    public const ushort BYTECODE_TAG = 0x0E;
    public const ushort BYTECODE_TAG_CLOSE = 0x0F;
    public const int TAG_HEADER_SIZE = 0x08;

    protected ushort GroupName = 0xFFFF;
    protected ushort TagName = 0xFFFF;

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

        ushort dataSize = BitConverter.ToUInt16(buffer, pointer);
        pointer += 2;

        // Initilize data of tag with abstract function
        int pointerPosBeforeInit = pointer;
        InitTag(ref pointer, buffer, dataSize);

        // Ensure the pointer has moved exactly dataSize
        if (pointer - pointerPosBeforeInit != dataSize)
            GD.PushError("Invalid InitTag implementation! - ", GetTagNameStr());
    }

    internal MsbtTagElement(ushort group, ushort tag)
    {
        GroupName = group;
        TagName = tag;
    }

    internal abstract void InitTag(ref int pointer, byte[] buffer, ushort dataSize);
    public abstract ushort CalcDataSize();

    public MemoryStream CreateMemoryStreamWithHeaderData()
    {
        // Create stream to store return
        ushort dataSize = CalcDataSize();
        int size = TAG_HEADER_SIZE + dataSize;
        MemoryStream value = new(size);

        // Write header properties into stream
        value.Write(BYTECODE_TAG);
        value.Write(GroupName);
        value.Write(TagName);
        value.Write(dataSize);

        return value;
    }

    public override bool IsValid()
    {
        bool result = CalcDataSize() % 2 != 0;
        return result;
    }
    public override bool IsTag() { return true; }

    public ushort GetGroupName() { return GroupName; }
    public ushort GetTagName() { return TagName; }
    public virtual string GetTagNameStr() { return "Unknown"; }
    public override string GetText() { throw new NotImplementedException(); }
    public override byte[] GetBytes() { return CreateMemoryStreamWithHeaderData().ToArray(); }
    public override void WriteBytes(MemoryStream stream) { stream.Write(GetBytes()); }
}

public abstract class MsbtTagElementWithTextData : MsbtTagElement
{
    public string Text = "";

    internal MsbtTagElementWithTextData(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    internal MsbtTagElementWithTextData(ushort group, ushort tag) : base(group, tag) { }

    public bool ReadTextData(ref int pointer, byte[] buffer)
    {
        // Read length of string
        ushort length = BitConverter.ToUInt16(buffer, pointer);
        pointer += sizeof(ushort);

        // Convert buffer segment to string
        int endPointer = pointer + length;
        Text = buffer[pointer..endPointer].GetStringFromUtf16();

        pointer = endPointer;
        return true;
    }

    public void WriteTextData(MemoryStream stream)
    {
        // Calculate length of string data
        const ushort wordSize = sizeof(ushort);
        ushort length = (ushort)(Text.Length * wordSize); // UTF16 string length
        stream.Write(length);

        if (Text != null && length > 0)
            stream.Write(Text.ToUtf16Buffer());
    }

    public override ushort CalcDataSize()
    {
        // Get length of string as char16, and add two bytes for string length prefix
        const ushort wordSize = sizeof(ushort);
        return (ushort)((Text.Length * wordSize) + wordSize);
    }
}