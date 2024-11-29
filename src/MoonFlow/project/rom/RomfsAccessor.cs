using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

using Nindot.Al.SMO;

namespace MoonFlow.Project;

public static class RomfsAccessor
{
    public static string ActiveDirectory { get; private set; } = "";
    public static RomfsValidation.RomfsVersion ActiveVersion { get; private set; }
        = RomfsValidation.RomfsVersion.INVALID_VERSION;

    public static Dictionary<RomfsValidation.RomfsVersion, string> VersionDirectories { get; private set; } = [];

    private const string ConfigDirectory = "user://romfs_config.ini";

    // ====================================================== //
    // ================== Common Utilities ================== //
    // ====================================================== //

    public static bool IsValid()
    {
        return ActiveVersion != RomfsValidation.RomfsVersion.INVALID_VERSION;
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
        foreach (var version in (int[])Enum.GetValues(typeof(RomfsValidation.RomfsVersion)))
        {
            string key = Enum.GetName(typeof(RomfsValidation.RomfsVersion), version);

            Variant item = config.GetValue("path", key, "");
            var value = item.AsString();

            if (value != null && value != string.Empty)
                VersionDirectories.Add((RomfsValidation.RomfsVersion)version, value);
        }

        // Get selected version directory
        var selection = config.GetValue("target", "ver", "v100").AsString();
        if (selection == null || selection == string.Empty) return;

        // Ensure that the value read from the config is defined in the num
        RomfsValidation.RomfsVersion selectionEnum;
        try
        {
            selectionEnum = Enum.Parse<RomfsValidation.RomfsVersion>(selection);
        }
        catch
        {
            return;
        }

        // Ensure this value exists in the version lookup dictionary
        if (!VersionDirectories.TryGetValue(selectionEnum, out string active)) return;

        // Assign active directory and ensure that this directory is a valid romfs endpoint
        bool isValid = RomfsValidation.ValidateAndUpdatePath(ref active, out RomfsValidation.RomfsVersion ver);
        if (!isValid) return;

        ActiveDirectory = active;
        ActiveVersion = ver;
    }

    public static void TryAssignDirectory(ref string directory, out RomfsValidation.RomfsVersion version)
    {
        // Ensure new directory is valid
        bool isValid = RomfsValidation.ValidateAndUpdatePath(ref directory, out version);
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
    }

    public static void TrySetGameVersion(RomfsValidation.RomfsVersion version)
    {
        if (!VersionDirectories.TryGetValue(version, out string path)) return;

        // Ensure new directory is valid
        bool isValid = RomfsValidation.ValidateAndUpdatePath(ref path, out version);
        if (!isValid) return;

        // Update active information
        ActiveDirectory = path;
        ActiveVersion = version;

        // Update configuration file on disk
        var config = new ConfigFile();
        if (config.Load(ConfigDirectory) != Error.Ok)
            return;

        var verName = Enum.GetName<RomfsValidation.RomfsVersion>(version);
        config.SetValue("target", "ver", verName);

        config.Save(ConfigDirectory);
    }

    public static void TryUnassignDirectory(RomfsValidation.RomfsVersion version)
    {
        if (!VersionDirectories.ContainsKey(version))
            return;

        if (version == ActiveVersion)
        {
            ActiveDirectory = null;
            ActiveVersion = RomfsValidation.RomfsVersion.INVALID_VERSION;
        }

        VersionDirectories.Remove(version);
    }
}