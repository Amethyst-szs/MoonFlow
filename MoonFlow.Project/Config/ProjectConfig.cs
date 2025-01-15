using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;

using static Nindot.RomfsPathUtility;

namespace MoonFlow.Project;

public partial class ProjectConfig : ProjectFileFormatBase
{
    private ProjectConfigBucketCommon Data = new();

    #region Initilization

    public ProjectConfig(string path) : base(path) { }
    public ProjectConfig(string path, ProjectInitInfo initInfo)
    {
        Path = path;

        // Copy data from init info to config
        Data.Version = initInfo.Version;
        Data.DefaultLanguage = initInfo.DefaultLanguage;
    }

    protected override void Init(string json)
    {
        Data = JsonSerializer.Deserialize<ProjectConfigBucketCommon>(json, JsonConfig);
    }

    protected override bool TryGetWriteData(out object data)
    {
        data = Data;
        return true;
    }

    #endregion

    #region Access Utility

    // ~~~~~~~~~~~~~ Common Data ~~~~~~~~~~~~~ //

    public RomfsVersion GetRomfsVersion() { return Data.Version; }
    public string GetDefaultLanguage() { return Data.DefaultLanguage; }
    public bool IsFirstBoot() { return Data.IsFirstBoot; }
    public bool IsDebug() { return Data.IsDebugProject; }

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

    #endregion

    #region Write Utility

    public void ClearFirstBootFlag() { Data.IsFirstBoot = false; }
    public void SetDebugState(bool isDebug) { Data.IsDebugProject = isDebug; }

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