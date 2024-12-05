using System;
using System.Drawing;
using System.IO;

using CommunityToolkit.HighPerformance;
using Nindot.LMS.Msbp;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementSystemColor : MsbtTagElementSystemBase
{
    private ushort _color;

    public MsbtTagElementSystemColor(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementSystemColor(Msbp.MsbpFile project, string color)
        : base((ushort)TagGroup.System, (ushort)TagNameSystem.Color)
    {
        SetColor(project, color);
    }
    public MsbtTagElementSystemColor(int idx)
        : base((ushort)TagGroup.System, (ushort)TagNameSystem.Color)
    {
        SetColor(idx);
    }
    public MsbtTagElementSystemColor()
        : base((ushort)TagGroup.System, (ushort)TagNameSystem.Color)
    {
        SetColorResetDefault();
    }

    internal override void InitTag(ref int pointer, byte[] buffer, ushort dataSize)
    {
        _color = BitConverter.ToUInt16(buffer, pointer);
        pointer += sizeof(ushort);
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(_color);
        return value.ToArray();
    }

    public override ushort CalcDataSize() { return sizeof(ushort); }

    public ushort GetColorIdx()
    {
        return _color;
    }
    public void GetColor(MsbpFile project, out BlockColor.Entry color, out string name)
    {
        if (_color == 0xFFFF)
        {
            color = new BlockColor.Entry(255, 255, 255, 255);
            name = "Reset to Default";
            return;
        }

        color = project.Color_Get(_color);
        name = project.Color_GetLabel(_color);
    }

    public void SetColor(MsbpFile project, string color)
    {
        _color = (ushort)project.Color_GetIndex(color);
    }
    public void SetColor(int idx)
    {
        _color = (ushort)idx;
    }
    public void SetColorResetDefault()
    {
        _color = 0xFFFF;
    }

    public override string GetTextureName(int _) { return "System_Color"; }
    public override Color GetModulateColor(MsbpFile project)
    {
        if (project == null) return Color.White;

        GetColor(project, out BlockColor.Entry color, out string _);
        return Color.FromArgb(color.R, color.G, color.B);
    }
};