using System;
using System.IO;

using Nindot.LMS.Msbt.TagLib;

namespace Nindot.LMS.Msbt;

public class SarcMsbtFile(MsbtElementFactory factory, byte[] data, string name, SarcFile sarc)
    : MsbtFile(factory, data, name)
{
    public SarcFile Sarc { get; private set; } = sarc;

    public Exception WriteArchive()
    {
        if (!Sarc.Content.ContainsKey(Name))
            throw new SarcFileException("Missing MsbtFile key!");

        MemoryStream stream = new();
        if (!WriteFile(stream))
            throw new LMSException("Failed to write MsbtFile");

        Sarc.Content[Name] = stream.ToArray();
        return Sarc.WriteArchive();
    }
}