using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Nindot.Al.SMO;

public static class RomfsValidation
{
    public static bool ValidateAndUpdatePath(ref string path, out RomfsVersion version)
    {
        // Setup default version value
        version = RomfsVersion.INVALID_VERSION;

        // Attempt to update the path to ensure validity
        path = ModifyPath(path);
        if (path == null) return false;

        // Get the game version from the romfs path using the hash table
        version = GetVersion(path);
        if (version == RomfsVersion.INVALID_VERSION) return false;

        return true;
    }

    private static string ModifyPath(string path)
    {
        // Replace all backslashes with forward slashes
        path = path.Replace('\\', '/');

        // Make sure the path ends with a slash
        if (!path.EndsWith('/') && !path.EndsWith('\\')) path += '/';
        
        // Check if the current path is valid
        if (!Directory.Exists(path)) return null;

        // If this directory contains a directory called romfs, enter that
        var dirList = Directory.GetDirectories(path);
        if (dirList.Any(s => s.EndsWith("romfs")))
        {
            path += "romfs/";
            dirList = Directory.GetDirectories(path);
        }

        // Ensure this directory contains the standard folders an SMO romfs should have
        foreach (var dir in RequiredRomfsDirectories)
        {
            if (!dirList.Contains(path + dir)) return null;
        }

        return path;
    }

    private static RomfsVersion GetVersion(string path)
    {
        GetHash(path, out string hash);
        if (hash == null || hash == string.Empty)
            return RomfsVersion.INVALID_VERSION;

        if (HashTable.TryGetValue(hash, out RomfsVersion version))
            return version;

        return RomfsVersion.INVALID_VERSION;
    }

    private static void GetHash(string path, out string hash)
    {
        hash = null;

        var filePath = path + "LocalizedData/EUen/MessageData/SystemMessage.szs";
        if (!File.Exists(filePath))
            return;

        FileStream filestream;
        SHA256 mySHA256 = SHA256.Create();

        filestream = new FileStream(filePath, FileMode.Open)
        {
            Position = 0
        };

        byte[] hashValue = mySHA256.ComputeHash(filestream);
        hash = BitConverter.ToString(hashValue).Replace("-", string.Empty).ToLower();

        filestream.Close();
    }

    public enum RomfsVersion
    {
        v100,
        v101,
        v110,
        v120,
        v130,
        INVALID_VERSION = -1,
    };

    private static readonly Dictionary<string, RomfsVersion> HashTable = new() {
        { "12f2c4840e7f942e8bcf6a81f4e90bfc58f327e4cfbe01d48225e61688f6a64d", RomfsVersion.v100 },
        { "4635afd1b5de643db68fc95f3422b2bd9e9efdb6fb1cc9f20a04c72b24cd8644", RomfsVersion.v101 },
        { "3d0e59ba4b727b7651f8aa9ed72b431a2bede917406533c69d3b6de268f8d198", RomfsVersion.v110 },
        { "437fc064b6916b405fc81db6985e09b2341fd0ce4f6663197ea3d7bac591b85a", RomfsVersion.v120 },
        { "9d1dc8610b5571ea32dad35c4e27a0895dc523c79cb170ffde838c898ed96d58", RomfsVersion.v130 },
    };

    private static readonly string[] RequiredRomfsDirectories = [
        "EffectData",
        "EventData",
        "LayoutData",
        "LocalizedData",
        "MovieData",
        "ObjectData",
        "ShaderData",
        "SoundData",
        "StageData",
        "SystemData",
    ];
}