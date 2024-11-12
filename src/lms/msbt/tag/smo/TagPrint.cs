using System;
using System.IO;
using Godot;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementPrintDelay : MsbtTagElement
{
    public ushort DelayFrames = 0;

    private enum SkipModeTable : ushort
    {
        NORMAL = 0,
        INPUT_REQUIRED = 1,
    };

    private ushort _skipMode;
    public ushort SkipMode
    {
        get { return _skipMode; }
        set
        {
            if (!Enum.IsDefined(typeof(SkipModeTable), value))
            {
#if !UNIT_TEST
                GD.PushWarning("Attempted to set Tag PrintDelay to invalid skip mode, clamped to 1");
#endif

                _skipMode = 1;
            }
            else
            {
                _skipMode = value;
            }
        }
    }

    public MsbtTagElementPrintDelay(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
    {
        if (!IsValid())
            return;

        // Copy data from buffer at pointer
        DelayFrames = BitConverter.ToUInt16(buffer, pointer);
        pointer += 0x2;

        SkipMode = BitConverter.ToUInt16(buffer, pointer);
        pointer += 0x2;
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(DelayFrames);
        value.Write(SkipMode);
        return value.ToArray();
    }

    public override ushort GetDataSizeBase()
    {
        return 0x4;
    }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameEui), TagName))
            return Enum.GetName(typeof(TagNameEui), TagName);

        return "Unknown";
    }
};

public class MsbtTagElementPrintSpeed : MsbtTagElement
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

    public MsbtTagElementPrintSpeed(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
    {
        if (!IsValid())
            return;

        // Copy data from buffer at pointer
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

    public override ushort GetDataSizeBase()
    {
        return 0x4;
    }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameEui), TagName))
            return Enum.GetName(typeof(TagNameEui), TagName);

        return "Unknown";
    }
};