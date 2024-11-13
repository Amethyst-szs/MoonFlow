using System;
using System.IO;
using Godot;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementEuiSpeed : MsbtTagElement
{
    private enum InstantModeTable : ushort
    {
        NORMAL = 0,
        INSTANT_PRINT = 1,
    };

    private ushort _instantMode;
    public ushort InstantMode
    {
        get { return _instantMode; }
        set
        {
            if (!Enum.IsDefined(typeof(InstantModeTable), value))
            {
#if !UNIT_TEST
                GD.PushWarning("Attempted to set Tag PrintSpeed to invalid instant mode, clamped to 1");
#endif

                _instantMode = 1;
            }
            else
            {
                _instantMode = value;
            }
        }
    }

    protected ushort PrintSpeed = 0x803F;

    public MsbtTagElementEuiSpeed(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementEuiSpeed() : base((ushort)TagGroup.Eui, (ushort)TagNameEui.Speed) { }

    internal override void InitTag(ref int pointer, byte[] buffer, ushort dataSize)
    {
        InstantMode = BitConverter.ToUInt16(buffer, pointer);
        pointer += 0x2;

        PrintSpeed = BitConverter.ToUInt16(buffer, pointer);
        pointer += 0x2;
    }

    public void SetPrintSpeedSlow()
    {
        PrintSpeed = 0x003F;
    }

    public void SetPrintSpeedNormal()
    {
        PrintSpeed = 0x803F;
    }

    public void SetPrintSpeedFast()
    {
        PrintSpeed = 0x0040;
    }

    public void SetPrintSpeedVeryFast()
    {
        PrintSpeed = 0x2041;
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(InstantMode);
        value.Write(PrintSpeed);
        return value.ToArray();
    }

    public override ushort CalcDataSize() { return sizeof(uint); }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameEui), TagName))
            return Enum.GetName(typeof(TagNameEui), TagName);

        return "Unknown";
    }
};