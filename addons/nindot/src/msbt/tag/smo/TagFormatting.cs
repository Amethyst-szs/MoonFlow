using System;
using System.IO;
using Godot;

namespace Nindot.MsbtTagLibrary.Smo;

public class MsbtTagElementFormatting : MsbtTagElement
{
    public ushort Unknown1 = 0;
    public ushort Unknown2 = 0;
    public ushort StringLength = 0;

    private string _formatString;
    public string FormatString
    {
        get { return _formatString; }
        set {
            byte[] valueBuf = value.ToUtf16Buffer();

            StringLength = (ushort)valueBuf.Length;
            DataSize = (ushort)(StringLength + 0x6);

            _formatString = value;
        }
    }

    public MsbtTagElementFormatting(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
    {
        // Copy data from buffer at pointer
        Unknown1 = BitConverter.ToUInt16(buffer, pointer);
        pointer += 0x2;

        Unknown2 = BitConverter.ToUInt16(buffer, pointer);
        pointer += 0x2;

        StringLength = BitConverter.ToUInt16(buffer, pointer);
        pointer += 0x2;

        // Before copying the format string, ensure that the string length property is exactly
        // equal to DataSize minus 0x6 (the size of the three proceeding properties)
        if (!IsValid())
            return;

        // Now we can safely read the string out
        int endPointer = pointer + StringLength;
        FormatString = buffer[pointer..endPointer].GetStringFromUtf16();

        pointer = endPointer;
    }

    public override string GetText()
    {
        throw new NotImplementedException();
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        // value.Write(FontSize);
        return value.ToArray();
    }

    public override bool IsValid()
    {
        if (DataSize % 2 != 0 || StringLength % 2 != 0)
            return false;
        
        if (DataSize - 0x6 != StringLength)
            return false;
        
        return true;
    }

    public override bool IsFixedDataSize()
    {
        return false;
    }
};