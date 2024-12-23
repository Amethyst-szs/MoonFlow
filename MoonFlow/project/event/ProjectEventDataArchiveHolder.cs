using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;

using Nindot;

using MoonFlow.Scene;

namespace MoonFlow.Project;

public class ProjectEventDataArchiveHolder
{
    public string Path { get; private set; } = null;
    public Dictionary<string, SarcFile> Content { get; private set; } = [];

    public ProjectEventDataArchiveHolder(string projectPath, ProjectLoading loadScreen)
    {
        Path = projectPath + "EventData/";

        // Ensure directory exists
        Directory.CreateDirectory(Path);

        // Get file lists
        var projEvents = Directory.GetFiles(Path).ToList();
        projEvents = projEvents.Select(p => p.Split('/', '\\').Last()).ToList();

        var romPath = RomfsAccessor.ActiveDirectory + "EventData/";
        var romEvents = Directory.GetFiles(romPath).ToList();
        romEvents = romEvents.Select(p => p.Split('/', '\\').Last()).ToList();

        // Remove all entries from romEvents that are also in projEvents
        romEvents.RemoveAll(projEvents.Contains);

        // Calculate total tasks to complete for the loading screen
        float taskProgress = 0;
        float taskTotal = projEvents.Count + romEvents.Count;

        // Load files from project first
        foreach (var file in projEvents)
        {
            var sarc = SarcFile.FromFilePath(Path + file);
            sarc.UserFlags.Add("Project");

            Content.Add(file, sarc);

            UpdateLoading(loadScreen, ++taskProgress, taskTotal);
        }

        // Afterwards, load files from RomfsAccessor if they weren't already loaded by project
        foreach (var file in romEvents)
        {
            var sarc = SarcFile.FromFilePath(romPath + file);
            sarc.UserFlags.Add("Rom");

            Content.Add(file, sarc);

            // Change the target path of the sarc file to be in the project, so that if the
            // archive is saved it writes to the project and not the romfs
            sarc.FilePath = Path + file;

            UpdateLoading(loadScreen, ++taskProgress, taskTotal);
        }

        GD.Print("Loaded all event archives");
    }

    private static void UpdateLoading(ProjectLoading loadScreen, float progress, float total)
    {
        float res = progress / total * 100F;
        loadScreen.LoadingUpdateProgress("LOAD_EVENT_DATA", string.Format("{0:0}%", res));
    }
}