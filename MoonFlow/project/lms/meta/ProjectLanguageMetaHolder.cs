using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Linq;

using Nindot;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;

using CsYaz0;

namespace MoonFlow.Project;

public class ProjectLanguageMetaHolder(string path) : ProjectConfigFileBase(path)
{
    // ====================================================== //
    // ==================== Initilization =================== //
    // ====================================================== //

    private ProjectLanguageMetaData Data = new();

    protected override void Init(string json)
    {
        Data = JsonSerializer.Deserialize<ProjectLanguageMetaData>(json, JsonConfig);
    }
    protected override bool TryGetWriteData(out dynamic data)
    {
        data = Data.Copy();

        // Compress lookup table by removing all elements identical to default state
        var lookupC = Data.EntryTable.ToDictionary(entry => entry.Key, entry => entry.Value);
        foreach (var item in lookupC)
        {
            if (!item.Value.IsModified())
                lookupC.Remove(item.Key);
        }

        // Compress time table by removing timestamps equal to the unix epoch
        var timeC = Data.FileTable.ToDictionary(entry => entry.Key, entry => entry.Value);
        foreach (var item in timeC)
        {
            if (item.Value == DateTime.UnixEpoch.ToFileTimeUtc())
                timeC.Remove(item.Key);
        }

        data.EntryTable = lookupC;
        data.FileTable = timeC;

        return true;
    }

    #region Public Utility

    public ProjectLanguageFileEntryMeta GetMetadata(SarcMsbtFile file, MsbtEntry entry)
    {
        string hash = CalcHash(file.Sarc.Name, file.Name, entry.Name);
        return GetMetadata(hash);
    }
    public ProjectLanguageFileEntryMeta GetMetadata(string hash)
    {
        if (Data.EntryTable.TryGetValue(hash, out ProjectLanguageFileEntryMeta value))
            return value;

        var newMeta = new ProjectLanguageFileEntryMeta();
        Data.EntryTable.Add(hash, newMeta);

        return newMeta;
    }

    public DateTime GetLastModifiedTime(SarcMsbtFile file)
    {
        return GetLastModifiedTime(file.Sarc, file.Name);
    }
    public DateTime GetLastModifiedTime(SarcFile file, string key)
    {
        string hash = CalcHash(file.Name, key);

        if (Data.FileTable.TryGetValue(hash, out long value))
            return DateTime.FromFileTime(value);

        var newMeta = DateTime.UnixEpoch.ToFileTimeUtc();
        Data.FileTable.Add(hash, newMeta);

        return DateTime.FromFileTime(newMeta);
    }

    public void SetLastModifiedTime(SarcMsbtFile file)
    {
        SetLastModifiedTime(file.Sarc, file.Name);
    }
    public void SetLastModifiedTime(SarcFile file, string key)
    {
        string hash = CalcHash(file.Name, key);
        Data.FileTable[hash] = DateTime.Now.ToFileTimeUtc();
    }

    #endregion

    #region Entry Manip

    public void EntryDuplicate(SarcFile sourceArc, string sourceEntry, string newArc, string newName)
    {
        // Move FileTable contents
        var sourceHash = CalcHash(sourceArc.Name, sourceEntry);
        var targetHash = CalcHash(newArc, newName);

        if (Data.FileTable.TryGetValue(sourceHash, out _))
        {
            long value = DateTime.Now.ToFileTimeUtc();
            Data.FileTable[targetHash] = value;
        }

        // Move EntryTable contents
        var content = sourceArc.GetFileMSBT(sourceEntry, new MsbtElementFactory());

        foreach (var label in content.GetEntryLabels())
        {
            var sourceLabelHash = CalcHash(sourceArc.Name, sourceEntry, label);
            var targetLabelHash = CalcHash(newArc, newName, label);

            var entryMeta = GetMetadata(sourceLabelHash);
            Data.EntryTable[targetLabelHash] = entryMeta;
        }

        WriteFile();
    }

    public void EntryRemove(SarcFile sourceArc, string sourceEntry)
    {
        // Remove FileTable contents
        var hash = CalcHash(sourceArc.Name, sourceEntry);
        Data.FileTable.Remove(hash);

        // Remove EntryTable contents
        var content = sourceArc.GetFileMSBT(sourceEntry, new MsbtElementFactory());

        foreach (var label in content.GetEntryLabels())
        {
            var sourceLabel = CalcHash(sourceArc.Name, sourceEntry, label);
            Data.EntryTable.Remove(sourceLabel);
        }

        WriteFile();
    }

    #endregion

    #region Hash Calcluation

    public static string CalcHash(string archive, string file, string entry)
    {
        var input = Encoding.UTF8.GetBytes(archive + file + entry + "SALT_4mAn0Lq");
        byte[] hashValue = MD5.HashData(input);

        return BitConverter.ToString(hashValue).Replace("-", string.Empty);
    }
    public static string CalcHash(string archive, string file)
    {
        var input = Encoding.UTF8.GetBytes(archive + file + "SALT_6zAmnOB");
        byte[] hashValue = MD5.HashData(input);

        return BitConverter.ToString(hashValue).Replace("-", string.Empty);
    }

    #endregion
}