using System;
using System.IO;
using System.Linq;

using Nindot.Al.EventFlow;
using Nindot.Byml;
using Nindot.LMS.Msbp;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;

namespace Nindot;

public class SarcFile(SarcLibrary.Sarc file, string filePath)
{
    // ====================================================== //
    // ============ Initilization and Parameters ============ //
    // ====================================================== //

    public SarcLibrary.Sarc Content { get; private set; } = file;

    private string _filePath = filePath;
    public string FilePath
    {
        get { return _filePath; }
        set
        {
            _filePath = value;
            _name = value.Split('/', '\\').Last();
        }
    }

    private string _name = filePath.Split(['/', '\\']).Last();
    public string Name
    {
        get { return _name; }
        set
        {
            _filePath = _filePath[.._filePath.IndexOf(_name)] + value;
            _name = value;
        }
    }

    public static SarcFile FromFilePath(string path)
    {
        byte[] data = File.ReadAllBytes(path);
        return FromBytes(data, path);
    }
    public static SarcFile FromBytes(byte[] fileCompressed, string path)
    {
        byte[] file;

        // Decompress file using Yaz0, and return early if this fails
        try { file = NindotYaz0.Decompress(fileCompressed); }
        catch { throw new SarcFileException("Yaz0 decompress failed!"); }

        // Convert this decompressed file into a sarc object, and return a failure if empty
        return new SarcFile(SarcLibrary.Sarc.FromBinary(file), path);
    }

    public Exception WriteArchive() { return WriteArchive(FilePath); }
    public virtual Exception WriteArchive(string path) { return WriteArchive(Content, path); }
    public static Exception WriteArchive(SarcLibrary.Sarc sarcBase, string path)
    {
        MemoryStream stream = new();
        sarcBase.Write(stream);

        try
        {
            var result = NindotYaz0.Compress(stream);
            File.WriteAllBytes(path, [.. result]);
        }
        catch (Exception e) { return e; }

        return null;
    }

    public byte[] GetBytes() { return GetBytes(Content); }
    public static byte[] GetBytes(SarcLibrary.Sarc sarcBase)
    {
        MemoryStream stream = new();
        sarcBase.Write(stream);

        return NindotYaz0.Compress(stream);
    }

    // ====================================================== //
    // ================ File Reading Utilities ============== //
    // ====================================================== //

    public DateTime GetLastModifiedTime() { return File.GetLastAccessTime(FilePath); }

    public BymlFile GetFileBYML(string name)
    {
        return BymlFile.FromBytes([.. Content[name]]);
    }
    public SarcMsbtFile GetFileMSBT(string name, MsbtElementFactory factory)
    {
        return new SarcMsbtFile(factory, [.. Content[name]], name, this);
    }
    public SarcMsbpFile GetFileMSBP(string name)
    {
        return new SarcMsbpFile([.. Content[name]], name, this);
    }
    public SarcEventFlowGraph GetFileEventFlow(string name, EventFlowFactoryBase nodeFactory)
    {
        if (!BymlFileAccess.ParseBytes(out BymlFile file, [.. Content[name]]))
            return null;

        return new SarcEventFlowGraph(file, name, nodeFactory, this);
    }

    // ====================================================== //
    // =============== File Writing Utilities =============== //
    // ====================================================== //

    public bool TryRenameFile(string oldName, string newName)
    {
        if (!Content.TryGetValue(oldName, out ArraySegment<byte> data) || Content.ContainsKey(newName))
            return false;

        Content.Remove(oldName);
        Content[newName] = data;
        return true;
    }
}

public class SarcFileException(string error) : Exception(error);