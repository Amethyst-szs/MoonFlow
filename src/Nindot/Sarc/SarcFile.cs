using System;
using System.Collections.ObjectModel;

using CsYaz0;
using SarcLibrary;

using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;
using Nindot.Byml;
using Nindot.Al.EventFlow;
using System.IO;
using Nindot.LMS.Msbp;

namespace Nindot;

public class SarcFile
{
    // ====================================================== //
    // ============ Initilization and Parameters ============ //
    // ====================================================== //

    private Sarc SarcData;

    public SarcFile(Sarc file)
    {
        SarcData = file;
    }

    public static SarcFile FromFilePath(string path)
    {
        byte[] data = File.ReadAllBytes(path);
        return FromBytes(data);
    }

    public static SarcFile FromBytes(byte[] fileCompressed)
    {
        byte[] file = [];

        // Decompress file using Yaz0, and return early if this fails
        try { file = Yaz0.Decompress(fileCompressed); }
        catch { throw new SarcFileException("Yaz0 decompress failed!"); }

        // Convert this decompressed file into a sarc object, and return a failure if empty
        return new SarcFile(Sarc.FromBinary(file));
    }

    // ====================================================== //
    // ================ File Reading Utilities ============== //
    // ====================================================== //

    public int GetFileCount()
    {
        return SarcData.Count;
    }
    public ReadOnlyCollection<string> GetFileList()
    {
        return new ReadOnlyCollection<string>([.. SarcData.Keys]);
    }
    public byte[] GetFile(string name)
    {
        return [.. SarcData[name]];
    }
    public BymlFile GetFileBYML(string name)
    {
        return BymlFile.FromBytes([.. SarcData[name]]);
    }
    public MsbtFile GetFileMSBT(string name, MsbtElementFactory factory)
    {
        return new MsbtFile(factory, [.. SarcData[name]]);
    }
    public MsbpFile GetFileMSBP(string name)
    {
        return new MsbpFile([.. SarcData[name]]);
    }
    public Graph GetFileAlEventFlow(string name, EventFlowFactoryBase nodeFactory)
    {
        return Graph.FromBytes([.. SarcData[name]], nodeFactory);
    }

    // ====================================================== //
    // =============== File Writing Utilities =============== //
    // ====================================================== //

    public bool TryAddFile(string name, byte[] data)
    {
        if (SarcData.ContainsKey(name))
            return false;

        SarcData[name] = data;
        return true;
    }
    public void AddOrReplaceFile(string name, byte[] data)
    {
        SarcData[name] = data;
    }

    public bool TryRenameFile(string oldName, string newName)
    {
        if (!SarcData.ContainsKey(oldName) || SarcData.ContainsKey(newName))
            return false;

        byte[] data = [.. SarcData[oldName]];
        SarcData.Remove(oldName);
        SarcData[newName] = data;
        return true;
    }
    public bool TryRemoveFile(string name)
    {
        if (!SarcData.ContainsKey(name))
            return false;

        SarcData.Remove(name);
        return true;
    }
}

public class SarcFileException(string error) : Exception(error);