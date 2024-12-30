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
    public Dictionary<string, EventDataArchive> Content { get; private set; } = [];

    public ProjectEventDataArchiveHolder(string projectPath, ProjectLoading loadScreen)
    {
        Path = projectPath + "EventData/";

        // Ensure directory exists
        Directory.CreateDirectory(Path);

        GetFileLists(out List<string> projEvents, out List<string> romEvents, out string romPath);

        // Remove all entries from romEvents that are also in projEvents
        romEvents.RemoveAll(projEvents.Contains);

        // Calculate total tasks to complete for the loading screen
        float taskProgress = 0;
        float taskTotal = projEvents.Count + romEvents.Count;

        // Load files from project first
        foreach (var file in projEvents)
        {
            var sarc = EventDataArchive.FromFilePath(Path + file, EventDataArchive.ArchiveSource.PROJECT);
            Content.Add(file, sarc);

            UpdateLoading(loadScreen, ++taskProgress, taskTotal);
        }

        // Afterwards, load files from RomfsAccessor if they weren't already loaded by project
        foreach (var file in romEvents)
        {
            var sarc = EventDataArchive.FromFilePath(romPath + file, EventDataArchive.ArchiveSource.ROMFS);
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
        loadScreen?.LoadingUpdateProgress("LOAD_EVENT_DATA", string.Format("{0:0}%", res));
    }

    #region Public Util

    public void RefreshArchiveList()
    {
        GetFileLists(out List<string> projEvents, out List<string> romEvents, out string romPath);

        // Add any missing project events
        foreach (var e in projEvents)
        {
            if (Content.ContainsKey(e))
                continue;
            
            var sarc = EventDataArchive.FromFilePath(Path + e, EventDataArchive.ArchiveSource.PROJECT);
            Content.Add(e, sarc);
        }

        // Remove any deleted project events
        foreach (var item in Content)
        {
            if (projEvents.Contains(item.Key) || romEvents.Contains(item.Key))
                continue;
            
            Content.Remove(item.Key);
        }
    }

    public void DeleteArchive(EventDataArchive arc)
    {
        if (!Content.ContainsValue(arc))
            throw new Exception("Archive is not contained in holder!");
        
        var romPath = RomfsAccessor.ActiveDirectory + "EventData/";
        if (File.Exists(romPath + arc.Name))
        {
            var sarc = EventDataArchive.FromFilePath(romPath + arc.Name, EventDataArchive.ArchiveSource.ROMFS);
            Content[arc.Name] = sarc;
            return;
        }

        Content[arc.Name] = null;
    }

    #endregion

    #region Backend Util

    private void GetFileLists(out List<string> projEvents, out List<string> romEvents, out string romPath)
    {
        projEvents = [.. Directory.GetFiles(Path)];
        projEvents = projEvents.Select(p => p.Split('/', '\\').Last()).ToList();

        romPath = RomfsAccessor.ActiveDirectory + "EventData/";
        romEvents = [.. Directory.GetFiles(romPath)];
        romEvents = romEvents.Select(p => p.Split('/', '\\').Last()).ToList();
    }

    #endregion
}