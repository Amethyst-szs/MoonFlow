using FluentFTP.Helpers;
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
    public string WorkingDirectory
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

    public string DefaultLanguage
    {
        get { return GetValue(SectionLocalized, "main", "USen").AsString(); }
        set { SetValue(SectionLocalized, "main", value); }
    }
    public string TranslationLanguage
    {
        get { return GetValue(SectionLocalized, "translate", "USen").AsString(); }
        set { SetValue(SectionLocalized, "translate", value); }
    }
    public bool IsTransferAllLanguages
    {
        get { return GetValue(SectionLocalized, "is_all", false).AsBool(); }
        set { SetValue(SectionLocalized, "is_all", value); }
    }

    public const string SectionServer = "server";
    public const string SectionCredentials = "cred";
    public const string SectionLocalized = "localized";
    public const string StorePath = "user://ftp.cfg";

    public ProjectFtpCredentialStore() => Load(StorePath);
    public void Save() => Save(StorePath);

    #region Target Presets

    public const string TargetPresetAtmosphere = "/atmosphere/contents/0100000000010000/romfs/";
    public const string TargetPresetLunaKit = "/LunaKit/";
    public const string TargetPresetQuickMoon = "/switch/qm/project/";

    public void SetTarget(string path)
    {
        WorkingDirectory = path.Replace('\\', '/').EnsurePrefix("/").EnsurePostfix("/");;
    }
    public void SetTargetAtmosphere() { SetTarget(TargetPresetAtmosphere); }
    public void SetTargetLunaKit() { SetTarget(TargetPresetLunaKit); }
    public void SetTargetQuickMoon() { SetTarget(TargetPresetQuickMoon); }

    public bool IsQuickMoon() { return WorkingDirectory == TargetPresetQuickMoon; }

    #endregion
}