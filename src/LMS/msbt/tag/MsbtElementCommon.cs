using System;
using System.IO;
using System.Linq;
using Godot;

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
    private MemoryStream _text = new();

    private string _initial_text = null;

    public MsbtTextElement()
    {
    }

    public MsbtTextElement(string txt)
    {
        _text.Write(txt.ToUtf16Buffer());
    }

    public MsbtTextElement(byte[] buffer)
    {
        _text.Write(buffer);
    }

    public void AppendChar16(ushort value)
    {
        // Don't include null terminators in text elements
        if (value == 0x0000)
            return;

        byte[] code = BitConverter.GetBytes(value);
        _text.Write(code);
    }

    public void FinalizeAppending()
    {
        _initial_text = GetText();
    }

    public bool IsFinalizedAppending()
    {
        return _initial_text != null;
    }

    public override bool IsTag()
    {
        return false;
    }

    public override bool IsValid()
    {
        return _initial_text != null;
    }

    public override string GetText()
    {
        string txt = _text.ToArray().GetStringFromUtf16();
        return txt;
    }

    public override byte[] GetBytes()
    {
        return _text.ToArray();
    }

    public override void WriteBytes(MemoryStream stream)
    {
        byte[] buffer = GetBytes();
        stream.Write(buffer);
    }
}

public class MsbtTagElementGeneric : MsbtBaseElement
{
    private byte[] _buffer;

    public MsbtTagElementGeneric(byte[] buf)
    {
        _buffer = buf;
    }

    public override bool IsTag()
    {
        return true;
    }

    public override bool IsValid()
    {
        return true;
    }

    public override byte[] GetBytes()
    {
        return _buffer;
    }

    public override string GetText()
    {
        return _buffer.GetStringFromUtf16();
    }

    public override void WriteBytes(MemoryStream stream)
    {
        stream.Write(_buffer);
    }
}