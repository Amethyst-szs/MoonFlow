using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Nindot.LMS.Msbt.TagLib;

namespace Nindot.LMS.Msbt;

public partial class MsbtFile : FileBase
{
    // ====================================================== //
    // ================ Constructor Utilities =============== //
    // ====================================================== //

    static public MsbtFile FromFilePath(string path, MsbtElementFactory factory)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException(path);

        byte[] bytes;

        try
        {
            bytes = File.ReadAllBytes(path);
        }
        catch
        {
            throw new FileLoadException(path);
        }


        return FromBytes(bytes, path.Split(['/', '\\']).Last(), factory);
    }

    static public MsbtFile FromBytes(byte[] data, string name, MsbtElementFactory factory)
    {
        MsbtFile file;

        try
        {
            file = new(factory, data, name);
        }
        catch (MsbtEntryParserException)
        {
            throw;
        }
        catch
        {
            throw new MsbtException("Failed to parse MsbtFile");
        }

        return file;
    }

    // ====================================================== //
    // ================== Getter Utilities ================== //
    // ====================================================== //

    public bool IsContainKey(string label)
    {
        return Content.ContainsKey(label);
    }
    public int GetEntryCount()
    {
        return Content.Count;
    }
    public MsbtEntry GetEntry(int idx)
    {
        if (idx >= Content.Count)
            return null;

        return Content.Values.ElementAt(idx);
    }
    public MsbtEntry GetEntry(string label)
    {
        Content.TryGetValue(label, out MsbtEntry entry);
        return entry;
    }
    public int GetEntryIndex(string label)
    {
        return Array.FindIndex([.. Content.Keys], x => x.Equals(label));
    }
    public string GetEntryLabel(int idx)
    {
        if (idx < 0 || idx >= Content.Count)
            return null;

        return Content.Keys.ElementAt(idx);
    }
    public ReadOnlyCollection<string> GetEntryLabels()
    {
        return new ReadOnlyCollection<string>([.. Content.Keys]);
    }

    // ====================================================== //
    // ========= File Content Modification Utilities ======== //
    // ====================================================== //

    public MsbtEntry AddEntry(string label)
    {
        MsbtEntry entry = new(Factory, label);
        Content.Add(label, entry);
        return entry;
    }
    public MsbtEntry AddEntry(string label, string textContent)
    {
        MsbtEntry entry = new(Factory, label, textContent);
        Content.Add(label, entry);
        return entry;
    }
    public MsbtEntry AddEntry(string label, MsbtEntry entry)
    {
        entry.Name = label;
        Content.Add(label, entry);
        return entry;
    }

    public void RenameEntry(string name, string nameNew)
    {
        if (!Content.TryGetValue(name, out MsbtEntry entry)) return;

        Content.Remove(name);

        entry.Name = nameNew;
        Content.Add(nameNew, entry);
    }

    public bool RemoveEntry(string label)
    {
        if (!Content.ContainsKey(label))
            return false;

        Content.Remove(label);
        return true;
    }

    public bool ReplaceEntry(string label, MsbtEntry newEntry)
    {
        if (!Content.ContainsKey(label))
            return false;

        Content[label] = newEntry;
        return true;
    }
}