using Godot;

using CsYaz0;
using SarcLibrary;
using System.Collections.ObjectModel;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;

namespace Nindot;

[GlobalClass]
public partial class SarcResource : Resource
{
    // ====================================================== //
    // ============ Initilization and Parameters ============ //
    // ====================================================== //

    private Sarc SarcData;
    
    public SarcResource(Sarc file)
    {
        SarcData = file;
    }

    public static SarcResource FromFilePath(string path)
    {
        if (!FileAccess.FileExists(path))
        {
            GD.PushError("SarcResource does not exist at path ", path);
            return null;
        }

        byte[] data = FileAccess.GetFileAsBytes(path);
        if (data.Length == 0)
        {
            GD.PushError("SarcResource read failed at ", path, " - ", FileAccess.GetOpenError());
            return null;
        }

        return FromBytes(data);
    }

    public static SarcResource FromBytes(byte[] fileCompressed)
    {
        // Decompress file using Yaz0, and return early if this fails
        byte[] bytes = Yaz0.Decompress(fileCompressed);
        if (bytes.IsEmpty())
        {
            GD.PushError("SarcResource failed to decompress from Yaz0!");
            return null;
        }

        // Convert this decompressed file into a sarc object, and return a failure if empty
        return new SarcResource(Sarc.FromBinary(bytes));
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
    public MsbtFile GetFileMSBT(string name, TagLibraryHolder.Type tagLib)
    {
        return new MsbtFile(tagLib, [.. SarcData[name]]);
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