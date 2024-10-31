using System;
using System.IO;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementUnknown : MsbtTagElement
{
    protected byte[] Data;

    public MsbtTagElementUnknown(ref int pointer, byte[] buffer, MsbtFile parent) : base(ref pointer, buffer, parent)
    {
        if (!IsValid())
            return;

        // The base constructor has already been called, so pointer is aligned with tag data
        // Copy all the unknown ambiguous data from this tag into data buffer
        int pointerEnd = pointer + DataSize;
        Data = buffer[pointer..pointerEnd];

        pointer = pointerEnd;
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(Data);
        return value.ToArray();
    }

    public override bool IsFixedDataSize()
    {
        return false;
    }
};