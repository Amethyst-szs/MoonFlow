using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection.Metadata;
using Nindot;
using Nindot.Al.StageData;
using Nindot.Byml;

namespace MoonFlow.Project.Database;

public class CheckpointFlagDbFile : List<CheckpointFlagInfo>
{
    private readonly string HomeStage;
    private readonly int Scenario;

    public CheckpointFlagDbFile(ProjectDatabaseHolder db, WorldInfo world, ReadOnlyStageData data, int scenario1Through15)
    {
        // Store init properties for later
        HomeStage = world.Name;
        Scenario = scenario1Through15;

        Init(db, data);
    }
    public CheckpointFlagDbFile(WorldInfo world, int scenario1Through15)
    {
        HomeStage = world.Name;
        Scenario = scenario1Through15;
    }

    private void Init(ProjectDatabaseHolder db, ReadOnlyStageData data)
    {
        if (data.IsEmptyStageFile())
            return;

        if (Scenario > data.GetScenarioCount() || Scenario < 1)
            throw new Exception("Invalid scenario number: " + Scenario);

        StageScenario scenario = data.GetScenario(Scenario);

        // Init all checkpoints in the main homestage
        InitCheckpointsInStage(data);

        // Get list of zones used in stage
        if (!scenario.TryGetValue("ZoneList", out List<StageObject> zones) && zones == null)
            zones = [];

        foreach (var zoneObject in zones)
        {
            string zoneName = zoneObject.GetUnitConfigName();
            var zoneData = CheckpointFlagDbGenerator.GetOrCacheStageData(db, zoneName);

            InitCheckpointsInZone(zoneData, zoneObject);
        }
    }

    private void InitCheckpointsInStage(ReadOnlyStageData data)
    {
        StageScenario scenario = data.GetScenario(Scenario);

        if (!scenario.TryGetValue("CheckPointList", out List<StageObject> list))
            return;

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
    private void InitCheckpointsInZone(ReadOnlyStageData zoneData, StageObject zoneObject)
    {
        StageScenario scenario = zoneData.GetScenario(Scenario);

        if (!scenario.TryGetValue("CheckPointList", out List<StageObject> list))
            return;

        // Generate rotation quaternion for zone
        var zoneRot = zoneObject.GetRotate();
        zoneRot.X = (float)(Math.PI / 180) * zoneRot.X;
        zoneRot.Y = (float)(Math.PI / 180) * zoneRot.Y;
        zoneRot.Z = (float)(Math.PI / 180) * zoneRot.Z;

        var zoneQuat = Quaternion.CreateFromYawPitchRoll(zoneRot.Y, zoneRot.X, zoneRot.Z);

        // Init all zone checkpoints
        foreach (var checkpoint in list)
        {
            // Ensure that the object in the list is an actual checkpoint flag object
            string unitName = checkpoint.GetUnitConfigName();
            string paramName = checkpoint.GetParameterConfigName();

            if (unitName != "CheckpointFlag" || paramName != "CheckpointFlag")
                continue;

            // Construct id in the weird and obtuse format the game uses
            string id = string.Format("{0}({1}[{2}])",
                checkpoint.GetId(),
                zoneObject.GetUnitConfigName(),
                zoneObject.GetId()
            );

            // Calculate checkpoint positiom from zone's position
            var pivot = zoneObject.GetPosition();
            var pos = checkpoint.GetPosition() + pivot;

            // Get the direction vector from the pivot to the point
            Vector3 dir = pos - pivot;

            // Rotate the direction vector by the quaternion
            dir = Vector3.Transform(dir, zoneQuat);

            // Add the rotated direction vector to the pivot point
            pos = pivot + dir;

            var info = new CheckpointFlagInfo(id, pos);
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

        using MemoryStream stream = new();
        BymlFileAccess.WriteFile(stream, bymlSource);

        string bymlName = FormatFileName(HomeStage, Scenario);
        target.Add(bymlName, stream.ToArray());
    }

    #region Utility

    internal static string GetHomeStageSarcPath(WorldInfo world, string projectPath)
    {
        return GetStageSarcPath(world.Name, projectPath);
    }
    internal static string GetStageSarcPath(string name, string projectPath)
    {
        // Create stage file lookup name
        string stageFile = name + "Map.szs";

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

    internal static string FormatFileName(string homeStage, int scenario)
    {
        return "FlagList_" + homeStage + "_" + scenario.ToString() + ".byml";
    }

    #endregion
}