using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

using Nindot;
using Nindot.Al.StageData;
using Nindot.Byml;

namespace MoonFlow.Project.Database;

public class CheckpointFlagDbFile : List<CheckpointFlagInfo>
{
    private readonly string HomeStage;
    private readonly int Scenario;

    public CheckpointFlagDbFile(WorldInfo world, ReadOnlyStageData data, int scenario1Through15)
    {
        // Store init properties for later
        HomeStage = world.Name;
        Scenario = scenario1Through15;

        Init(data);
    }

    [Obsolete("This cobnstructor has very bad performance due to needing to read from disk for every scenario.Please provide ReadOnlyStageData in constructor for better performance.")]
    public CheckpointFlagDbFile(ProjectDatabaseHolder db, WorldInfo world, int scenario1Through15)
    {
        // Store init properties for later
        HomeStage = world.Name;
        Scenario = scenario1Through15;

        // Fetch stage data
        string path = GetHomeStageSarcPath(world, db.Path);
        ReadOnlyStageData data = ReadOnlyStageData.FromSarcFilePath(path);
        Init(data);
    }

    private void Init(ReadOnlyStageData data)
    {
        if (data.IsEmptyStageFile())
            return;

        if (Scenario > data.GetScenarioCount() || Scenario < 1)
            throw new Exception("Invalid scenario number: " + Scenario);

        StageScenario scenario = data.GetScenario(Scenario);
        if (!scenario.TryGetValue("CheckPointList", out List<StageObject> list))
            return;

        // Add an entry for every checkpoint in the list
        foreach (var checkpoint in list)
        {   
            // Ensure that the object in the list is an actual checkpoint flag object
            string unitName = checkpoint.GetUnitConfigName();
            string paramName = checkpoint.GetParameterConfigName();

            if (unitName != "CheckpointFlag" || paramName != "CheckpointFlag")
                continue;

            var info = new CheckpointFlagInfo(checkpoint.GetId(), checkpoint.GetPosition());
            Add(info);
        }
    }

    public void WriteIntoSarc(SarcLibrary.Sarc target)
    {
        Dictionary<string, CheckpointFlagDbFile> bymlSource = [];
        bymlSource.Add("FlagList", this);

        // If the list is completely empty, replace the source with a null because that's
        // what the original game does
        if (Count == 0)
            bymlSource["FlagList"] = null;

        MemoryStream stream = new();
        BymlFileAccess.WriteFile(stream, bymlSource);

        string bymlName = "FlagList_" + HomeStage + "_" + Scenario.ToString() + ".byml";
        target.Add(bymlName, stream.ToArray());
    }

    #region Utility

    internal static string GetHomeStageSarcPath(WorldInfo world, string projectPath)
    {
        // Create stage file lookup name
        string stageFile = world.Name + "Map.szs";

        string stagePath = projectPath + "StageData/" + stageFile;

        // Attempt to access archive
        if (File.Exists(stagePath))
        {
            return stagePath;
        }
        else
        {
            if (!RomfsAccessor.TryGetRomfsDirectory(out string romDir))
                throw new Exception("RomfsAccessor could not return directory");

            stagePath = romDir + "StageData/" + stageFile;
            if (File.Exists(stagePath))
                return stagePath;
        }

        throw new FileNotFoundException("Could not find world HomeStage at path " + stagePath);
    }

    #endregion
}