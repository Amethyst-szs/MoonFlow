using Godot;
using Nindot.Al.SMO;

namespace MoonFlow.Project;

public partial class ProjectConfig : ConfigFile
{
    public RomfsValidation.RomfsVersion Version = RomfsValidation.RomfsVersion.INVALID_VERSION;
    public string DefaultLanguage = "USen";

    private string FilePath = "";
    private Error Err = Error.Ok;

    // Constructor for loading project
    public ProjectConfig(string path)
    {
        // Load config data from disk
        Err = Load(path);
        if (Err != Error.Ok)
            return;

        FilePath = path;

        // Get values from config file
        var ver = GetValue("common", "ver", (int)RomfsValidation.RomfsVersion.INVALID_VERSION).AsInt32();
        Version = (RomfsValidation.RomfsVersion)ver;

        DefaultLanguage = GetValue("common", "lang", "USen").AsString();
    }

    // Constructor for creating a new project
    public ProjectConfig(string projectFilePath, ProjectInitInfo initInfo)
    {
        // Copy data from init info to config
        FilePath = projectFilePath;
        Version = initInfo.Version;
        DefaultLanguage = initInfo.DefaultLanguage;

        // Write config file to disk
        WriteConfig();
    }

    public Error WriteConfig()
    {
        // Write values into config file
        SetValue("common", "ver", (int)Version);
        SetValue("common", "lang", DefaultLanguage);

        // Save to disk
        return Save(FilePath);
    }

    // ====================================================== //
    // ====================== Utilities ===================== //
    // ====================================================== //

    public bool IsValid()
    {
        return Err == Error.Ok;
    }
}