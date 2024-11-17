using System;
using System.IO;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementEuiSpeed : MsbtTagElement
{
    public float PrintSpeed = 1F;

    public MsbtTagElementEuiSpeed(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementEuiSpeed() : base((ushort)TagGroup.Eui, (ushort)TagNameEui.Speed) { }

    internal override void InitTag(ref int pointer, byte[] buffer, ushort dataSize)
    {
        PrintSpeed = BitConverter.ToSingle(buffer, pointer);
        pointer += sizeof(float);
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(PrintSpeed);
        return value.ToArray();
    }

    public override ushort CalcDataSize() { return sizeof(float); }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameEui), TagName))
            return Enum.GetName(typeof(TagNameEui), TagName);

        return "Unknown";
    }

    public void SetPrintSpeedSlow() { PrintSpeed = 0.5F; }
    public void SetPrintSpeedNormal() { PrintSpeed = 1.0F; }
    public void SetPrintSpeedFast() { PrintSpeed = 2.0F; }
    public void SetPrintSpeedVeryFast() { PrintSpeed = 10.0F; }
};