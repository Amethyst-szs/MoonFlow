using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nindot.Byml;

namespace Nindot.Al.StageData;

public partial class ReadOnlyStageData
{
    public readonly string StageName = null;
    public readonly string StageNameNoSuffix = null;
    private readonly List<StageScenario> ScenarioList = null;

    private ReadOnlyStageData(SarcFile sarc, string bymlName)
    {
        // Ensure sarc file contains a matching byml
        if (!sarc.Content.TryGetValue(bymlName, out ArraySegment<byte> data))
            throw new FileNotFoundException("Stage file does not contain data byml!");

        StageName = TrimSuffix(bymlName, ".byml");
        StageNameNoSuffix = TrimSuffix(TrimSuffix(TrimSuffix(StageName, "Map"), "Design"), "Sound");

        ScenarioList = BymlFileAccess.ParseBytes<List<StageScenario>>([.. data]);
    }

    public static ReadOnlyStageData FromSarcFilePath(string sarcPath)
    {
        var name = sarcPath.Split(['/', '\\']).Last();

        // Ensure this is named like an actual stage file
        if (!name.EndsWith("Map.szs") && !name.EndsWith("Design.szs") && !name.EndsWith("Sound.szs"))
            throw new Exception("File is not a stage map, stage design, or stage sound file!");

        SarcFile sarc = SarcFile.FromFilePath(sarcPath) ?? throw new NullReferenceException();
        return FromSarc(sarc);
    }

    public static ReadOnlyStageData FromSarc(SarcFile sarc)
    {
        var nameNoExt = TrimSuffix(sarc.Name, ".szs");
        var bymlName = nameNoExt + ".byml";
        return new ReadOnlyStageData(sarc, bymlName);
    }

    #region Utility

    public bool IsEmptyStageFile() => ScenarioList == null;

    public List<StageScenario> GetScenarios() => ScenarioList;
    public StageScenario GetScenario(int scenario1Through15) => ScenarioList[scenario1Through15 - 1];
    public int GetScenarioCount() => ScenarioList.Count;

    // This function exists as an extension method from Godot, but Nindot doesn't have access
    // to the Godot namespace so this is just a quick copy from:
    // https://stackoverflow.com/questions/5284591/how-to-remove-a-suffix-from-end-of-string
    private static string TrimSuffix(string s, string suffix)
    {
        if (s.EndsWith(suffix))
            return s[..^suffix.Length];

        return s;
    }
    
    #endregion
}