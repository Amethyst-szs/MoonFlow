using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;

using Nindot;

using MoonFlow.Scene;
using MoonFlow.Scene.EditorEvent;

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
            RegisterArchive(Path, file, EventDataArchive.ArchiveSource.PROJECT);
            UpdateLoading(loadScreen, ++taskProgress, taskTotal);
        }

        // Afterwards, load files from RomfsAccessor if they weren't already loaded by project
        foreach (var file in romEvents)
        {
            RegisterArchive(romPath, file, EventDataArchive.ArchiveSource.ROMFS);
            UpdateLoading(loadScreen, ++taskProgress, taskTotal);
        }

        GD.Print("Loaded all event archives");
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

            RegisterArchive(Path, e, EventDataArchive.ArchiveSource.PROJECT);
        }

        // Remove any deleted project events
        foreach (var item in Content)
        {
            if (projEvents.Contains(item.Key) || romEvents.Contains(item.Key))
                continue;

            Content.Remove(item.Key);
        }
    }

    public bool TryDeleteArchive(EventDataArchive arc)
    {
        if (!Content.ContainsValue(arc))
            throw new Exception("Archive is not contained in holder!");
        
        // ~~~~~~~~~~~ Remove from Disk ~~~~~~~~~~ //

        // Return if the archive isn't saved to project
        if (!File.Exists(arc.FilePath))
            return false;

        // Delete all mfgraph metadata files linked to archive
        foreach (var file in arc.Content.Keys)
        {
            var path = GraphMetaHolder.GetPath(arc.Name, file);
            if (File.Exists(path))
                File.Delete(path);
        }

        // Delete archive file
        File.Delete(arc.FilePath);

        // ~~~~~~~~~~ Remove from Holder ~~~~~~~~~ //

        // If this file doesn't exist in the romfs accessor, remove and return
        var romPath = RomfsAccessor.ActiveDirectory + "EventData/";
        if (!File.Exists(romPath + arc.Name))
        {
            Content[arc.Name] = null;
            return true;
        }

        // If it does exist, register the romfs accessor version of the file
        RegisterArchive(romPath, arc.Name, EventDataArchive.ArchiveSource.ROMFS);
        return true;
    }

    #endregion

    #region Backend Util

    private void RegisterArchive(string dir, string file, EventDataArchive.ArchiveSource type)
    {
        var sarc = EventDataArchive.FromFilePath(dir + file, type);
        Content[file] = sarc;

        // Change the target path of the sarc file to be in the project, so that if the
        // archive is saved it writes to the project and not the romfs
        if (type == EventDataArchive.ArchiveSource.ROMFS)
            sarc.FilePath = Path + file;
    }

    private void GetFileLists(out List<string> projEvents, out List<string> romEvents, out string romPath)
    {
        projEvents = [.. Directory.GetFiles(Path)];
        projEvents = projEvents.Select(p => p.Split('/', '\\').Last()).ToList();

        romPath = RomfsAccessor.ActiveDirectory + "EventData/";
        romEvents = [.. Directory.GetFiles(romPath)];
        romEvents = romEvents.Select(p => p.Split('/', '\\').Last()).ToList();
    }

    private static void UpdateLoading(ProjectLoading loadScreen, float progress, float total)
    {
        float res = progress / total * 100F;
        loadScreen?.LoadingUpdateProgress("LOAD_EVENT_DATA", string.Format("{0:0}%", res));
    }

    #endregion
}