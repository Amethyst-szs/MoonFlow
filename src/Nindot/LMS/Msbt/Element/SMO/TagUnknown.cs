using System.IO;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementUnknown : MsbtTagElement
{
    protected byte[] Data;

    public MsbtTagElementUnknown(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }

    internal override void InitTag(ref int pointer, byte[] buffer, ushort dataSize)
    {
        // The base constructor has already been called, so pointer is aligned with tag data
        // Copy all the unknown ambiguous data from this tag into data buffer
        int pointerEnd = pointer + dataSize;
        Data = buffer[pointer..pointerEnd];

        pointer = pointerEnd;
    }

    public override ushort CalcDataSize()
    {
        return (ushort)Data.Length;
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(Data);
        return value.ToArray();
    }
};