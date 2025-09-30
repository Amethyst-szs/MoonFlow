using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;

using static Nindot.RomfsPathUtility;

namespace MoonFlow.Project;

public class ProjectConfig : ProjectFileFormatBase<ProjectConfigBucketCommon>
{
    public ProjectLocalConfig LocalConfig { get; private set; } = null;

    #region Init

    public ProjectConfig(string path) : base("PROJ", path) { InitLocalConfig(); }
    public ProjectConfig(byte[] data) : base("PROJ", data) { InitLocalConfig(); }
    public ProjectConfig(string path, ProjectInitInfo initInfo) : base("PROJ")
    {
        // Copy data from init info to config
        Path = path;
        Data.Version = initInfo.Version;
        Data.DefaultLanguage = initInfo.DefaultLanguage;

        InitLocalConfig();
    }
    private void InitLocalConfig()
    {
        LocalConfig = new(Path, this);
    }

    #endregion

    #region Access Utility

    // ~~~~~~~~~~~~~ Common Data ~~~~~~~~~~~~~ //

    public RomfsVersion GetRomfsVersion() { return Data.Version; }
    public string GetDefaultLanguage() { return Data.DefaultLanguage; }
    public string GetSignature() { return Data.Signature; }

    public bool IsFirstBoot() { return Data.Flags.FirstBoot; }
    public bool IsDebug() { return Data.Flags.DebugProject; }
    public bool IsAlwaysUpgrade() { return Data.Flags.AlwaysUpgrade; }

    // ~~~~~~~~~~~~~~~~ Target ~~~~~~~~~~~~~~~ //

    public bool IsEngineTargetOk(string hash)
    {
        return hash == Data.Target.CommitHash;
    }
    public void GetEngineTarget(out string name, out string hash, out DateTime time)
    {
        name = Data.Target.Name;
        hash = Data.Target.CommitHash;
        time = DateTime.FromFileTimeUtc(Data.Target.UnixTime);
    }
    public void GetEngineTarget(out string name, out string hash, out long time)
    {
        name = Data.Target.Name;
        hash = Data.Target.CommitHash;
        time = Data.Target.UnixTime;
    }

    #endregion

    #region Write Utility

    public void ClearFirstBootFlag() { Data.Flags.FirstBoot = false; }
    public void SetAlwaysAcceptUpgradeFlag() { Data.Flags.AlwaysUpgrade = true; }
    public void SetDebugState(bool isDebug) { Data.Flags.DebugProject = isDebug; }
    public void EnsureSignature() { _ = Data.Signature; } // The signature's get method generates a sig if not present

    public void SetEngineTarget(string name, string hash, long time)
    {
        SetEngineTarget(name, hash, DateTime.FromFileTimeUtc(time));
    }
    public void SetEngineTarget(string name, string hash, DateTime time)
    {
        Data.Target.Name = name;
        Data.Target.CommitHash = hash;
        Data.Target.UnixTime = time.ToFileTimeUtc();
    }

    protected override bool TryGetWriteData(out object data)
    {
        data = Data;
        return true;
    }

    #endregion
}