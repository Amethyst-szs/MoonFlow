using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using AuroraLib.Compression.Algorithms;

using Nindot;
using Nindot.Al.StageData;
using Nindot.Byml;

namespace MoonFlow.Project.Database;

public static class CheckpointFlagDbGenerator
{
    public static void Generate(ProjectDatabaseHolder db, Action<int, int> progressCallback)
    {
        // Create sarc to store all generated checkpoint files
        SarcLibrary.Sarc sarc = [];

        // Calculate total number of steps
        int curWorld = 0;
        int totalWorld = db.WorldList.Count;

        progressCallback.Invoke(curWorld, totalWorld);

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

            // Update end-user on progress
            curWorld++;
            progressCallback.Invoke(curWorld, totalWorld);
        }

        // Write to project's SystemData directory
        string path = GetCheckpointDbPath(db.Path);

        MemoryStream stream = new();
        sarc.Write(stream);

        var yaz0 = new NindotYaz0();

        var result = yaz0.Compress(stream);
        File.WriteAllBytes(path, [.. result]);
    }

    public static async Task<CheckpointFlagDbFile> CreateInfoAsync(ProjectDatabaseHolder db, WorldInfo world, int scenario1Through15)
    {
        return await Task.Run(() => CreateInfo(db, world, scenario1Through15));
    }
    public static CheckpointFlagDbFile CreateInfo(ProjectDatabaseHolder db, WorldInfo world, int scenario1Through15)
    {
        // Lookup database sarc file
        var dbPath = GetDbPathFromProjectOtherwiseRomfs(db);
        var sarc = SarcFile.FromFilePath(dbPath);

        // Lookup byml in sarc
        var bymlName = CheckpointFlagDbFile.FormatFileName(world.Name, scenario1Through15);
        if (!sarc.Content.TryGetValue(bymlName, out ArraySegment<byte> bymlData))
            return null;

        var byml = BymlFileAccess.ParseBytes<Dictionary<string, List<CheckpointFlagInfo>>>([.. bymlData]);
        if (!byml.TryGetValue("FlagList", out List<CheckpointFlagInfo> flagList))
            return null;

        // Create file from byml
        var file = new CheckpointFlagDbFile(world, scenario1Through15);
        file.AddRange(flagList);

        return file;
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

    private const string CheckpointDbPath = "SystemData/CheckpointFlagInfo.szs";
    private static string GetCheckpointDbPath(string root) { return root + CheckpointDbPath; }
    private static string GetDbPathFromProjectOtherwiseRomfs(ProjectDatabaseHolder db)
    {
        string path = GetCheckpointDbPath(db.Path);

        // Attempt to access archive
        if (File.Exists(path))
        {
            return path;
        }
        else
        {
            if (!RomfsAccessor.TryGetRomfsDirectory(out string romDir))
                throw new Exception("RomfsAccessor could not return directory");

            path = romDir + CheckpointDbPath;
            if (File.Exists(path))
                return path;
        }

        throw new FileNotFoundException("Could not find CheckpointFlagInfo.szs at path " + path);
    }
    
    #endregion
}