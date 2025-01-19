using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using Godot.Extension;

using static Nindot.RomfsPathUtility;

namespace MoonFlow.Project;

public static class RomfsAccessor
{
    public static string ActiveDirectory { get; private set; } = "";
    public static RomfsVersion ActiveVersion { get; private set; }
        = RomfsVersion.INVALID_VERSION;

    public static Dictionary<RomfsVersion, string> VersionDirectories { get; private set; } = [];

    private const string ConfigDirectory = "user://romfs.cfg";

    // ====================================================== //
    // ================== Common Utilities ================== //
    // ====================================================== //

    public static bool IsValid()
    {
        return ActiveVersion != RomfsVersion.INVALID_VERSION;
    }

    public static bool IsHaveVersion(RomfsVersion ver)
    {
        if (!VersionDirectories.TryGetValue(ver, out string path))
            return false;

        if (path == string.Empty)
            return false;

        return true;
    }

    public static bool TryGetRomfsDirectory(out string directory)
    {
        directory = null;
        if (!IsValid()) return false;

        directory = ActiveDirectory;
        return true;
    }

    public static bool IsExistFile(string path)
    {
        if (!IsValid()) return false;

        path = ActiveDirectory + path;
        return System.IO.File.Exists(path);
    }

    public static Error TryGetFile(string path, out byte[] file)
    {
        file = [];
        if (!IsValid())
            return Error.Unavailable;

        path = ActiveDirectory + path;
        if (!System.IO.File.Exists(path))
            return Error.FileNotFound;

        try
        {
            file = System.IO.File.ReadAllBytes(path);
            return Error.Ok;
        }
        catch
        {
            GD.PushError("Failed to open file at ", path);
            return Error.FileCantOpen;
        }
    }

    public static async Task<byte[]> TryGetFileAsync(string path)
    {
        if (!IsValid())
            return [];

        path = ActiveDirectory + path;
        if (!System.IO.File.Exists(path))
            return [];

        try
        {
            return await System.IO.File.ReadAllBytesAsync(path);
        }
        catch
        {
            GD.PushError("Failed to open file at ", path);
            return [];
        }
    }

    // ====================================================== //
    // =========== Initilization and Configuration ========== //
    // ====================================================== //

    [StartupTask]
    public static void InitAccessor()
    {
        var config = new ConfigFile();
        if (config.Load(ConfigDirectory) != Error.Ok) return;

        // Get list of version directory paths
        foreach (var version in (RomfsVersion[])Enum.GetValues(typeof(RomfsVersion)))
        {
            string key = Enum.GetName(version);

            Variant item = config.GetValue("path", key, "");
            var value = item.AsString();

            bool isOK = ValidateAndUpdatePath(ref value, out RomfsVersion validateVer);
            if (!isOK || version != validateVer)
                continue;

            VersionDirectories.Add(version, value);
        }

        // Get selected version directory
        var selection = config.GetValue("target", "ver", "v100").AsString();
        if (selection == null || selection == string.Empty) return;

        // Ensure that the value read from the config is defined in the num
        RomfsVersion selectionEnum;
        try
        {
            selectionEnum = Enum.Parse<RomfsVersion>(selection);
        }
        catch
        {
            return;
        }

        // Ensure this value exists in the version lookup dictionary
        if (!VersionDirectories.TryGetValue(selectionEnum, out string active)) return;

        // Assign active directory and ensure that this directory is a valid romfs endpoint
        bool isValid = ValidateAndUpdatePath(ref active, out RomfsVersion ver);
        if (!isValid) return;

        ActiveDirectory = active;
        ActiveVersion = ver;

        GD.Print("Initilized RomfsAccessor for ", Enum.GetName(ActiveVersion));
    }

    public static void TryAssignDirectory(ref string directory, out RomfsVersion version)
    {
        // Ensure new directory is valid
        bool isValid = ValidateAndUpdatePath(ref directory, out version);
        if (!isValid) return;

        // Update version dictionary
        VersionDirectories[version] = directory;

        // Update configuration file on disk
        var verName = Enum.GetName(version);

        var config = new ConfigFile();
        if (config.Load(ConfigDirectory) != Error.Ok)
        {
            // In the event of a load failure, create a default state
            config.SetValue("path", verName, directory);
            config.SetValue("target", "ver", verName);
            config.Save(ConfigDirectory);
            return;
        }

        config.SetValue("path", verName, directory);
        config.Save(ConfigDirectory);

        GD.Print(string.Format("Assigned path for {0} to {1}", Enum.GetName(ActiveVersion), directory));
    }

    public static bool TrySetGameVersion(RomfsVersion version)
    {
        if (version == RomfsVersion.INVALID_VERSION)
            return false;
        
        if (!VersionDirectories.TryGetValue(version, out string path))
            return false;

        // Ensure new directory is valid
        bool isValid = ValidateAndUpdatePath(ref path, out version);
        if (!isValid) return false;

        // Update active information
        ActiveDirectory = path;
        ActiveVersion = version;

        // Update configuration file on disk
        var config = new ConfigFile();
        if (config.Load(ConfigDirectory) != Error.Ok)
            return false;

        var verName = Enum.GetName(version);
        config.SetValue("target", "ver", verName);

        config.Save(ConfigDirectory);

        GD.Print("Set RomfsAccessor to version ", Enum.GetName(ActiveVersion));
        return true;
    }

    public static void TryUnassignDirectory(RomfsVersion version)
    {
        if (!VersionDirectories.ContainsKey(version))
            return;

        if (version == ActiveVersion)
        {
            ActiveDirectory = null;
            ActiveVersion = RomfsVersion.INVALID_VERSION;
        }

        VersionDirectories.Remove(version);
    }
}

public class RomfsAccessException(string error) : Exception(error);