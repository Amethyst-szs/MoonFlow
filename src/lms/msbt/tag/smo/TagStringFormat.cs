using System;
using System.IO;
using Godot;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementStringFormat : MsbtTagElementWithTextData
{
    public new string TextData
    {
        get { return _textData; }
        set
        {
            byte[] valueBuf = value.ToUtf16Buffer();

            TextDataLength = (ushort)valueBuf.Length;
            DataSize = (ushort)Math.Clamp(TextDataLength + 0x2, 0x4, 0xFFFF);
            IsTextDataInvalid = false;

            _textData = value;
        }
    }

    public MsbtTagElementStringFormat(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
    {
        // Copy data from buffer at pointer
        TextDataLength = BitConverter.ToUInt16(buffer, pointer);
        pointer += 0x2;

        // Ensure validity before reading the TextData
        if (!IsValid())
        {
            // If not valid, set all properties to defaults with an empty string 
            DataSize = GetDataSizeBase();
            TextDataLength = 0x0;

            IsTextDataInvalid = true;

            pointer += 0x2;
            return;
        }

        // This tag group is inconsistent between different tag names.
        // When DataSize is equal to 0x4 and TextDataLength is 0x0, end early.
        // This isn't a sign of being invalid, but instead just specific types don't implement
        // the string functionality despite the whole group having the extra bytes allocated.
        if (DataSize == 0x4 && TextDataLength == 0x0)
        {
            pointer += 0x2;
            return;
        }

        // Now we can safely read the string out
        int endPointer = pointer + TextDataLength;
        TextData = buffer[pointer..endPointer].GetStringFromUtf16();

        pointer = endPointer;
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(TextDataLength);

        if (TextData != null && TextDataLength > 0)
            value.Write(TextData.ToUtf16Buffer());

        if (DataSize == 0x4 && TextDataLength == 0x0)
            value.Write((ushort)0x0000);

        return value.ToArray();
    }

    public override bool IsValid()
    {
        if (IsTextDataInvalid)
            return false;

        if (DataSize % 2 != 0 || TextDataLength % 2 != 0)
            return false;

        if (TextDataLength != 0x0 && DataSize - 0x2 != TextDataLength)
            return false;

        if (TextDataLength == 0x0 && DataSize != 0x4)
            return false;

        return true;
    }

    public override ushort GetDataSizeBase()
    {
        return 0x4;
    }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameFormatString), TagName))
            return Enum.GetName(typeof(TagNameFormatString), TagName);

        return "Unknown";
    }
};