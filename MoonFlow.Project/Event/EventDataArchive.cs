using System;
using System.IO;

using Nindot;

using CsYaz0;

namespace MoonFlow.Project;

public class EventDataArchive(SarcLibrary.Sarc file, string filePath) : SarcFile(file, filePath)
{
    public enum ArchiveSource
    {
        PROJECT,
        ROMFS
    }

    public ArchiveSource Source { get; private set; } = ArchiveSource.ROMFS;

    public static EventDataArchive FromFilePath(string path, ArchiveSource source)
    {
        byte[] data = File.ReadAllBytes(path);
        return FromBytes(data, path, source);
    }
    public static EventDataArchive FromBytes(byte[] fileCompressed, string path, ArchiveSource source)
    {
        byte[] file;

        // Decompress file using Yaz0, and return early if this fails
        try { file = Yaz0.Decompress(fileCompressed); }
        catch { throw new SarcFileException("Yaz0 decompress failed!"); }

        // Convert this decompressed file into a sarc object, and return a failure if empty
        var output = new EventDataArchive(SarcLibrary.Sarc.FromBinary(file), path)
        {
            Source = source
        };
        
        return output;
    }

    public override Exception WriteArchive(string path)
    {
        Source = ArchiveSource.PROJECT;
        return base.WriteArchive(path);
    }
}