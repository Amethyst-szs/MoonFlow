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
        var bytes = File.ReadAllBytes(path);
        return FromBytes(bytes, factory);
    }

    static public MsbtFile FromBytes(byte[] data, MsbtElementFactory factory)
    {
        MsbtFile file;

        try { file = new(factory, data); }
        catch { throw new MsbtException("Failed to parse MsbtFile"); }

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
        if (!Content.ContainsKey(label))
            return null;

        return Content[label];
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
}