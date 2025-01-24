using Godot;

namespace MoonFlow.Project.FTP;

public partial class ProjectFtpCredentialStore : ConfigFile
{
    public string Host
    {
        get { return GetValue(SectionServer, "host", "").AsString(); }
        set { SetValue(SectionServer, "host", value); }
    }
    public int Port
    {
        get { return GetValue(SectionServer, "port", 5000).AsInt32(); }
        set { SetValue(SectionServer, "port", value); }
    }
    public string TargetDirectory
    {
        get { return GetValue(SectionServer, "remote_target", TargetPresetAtmosphere).AsString(); }
        set { SetValue(SectionServer, "remote_target", value); }
    }

    public string User
    {
        get { return GetValue(SectionCredentials, "user", "").AsString(); }
        set { SetValue(SectionCredentials, "user", value); }
    }
    public string Pass
    {
        get { return GetValue(SectionCredentials, "pass", "").AsString(); }
        set { SetValue(SectionCredentials, "pass", value); }
    }

    public const string SectionServer = "server";
    public const string SectionCredentials = "cred";
    public const string StorePath = "user://ftp.cfg";

    public ProjectFtpCredentialStore() => Load(StorePath);
    public void Save() => Save(StorePath);

    #region Target Presets

    public const string TargetPresetAtmosphere = "/atmosphere/contents/0100000000010000/romfs/";
    public const string TargetPresetLunaKit = "/LunaKit/";
    public const string TargetPresetQuickMoon = "/switch/qm/";

    public void SetTarget(string path) { TargetDirectory = path; }
    public void SetTargetAtmosphere() { TargetDirectory = TargetPresetAtmosphere; }
    public void SetTargetLunaKit() { TargetDirectory = TargetPresetLunaKit; }
    public void SetTargetQuickMoon() { TargetDirectory = TargetPresetQuickMoon; }

    #endregion
}