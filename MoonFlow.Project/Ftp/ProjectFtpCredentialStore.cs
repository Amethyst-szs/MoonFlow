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
}