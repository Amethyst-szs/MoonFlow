using System;
using System.Drawing;
using System.IO;
using System.Text;
using CommunityToolkit.HighPerformance;

using Nindot.LMS.Msbp;

namespace Nindot.LMS.Msbt.TagLib;

public abstract class MsbtTagElement : MsbtBaseElement
{
    public const ushort BYTECODE_TAG = 0x0E;
    public const int TAG_HEADER_SIZE = 0x08;

    protected ushort GroupName = 0xFFFF;
    protected ushort TagName = 0xFFFF;

    public MsbtTagElement(ref int pointer, byte[] buffer)
    {
        // If the pointer is pointing at a 0x0E, jump ahead two bytes to align with tag group
        ushort startValue = BitConverter.ToUInt16(buffer, pointer);
        if (startValue == BYTECODE_TAG)
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
            throw new MsbtException(string.Format("Invalid InitTag implementation for {0}", GetType()));
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
        bool result = CalcDataSize() % 2 == 0;
        return result;
    }
    public override bool IsTag() { return true; }
    public override bool IsTagClose() { return false; }
    public override bool IsPageBreak() { return false; }

    public string GetTagNameInProject(MsbpFile project)
    {
        TagGroupInfo group = project.TagGroup_Get(GroupName);
        if (group == null || group.ListingIndexList.Count <= TagName)
            return null;
        
        int tagBlockIdx = group.ListingIndexList[TagName];

        TagInfo tag = project.Tag_Get(tagBlockIdx);
        if (tag == null)
            return null;
        
        return tag.Name;
    }

    public ushort GetGroupName() { return GroupName; }
    public ushort GetTagName() { return TagName; }
    public virtual string GetTagNameStr() { return "Unknown"; }
    public override string GetText() { throw new NotImplementedException(); }
    public override byte[] GetBytes() { return CreateMemoryStreamWithHeaderData().ToArray(); }
    public override void WriteBytes(MemoryStream stream) { stream.Write(GetBytes()); }

    public void SetTagNameDangerous(ushort name) { TagName = name; }

    public abstract string GetTextureName(int parameter);
    public virtual Color GetModulateColor(MsbpFile project) { return Color.White; }
}

public abstract class MsbtTagElementWithTextData : MsbtTagElement
{
    public string Text = "";

    internal MsbtTagElementWithTextData(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    internal MsbtTagElementWithTextData(ushort group, ushort tag) : base(group, tag) { }

    internal override void InitTag(ref int pointer, byte[] buffer, ushort dataSize)
    {
        ReadTextData(ref pointer, buffer);
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        WriteTextData(value);
        return value.ToArray();
    }

    public bool ReadTextData(ref int pointer, byte[] buffer)
    {
        // Read length of string
        ushort length = BitConverter.ToUInt16(buffer, pointer);
        pointer += sizeof(ushort);

        // Convert buffer segment to string
        int endPointer = pointer + length;
        if (endPointer >= buffer.Length)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("WARNING: Ran into an invalid MSBT text tag. String will be empty.");
            Console.ForegroundColor = ConsoleColor.Gray;

            Text = "";
            return false;
        }

        Text = Encoding.Unicode.GetString(buffer[pointer..endPointer]);

        pointer = endPointer;
        return true;
    }

    public void WriteTextData(MemoryStream stream)
    {
        // Calculate length of string data
        const ushort wordSize = sizeof(ushort);
        ushort length = (ushort)(Text.Length * wordSize); // UTF16 string length

        if (Text == null)
            throw new MsbtException("Invalid MsbtTagElementWithTextData!");

        stream.Write(length);
        stream.Write(Encoding.Unicode.GetBytes(Text));
    }

    public override ushort CalcDataSize()
    {
        // Get length of string as char16, and add two bytes for string length prefix
        const ushort wordSize = sizeof(ushort);
        return (ushort)((Text.Length * wordSize) + wordSize);
    }
}