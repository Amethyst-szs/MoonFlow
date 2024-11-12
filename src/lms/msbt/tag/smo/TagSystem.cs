using System;
using System.IO;
using Godot;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib.Smo;

// Used for rendering Japanese Furigana
public class MsbtTagElementSystemRuby : MsbtTagElementWithTextData
{
    public ushort Code = 0;

    public MsbtTagElementSystemRuby(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
    {
        // Copy data from buffer at pointer
        Code = BitConverter.ToUInt16(buffer, pointer);
        pointer += 0x2;

        ReadTextData(ref pointer, buffer);
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(Code);
        WriteTextData(value);

        return value.ToArray();
    }

    public override ushort GetDataSizeBase()
    {
        return 0x4;
    }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameSystem), TagName))
            return Enum.GetName(typeof(TagNameSystem), TagName);

        return "Unknown";
    }
};

public class MsbtTagElementSystemFont : MsbtTagElement
{
    public TagFontIndex Font = TagFontIndex.MESSAGE_FONT;

    public MsbtTagElementSystemFont(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
    {
        // Copy data from buffer at pointer
        Font = (TagFontIndex)BitConverter.ToUInt16(buffer, pointer);
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(Font);
        return value.ToArray();
    }

    public override ushort GetDataSizeBase()
    {
        return 0x2;
    }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameSystem), TagName))
            return Enum.GetName(typeof(TagNameSystem), TagName);

        return "Unknown";
    }
};

public class MsbtTagElementDeviceFontSize : MsbtTagElement
{
    // Converted to 100-based percentage floating-point number (NEON_ucvtf)
    public ushort FontSize = 100;

    public MsbtTagElementDeviceFontSize(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
    {
        if (!IsValid())
            return;

        // Copy short from buffer at pointer
        FontSize = BitConverter.ToUInt16(buffer, pointer);
        return;
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(FontSize);
        return value.ToArray();
    }

    public override ushort GetDataSizeBase()
    {
        return 0x2;
    }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameSystem), TagName))
            return Enum.GetName(typeof(TagNameSystem), TagName);

        return "Unknown";
    }
};

public class MsbtTagElementSystemColor : MsbtTagElement
{
    private ushort _color;

    public MsbtTagElementSystemColor(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
    {
        if (!IsValid())
            return;

        // Copy short from buffer at pointer
        // Color = BitConverter.ToUInt16(buffer, pointer);
        pointer += 0x2;
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        // value.Write(Color);
        return value.ToArray();
    }

    public override ushort GetDataSizeBase()
    {
        return 0x2;
    }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameSystem), TagName))
            return Enum.GetName(typeof(TagNameSystem), TagName);

        return "Unknown";
    }

    public ushort GetColorIdx()
    {
        return _color;
    }
    public Msbp.BlockColor.Entry GetColor(Msbp.MsbpFile project)
    {
        return project.ColorGet(_color);
    }
    public string GetColorName(Msbp.MsbpFile project)
    {
        return project.ColorGetLabel(_color);
    }

    public void SetColor(Msbp.MsbpFile project, string color)
    {
        // project.ColorGet
    }
};

public class MsbtTagElementSystemPageBreak : MsbtTagElement
{
    public MsbtTagElementSystemPageBreak(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
    {
    }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameSystem), TagName))
            return Enum.GetName(typeof(TagNameSystem), TagName);

        return "Unknown";
    }
};