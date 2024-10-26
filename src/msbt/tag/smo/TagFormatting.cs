using System;
using System.IO;
using Godot;

using CommunityToolkit.HighPerformance;

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
            IsInvalid = false;

            _formatString = value;
        }
    }

    private bool IsInvalid = false;

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
        if (!IsValid()) {
            // In the event the data isn't valid, wipe the proposed string length and set the data size to just
            // the consistent bytes
            DataSize = 0x6;
            StringLength = 0x0;
            IsInvalid = true;
            return;
        }

        // Now we can safely read the string out
        int endPointer = pointer + StringLength;
        FormatString = buffer[pointer..endPointer].GetStringFromUtf16();

        pointer = endPointer;
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(Unknown1);
        value.Write(Unknown2);
        value.Write(StringLength);

        if (FormatString != null && StringLength > 0)
            value.Write(FormatString.ToUtf16Buffer());
        
        return value.ToArray();
    }

    public override bool IsValid()
    {
        if (IsInvalid)
            return false;
        
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