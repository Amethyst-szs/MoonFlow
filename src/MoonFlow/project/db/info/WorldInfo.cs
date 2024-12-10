using System;
using System.Collections.Generic;

using YamlDotNet.Serialization;

namespace MoonFlow.Project.Database;

public class WorldInfo()
{
    public string Name;
    public string WorldName;

    [YamlIgnore]
    public string Display;

    public int AfterEndingScenario;
    public int ClearMainScenario;
    public int MoonRockScenario;
    public int ScenarioNum;

    public List<StageInfo> StageList = [];
    public List<int> MainQuestInfo = [];

    [YamlIgnore]
    public WorldShineList ShineList;
    [YamlIgnore]
    public WorldItemType WorldItemType;

    public static string GetWorldListPath(string root) { return root + "SystemData/WorldList.szs"; } 
    public static string GetShineInfoPath(string root) { return root + "SystemData/ShineInfo.szs"; } 

    [YamlIgnore]
    public const string DatabaseBymlPath = "WorldListFromDb.byml";
}