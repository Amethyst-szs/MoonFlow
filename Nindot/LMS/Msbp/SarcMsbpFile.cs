using System;
using System.IO;

namespace Nindot.LMS.Msbp;

public class SarcMsbpFile(byte[] data, string name, SarcFile sarc)
    : MsbpFile(data, name)
{
    public SarcFile Sarc { get; private set; } = sarc;

    public Exception WriteArchive()
    {
        if (!Sarc.Content.ContainsKey(Name))
            throw new SarcFileException("Missing MsbpFile key!");

        MemoryStream stream = new();
        if (!WriteFile(stream))
            throw new LMSException("Failed to write MsbpFile");

        Sarc.Content[Name] = stream.ToArray();
        return Sarc.WriteArchive();
    }
}