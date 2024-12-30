using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Linq;

using Nindot;
using Nindot.LMS.Msbt;

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
        var timeC = Data.TimeTable.ToDictionary(entry => entry.Key, entry => entry.Value);
        foreach (var item in timeC)
        {
            if (item.Value == DateTime.UnixEpoch.ToFileTimeUtc())
                timeC.Remove(item.Key);
        }

        data.EntryTable = lookupC;
        data.TimeTable = timeC;

        return true;
    }

    // ====================================================== //
    // ================== Access Utilities ================== //
    // ====================================================== //

    public ProjectLanguageFileEntryMeta GetMetadata(SarcMsbtFile file, MsbtEntry entry)
    {
        string hash = CalcHash(file.Sarc.Name, file.Name, entry.Name);

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

        if (Data.TimeTable.TryGetValue(hash, out long value))
            return DateTime.FromFileTime(value);

        var newMeta = DateTime.UnixEpoch.ToFileTimeUtc();
        Data.TimeTable.Add(hash, newMeta);

        return DateTime.FromFileTime(newMeta);
    }

    public void SetLastModifiedTime(SarcMsbtFile file)
    {
        SetLastModifiedTime(file.Sarc, file.Name);
    }
    public void SetLastModifiedTime(SarcFile file, string key)
    {
        string hash = CalcHash(file.Name, key);
        Data.TimeTable[hash] = DateTime.Now.ToFileTime();
    }

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
}