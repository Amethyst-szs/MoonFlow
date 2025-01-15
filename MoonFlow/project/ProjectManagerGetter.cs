using System;
using Godot;

using Nindot.LMS.Msbp;
using static Nindot.RomfsPathUtility;

using MoonFlow.Project.Database;

namespace MoonFlow.Project;

public static partial class ProjectManager
{
    public static bool IsProjectExist() { return Project != null; }
    public static bool IsProjectDebug()
    {
        if (!OS.IsDebugBuild()) return false;
        if (Project == null) return false;
        return Project.Config.IsDebug();
    }

    public static ProjectState GetProject() { return Project; }
    public static RomfsVersion GetRomfsVersion()
    {
        if (Project == null) return RomfsVersion.INVALID_VERSION;
        return Project.Config.GetRomfsVersion();
    }
    public static string GetPath() { return Project?.Path; }
    public static string GetDefaultLang()
    {
        if (Project == null) return null;
        return Project.Config.GetDefaultLanguage();
    }


    public static ProjectMsbpHolder GetMSBPHolder() { return Project?.MsgStudioProject; }
    public static SarcMsbpFile GetMSBP() { return Project?.MsgStudioProject?.Project; }

    public static ProjectMessageStudioText GetMSBT() { return Project?.MsgStudioText; }
    public static ProjectLanguageHolder GetMSBTArchives() { return Project?.MsgStudioText?.DefaultLanguage; }
    public static ProjectLanguageHolder GetMSBTArchives(string lang) { return Project?.MsgStudioText[lang]; }
    public static ProjectLanguageMetaFile GetMSBTMetaHolder()
    {
        return GetMSBTMetaHolder(Project.Config.GetDefaultLanguage());
    }
    public static ProjectLanguageMetaFile GetMSBTMetaHolder(string lang)
    {
        if (Project == null || Project.MsgStudioText == null || lang == null)
            return null;

        Project.MsgStudioText.TryGetValue(lang, out ProjectLanguageHolder langHolder);
        if (langHolder == null)
            throw new Exception("Invalid Language: " + lang);

        return langHolder.Metadata;
    }

    public static ProjectDatabaseHolder GetDB() { return Project?.Database; }
}