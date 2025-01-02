using System.Collections.Generic;
using System.Text.Json;

using Nindot.Al.SMO;

namespace MoonFlow.Project;

public partial class ProjectConfig : ProjectConfigFileBase
{
    public class DataContainer
    {
        public RomfsValidation.RomfsVersion Version = RomfsValidation.RomfsVersion.INVALID_VERSION;
        public string DefaultLanguage = "USen";
        public bool IsFirstBoot = true;
        public bool IsDebugProject = false;

        // EventFlow graph info
        public List<string> EventFlowGraphPins = [];
    };

    public DataContainer Data { get; private set; } = new();

    // ====================================================== //
    // ==================== Initilization =================== //
    // ====================================================== //

    public ProjectConfig(string path) : base(path) { }
    public ProjectConfig(string path, ProjectInitInfo initInfo) : base(path)
    {
        // Copy data from init info to config
        Data.Version = initInfo.Version;
        Data.DefaultLanguage = initInfo.DefaultLanguage;

        // Write config file to disk
        WriteFile();
    }

    protected override void Init(string json)
    {
        Data = JsonSerializer.Deserialize<DataContainer>(json, JsonConfig);
    }

    protected override bool TryGetWriteData(out object data)
    {
        data = Data;
        return true;
    }
}