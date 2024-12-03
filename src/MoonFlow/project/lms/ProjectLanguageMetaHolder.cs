using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Linq;

using Nindot.LMS.Msbt;

using CsYaz0;

namespace MoonFlow.Project;

public class ProjectLanguageMetaHolder
{
    // ====================================================== //
    // ======== Metadata Definition and Lookup Table ======== //
    // ====================================================== //

    public class Meta()
    {
        public bool IsDisableSync = false;

        public static readonly Meta Default = new();

        public bool IsModified() { return !Equals(Default); }
        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Meta))
                return false;

            var o = (Meta)obj;
            return IsDisableSync == o.IsDisableSync;
        }
        public override int GetHashCode() { return base.GetHashCode(); }
    }

    private readonly Dictionary<string, Meta> MetadataLookup = [];
    private readonly string Path = null;

    private static readonly JsonSerializerOptions JsonConfig = new()
    {
        IncludeFields = true,
        IgnoreReadOnlyFields = true,
    };

    // ====================================================== //
    // ==================== Initilization =================== //
    // ====================================================== //

    public ProjectLanguageMetaHolder(string path)
    {
        // Copy file path into properties
        Path = path;

        // If no file exists at this path, return
        if (!File.Exists(path))
            return;

        // Convert string into metadata lookup table
        var data = File.ReadAllBytes(path);
        data = Yaz0.Decompress(data);

        var jsonStr = Encoding.UTF8.GetString(data);
        MetadataLookup = JsonSerializer.Deserialize<Dictionary<string, Meta>>(jsonStr, JsonConfig);
    }

    public void WriteMetadata()
    {
        // Compress lookup table by removing all elements identical to default state
        var lookupC = MetadataLookup.ToDictionary(entry => entry.Key, entry => entry.Value);
        foreach (var item in lookupC)
        {
            if (!item.Value.IsModified())
                lookupC.Remove(item.Key);
        }

        // Write lookup table to file
        string dataStr = JsonSerializer.Serialize(lookupC, JsonConfig);
        byte[] data = Encoding.UTF8.GetBytes(dataStr);

        var dataCompressed = Yaz0.Compress(data);
        File.WriteAllBytes(Path, dataCompressed.ToArray());

        // Debug code, remove this later
        File.WriteAllText(Path + "_d", dataStr);
    }

    // ====================================================== //
    // ================== Access Utilities ================== //
    // ====================================================== //

    public Meta GetMetadata(SarcMsbtFile file, MsbtEntry entry)
    {
        string hash = CalcHash(file.Sarc.Name, file.Name, entry.Name);

        if (MetadataLookup.TryGetValue(hash, out Meta value))
            return value;

        var newMeta = new Meta();
        MetadataLookup.Add(hash, newMeta);

        return newMeta;
    }

    private static string CalcHash(string archive, string file, string entry)
    {
        var input = Encoding.UTF8.GetBytes(archive + file + entry + "SALT_4mAn0Lq");
        byte[] hashValue = MD5.HashData(input);

        return BitConverter.ToString(hashValue).Replace("-", string.Empty);
    }
}