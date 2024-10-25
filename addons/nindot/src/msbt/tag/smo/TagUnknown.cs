using System.IO;

namespace Nindot.MsbtTagLibrary.Smo;

class MsbtTagElementUnknown : MsbtTagElement
{
    internal byte[] Data;

    public MsbtTagElementUnknown(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
    {
        // If DataSize is equal to zero, return early
        if (DataSize == 0)
            return;
        
        // The base constructor has already been called, so pointer is aligned with tag data
        // Copy all the unknown ambiguous data from this tag into data buffer
        int pointerEnd = pointer + DataSize;
        Data = buffer[pointer..pointerEnd];

        pointer = pointerEnd;
    }

    public override string GetText()
    {
        throw new System.NotImplementedException();
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(Data);
        return value.ToArray();
    }
};