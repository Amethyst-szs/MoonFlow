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
            Console.WriteLine("Loading " + world.Name);

            string homeStagePath = CheckpointFlagDbFile.GetHomeStageSarcPath(world, db.Path);
            ReadOnlyStageData homeStage = ReadOnlyStageData.FromSarcFilePath(homeStagePath);

            Console.WriteLine(homeStagePath + "\n^ writing " + world.ScenarioNum, " scenarios");

            for (int scenario = 1; scenario <= world.ScenarioNum; scenario++)
            {
                var file = new CheckpointFlagDbFile(world, homeStage, scenario);
                file.WriteIntoSarc(sarc);
            }
        }

        // Write to project's SystemData directory
        string path = GetCheckpointDbPath(db.Path);
        
        MemoryStream stream = new();
        sarc.Write(stream);

        var result = NindotYaz0.Compress(stream);
        File.WriteAllBytes(path, [.. result]);
    }

    private static string GetCheckpointDbPath(string root) { return root + "SystemData/CheckpointFlagInfo.szs"; } 
}