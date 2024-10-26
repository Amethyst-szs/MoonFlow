using System;
using System.IO;
using Godot;

using CommunityToolkit.HighPerformance;

namespace Nindot.MsbtTagLibrary.Smo;

public class MsbtTagElementSystemFontSize : MsbtTagElement
{
    public ushort FontSize = 0;

    public MsbtTagElementSystemFontSize(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
    {
        if (!IsValid())
            return;

        // Copy short from buffer at pointer
        FontSize = BitConverter.ToUInt16(buffer, pointer);
        pointer += 0x2;
    }

    public override string GetText()
    {
        throw new NotImplementedException();
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(FontSize);
        return value.ToArray();
    }

    public override int FixedDataSizeValue()
    {
        return 0x2;
    }
};

public class MsbtTagElementSystemColor : MsbtTagElement
{
    enum ColorTable : ushort
    {
        BLACK = 0,
        YELLOW = 1,
        WHITE = 2,
        RED = 3,
        GREEN = 4,
        BLUE = 5,
        GRAY = 6,
        LIGHT_BLUE = 7,
        RESET = 0xFFFF
    };

    private ushort _color;
    public ushort Color
    {
        get { return _color; }
        set
        {
            if (!Enum.IsDefined(typeof(ColorTable), value)) {
                GD.PushWarning("Attempted to set Tag SystemColor to invalid color, set to reset value instead");
                _color = (ushort)ColorTable.RESET;
            } else {
                _color = value;
            }
        }
    }

    public MsbtTagElementSystemColor(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
    {
        if (!IsValid())
            return;

        // Copy short from buffer at pointer
        Color = BitConverter.ToUInt16(buffer, pointer);
        pointer += 0x2;
    }

    public override string GetText()
    {
        throw new NotImplementedException();
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(Color);
        return value.ToArray();
    }

    public override int FixedDataSizeValue()
    {
        return 0x2;
    }
};

public class MsbtTagElementSystemPageBreak : MsbtTagElement
{
    public MsbtTagElementSystemPageBreak(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
    {
    }

    public override string GetText()
    {
        throw new NotImplementedException();
    }

    public override byte[] GetBytes()
    {
        return CreateMemoryStreamWithHeaderData().ToArray();
    }
};