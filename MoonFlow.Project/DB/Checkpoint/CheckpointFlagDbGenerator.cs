using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

using Nindot;
using Nindot.Al.StageData;
using Nindot.Byml;

namespace MoonFlow.Project.Database;

public static class CheckpointFlagDbGenerator
{
    public static void Generate(ProjectDatabaseHolder db)
    {
        // Create sarc to store all generated checkpoint files
        SarcLibrary.Sarc sarc = [];

        // Iterate through all worlds and scenarios
        foreach (var world in db.WorldList)
        {
            var stage = GetOrCacheStageData(db, world.Name);

            for (int scenario = 1; scenario <= world.ScenarioNum; scenario++)
            {
                var file = new CheckpointFlagDbFile(db, world, stage, scenario);
                file.WriteIntoSarc(sarc);
            }

            // Once all scenarios are done we can clear the kingdom's cache to save memory
            ClearStageDataCache();
        }

        // Write to project's SystemData directory
        string path = GetCheckpointDbPath(db.Path);

        MemoryStream stream = new();
        sarc.Write(stream);

        var result = NindotYaz0.Compress(stream);
        File.WriteAllBytes(path, [.. result]);
    }

    #region Cache

    private static readonly Dictionary<string, ReadOnlyStageData> StageDataCache = [];
    internal static ReadOnlyStageData GetOrCacheStageData(ProjectDatabaseHolder db, string stageName)
    {
        if (StageDataCache.TryGetValue(stageName, out ReadOnlyStageData cache))
            return cache;

        Console.WriteLine("Caching StageData for " + stageName);

        string path = CheckpointFlagDbFile.GetStageSarcPath(stageName, db.Path);
        ReadOnlyStageData stage = ReadOnlyStageData.FromSarcFilePath(path);

        StageDataCache.Add(stageName, stage);

        return stage;
    }
    internal static void ClearStageDataCache()
    {
        StageDataCache.Clear();
    }

    #endregion

    #region Utility

    private static string GetCheckpointDbPath(string root) { return root + "SystemData/CheckpointFlagInfo.szs"; }
    
    #endregion
}