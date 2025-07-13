using System;
using System.IO;
using AuroraLib.Compression.Algorithms;

using Nindot;

namespace MoonFlow.Project;

public class EventDataArchive(SarcLibrary.Sarc file, NindotYaz0 yaz0Inst, string filePath) : SarcFile(file, yaz0Inst, filePath)
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
        var yaz0 = new NindotYaz0();

        // Decompress file using Yaz0, and return early if this fails
        try { file = yaz0.Decompress(fileCompressed); }
        catch { throw new SarcFileException("Yaz0 decompress failed!"); }

        // Convert this decompressed file into a sarc object, and return a failure if empty
        var output = new EventDataArchive(SarcLibrary.Sarc.FromBinary(file), yaz0, path)
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