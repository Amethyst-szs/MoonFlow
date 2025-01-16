using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Linq;

using Nindot;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;
using Godot;

namespace MoonFlow.Project;

public class ProjectLanguageMetaFile(string path) : ProjectFileFormatBase<ProjectLanguageMetaBucketCommon>("LANG", path)
{
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
            if (item.Value.UnixTime == DateTime.UnixEpoch.ToFileTimeUtc())
                timeC.Remove(item.Key);
        }

        data.EntryTable = lookupC;
        data.FileTable = timeC;

        return true;
    }

    #region Public Utility

    public ProjectLanguageMetaBucketEntry GetMetadata(SarcMsbtFile file, MsbtEntry entry)
    {
        return GetMetadata(file, entry.Name);
    }
    public ProjectLanguageMetaBucketEntry GetMetadata(SarcMsbtFile file, string entry)
    {
        string hash = CalcHash(file.Sarc.Name, file.Name, entry);
        return GetMetadata(hash);
    }
    public ProjectLanguageMetaBucketEntry GetMetadata(string hash)
    {
        if (Data.EntryTable.TryGetValue(hash, out ProjectLanguageMetaBucketEntry value))
            return value;

        var newMeta = new ProjectLanguageMetaBucketEntry();
        Data.EntryTable.Add(hash, newMeta);

        return newMeta;
    }

    public void TryResetMetadata(SarcMsbtFile file, MsbtEntry entry) { TryResetMetadata(file, entry.Name); }
    public void TryResetMetadata(SarcMsbtFile file, string entry)
    {
        string hash = CalcHash(file.Sarc.Name, file.Name, entry);
        TryResetMetadata(hash);
    }
    public void TryResetMetadata(string hash)
    {
        var meta = GetMetadata(hash);
        meta.Mod = false;
        meta.OffSync = false;
        meta.Custom = false;
    }

    public ProjectLanguageMetaBucketMsbtFile GetMsbtFileMetadata(string hash)
    {
        if (Data.FileTable.TryGetValue(hash, out ProjectLanguageMetaBucketMsbtFile value))
            return value;
        
        var newMeta = new ProjectLanguageMetaBucketMsbtFile();
        Data.FileTable.Add(hash, newMeta);

        return newMeta;
    }

    public DateTime GetLastModifiedTime(SarcMsbtFile file)
    {
        return GetLastModifiedTime(file.Sarc, file.Name);
    }
    public DateTime GetLastModifiedTime(SarcFile file, string key)
    {
        string hash = CalcHash(file.Name, key);
        return GetLastModifiedTime(hash);
    }
    public DateTime GetLastModifiedTime(string hash)
    {
        var meta = GetMsbtFileMetadata(hash);
        return DateTime.FromFileTime(meta.UnixTime);
    }

    public bool IsLastModifiedTimeAtEpoch(SarcMsbtFile file)
    {
        return IsLastModifiedTimeAtEpoch(file.Sarc, file.Name);
    }
    public bool IsLastModifiedTimeAtEpoch(SarcFile file, string key)
    {
        var time = GetLastModifiedTime(file, key);
        return time.ToFileTimeUtc() == DateTime.UnixEpoch.ToFileTimeUtc();
    }

    public void SetLastModifiedTime(SarcMsbtFile file)
    {
        SetLastModifiedTime(file.Sarc, file.Name);
    }
    public void SetLastModifiedTime(SarcFile file, string key)
    {
        string hash = CalcHash(file.Name, key);
        var entry = GetMsbtFileMetadata(hash);

        entry.UnixTime = DateTime.Now.ToFileTimeUtc();
    }

    public void RemoveLastModifiedTime(SarcMsbtFile file)
    {
        RemoveLastModifiedTime(file.Sarc, file.Name);
    }
    public void RemoveLastModifiedTime(SarcFile file, string key)
    {
        string hash = CalcHash(file.Name, key);
        var entry = GetMsbtFileMetadata(hash);

        entry.UnixTime = DateTime.UnixEpoch.ToFileTimeUtc();
    }

    #endregion

    #region Entry Manip

    public void EntryDuplicate(SarcFile sourceArc, string sourceEntry, string newArc, string newName)
    {
        // Move FileTable contents
        var sourceHash = CalcHash(sourceArc.Name, sourceEntry);
        var targetHash = CalcHash(newArc, newName);

        if (Data.FileTable.ContainsKey(sourceHash))
        {
            var target = GetMsbtFileMetadata(targetHash);

            long time = DateTime.Now.ToFileTimeUtc();
            target.UnixTime = time;
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

        return BitConverter.ToString(hashValue).Replace("-", string.Empty).Left(16);
    }
    public static string CalcHash(string archive, string file)
    {
        var input = Encoding.UTF8.GetBytes(archive + file + "SALT_6zAmnOB");
        byte[] hashValue = MD5.HashData(input);

        return BitConverter.ToString(hashValue).Replace("-", string.Empty).Left(16);
    }

    #endregion
}