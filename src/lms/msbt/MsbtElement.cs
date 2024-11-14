using System;
using System.IO;
using Godot;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib;

public abstract class MsbtBaseElement
{
    public abstract bool IsTag();
    public abstract bool IsValid();
    public abstract string GetText();
    public abstract byte[] GetBytes();
    public abstract void WriteBytes(MemoryStream stream);
}

public class MsbtTextElement : MsbtBaseElement
{
    public string Text = "";

    public MsbtTextElement() { }
    public MsbtTextElement(string txt) { Text = txt; }
    public MsbtTextElement(byte[] buffer)
    {
        Text = buffer.GetStringFromUtf16();
        RemoveNullTerminator();
    }

    public override bool IsTag() { return false; }
    public override bool IsValid() { return true; }
    public override string GetText() { return Text; }

    public override byte[] GetBytes()
    {
        RemoveNullTerminator();
        var buf = Text.ToUtf16Buffer();
        return buf;
    }

    public override void WriteBytes(MemoryStream stream)
    {
        byte[] buffer = GetBytes();
        stream.Write(buffer);
    }

    public bool IsEmpty() { return Text == string.Empty; }
    public void RemoveNullTerminator() { Text = Text.Replace("\0", ""); }
}

public class MsbtTagCloseElement : MsbtBaseElement
{
    public const ushort BYTECODE_TAG_CLOSE = 0x0F;
    public const int STRUCT_SIZE = 0x6;

    public MsbtTagCloseElement(ref int pointer, byte[] buffer)
    {
        pointer += STRUCT_SIZE;
    }

    public override bool IsTag() { return true; }

    public override bool IsValid() { return true; }

    public override string GetText() { throw new NotImplementedException(); }

    public override byte[] GetBytes()
    {
        var stream = new MemoryStream();
        stream.Write(BYTECODE_TAG_CLOSE);
        stream.Write((uint)0x00000000); // Padding
        return stream.ToArray();
    }

    public override void WriteBytes(MemoryStream stream)
    {
        byte[] buffer = GetBytes();
        stream.Write(buffer);
    }
}