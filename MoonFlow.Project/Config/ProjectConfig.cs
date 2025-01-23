using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;

using static Nindot.RomfsPathUtility;

namespace MoonFlow.Project;

public class ProjectConfig : ProjectFileFormatBase<ProjectConfigBucketCommon>
{
    public ProjectConfig(string path) : base("PROJ", path) { }
    public ProjectConfig(byte[] data) : base("PROJ", data) { }
    public ProjectConfig(string path, ProjectInitInfo initInfo) : base("PROJ")
    {
        Path = path;

        // Copy data from init info to config
        Data.Version = initInfo.Version;
        Data.DefaultLanguage = initInfo.DefaultLanguage;
    }

    protected override bool TryGetWriteData(out object data)
    {
        data = Data;
        return true;
    }

    #region Access Utility

    // ~~~~~~~~~~~~~ Common Data ~~~~~~~~~~~~~ //

    public RomfsVersion GetRomfsVersion() { return Data.Version; }
    public string GetDefaultLanguage() { return Data.DefaultLanguage; }
    public string GetSignature() { return Data.Signature; }

    public bool IsFirstBoot() { return Data.Flags.FirstBoot; }
    public bool IsDebug() { return Data.Flags.DebugProject; }
    public bool IsAlwaysUpgrade() { return Data.Flags.AlwaysUpgrade; }

    // ~~~~~~~~~~~~~ Event Graph ~~~~~~~~~~~~~ //

    public List<string> GetEventGraphPinned() { return Data.EventGraph.NodePins; }
    public bool IsEventGraphNodePinned(string n) { return Data.EventGraph.NodePins.Contains(n); }

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

    public void AddEventGraphPin(string pin)
    {
        if (Data.EventGraph.NodePins.Contains(pin))
            return;

        Data.EventGraph.NodePins.Add(pin);
    }
    public void RemoveEventGraphPin(string pin) { Data.EventGraph.NodePins.Remove(pin); }

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

    #endregion
}