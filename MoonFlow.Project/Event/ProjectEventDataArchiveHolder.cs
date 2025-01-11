using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;

using Nindot;
using Nindot.Al.EventFlow;
using Nindot.Al.EventFlow.Smo;

namespace MoonFlow.Project;

public class ProjectEventDataArchiveHolder
{
    public string Path { get; private set; } = null;
    public Dictionary<string, EventDataArchive> Content { get; private set; } = [];

    public ProjectEventDataArchiveHolder(string projectPath, IProjectLoadingScene loadScreen)
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
        GetFileLists(out List<string> projEvents, out List<string> romEvents, out _);

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

    public bool TryNewArchive(string target)
    {
        // Ensure target doesn't already exist on disk
        var targetPath = Path + target;
        if (File.Exists(targetPath))
            return false;

        // Create new empty sarc and write to disk
        var sarc = new SarcLibrary.Sarc();
        SarcFile.WriteArchive(sarc, targetPath);

        RefreshArchiveList();
        return true;
    }
    public static void NewGraph(EventDataArchive arc, string newName)
    {
        // Generate graph byte data
        var root = new NodeActionLoop("ActionLoop", 0);
        var graph = new Graph(newName, root);

        if (!graph.WriteBytes(out byte[] file))
            throw new EventFlowException("Failed to generate new graph");
        
        // Add data into selected archive
        arc.Content.Add(newName, file);
        arc.WriteArchive();
    }

    public bool TryDuplicateArchive(EventDataArchive source, string target, string projectPath)
    {
        // Attempt to copy archive file on disk
        var sourcePath = source.FilePath;
        if (source.Source == EventDataArchive.ArchiveSource.ROMFS)
            sourcePath = GetRomfsPathEquivalent(source.FilePath);
        
        if (!File.Exists(sourcePath))
            return false;

        var targetPath = Path + target;
        if (File.Exists(targetPath))
            return false;

        File.Copy(sourcePath, targetPath);

        // Attempt to copy each byml's mfgraph metadata file
        foreach (var file in source.Content)
        {
            var metaSource = GraphMetaHolder.GetPath(source.Name, file.Key, projectPath);
            var metaTarget = GraphMetaHolder.GetPath(target, file.Key, projectPath);

            if (File.Exists(metaSource))
                File.Copy(metaSource, metaTarget, true);
        }

        RefreshArchiveList();
        return true;
    }
    public static bool TryDuplicateGraph(EventDataArchive arc, string source, string target, string projectPath)
    {
        return TryDuplicateGraph(arc, arc, source, target, projectPath);
    }
    public static bool TryDuplicateGraph(EventDataArchive arcSource, EventDataArchive arcTarget, string source, string target, string projectPath)
    {
        // Duplicate byml in archive
        if (arcTarget.Content.ContainsKey(target) || !arcSource.Content.ContainsKey(source))
            return false;
        
        var value = arcSource.Content[source];
        arcTarget.Content[target] = value.ToArray();
        
        arcTarget.WriteArchive();

        // Duplicate mfgraph file
        var sourceHash = GraphMetaHolder.CalcNameHash(arcSource.Name, source);
        var sourceMetaPath = GraphMetaHolder.GetPath(arcSource.Name, source, projectPath);
        
        var sourceMeta = new GraphMetaHolder(sourceMetaPath);
        if (!sourceMeta.IsReadFromDisk)
        {
            // If there isn't a local metadata file, attempt to copy from embeds
            var embed = GraphMetaHolder.EmbedGraphPath + sourceHash;
            if (!Godot.FileAccess.FileExists(embed))
                return true;
            
            var data = Godot.FileAccess.GetFileAsBytes(embed);
            sourceMeta = new GraphMetaHolder(data);
        }

        var targetMetaPath = GraphMetaHolder.GetPath(arcTarget.Name, target, projectPath);

        sourceMeta.Data.ArchiveName = arcTarget.Name;
        sourceMeta.Data.FileName = target;

        sourceMeta.ChangeWritePath(targetMetaPath);
        sourceMeta.WriteFile();

        return true;
    }

    public bool TryDeleteArchive(EventDataArchive arc, string projectPath)
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
            var path = GraphMetaHolder.GetPath(arc.Name, file, projectPath);
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

    public static void DeleteGraph(EventDataArchive arc, string key, string projectPath)
    {
        if (!arc.Content.ContainsKey(key))
            throw new FileNotFoundException("Event isn't present in provided archive!");

        var hashPath = GraphMetaHolder.GetPath(arc.Name, key, projectPath);
        if (File.Exists(hashPath))
            File.Delete(hashPath);

        arc.Content.Remove(key);
        arc.WriteArchive();
    }

    #endregion

    #region Backend Util

    private void RegisterArchive(string dir, string file, EventDataArchive.ArchiveSource type)
    {
        if (!file.EndsWith(".szs"))
            return;
        
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

    private static string GetRomfsPathEquivalent(string path)
    {
        var file = path.Split(['/', '\\']).Last();
        var romPath = RomfsAccessor.ActiveDirectory + "EventData/";
        return romPath + file;
    }

    private static void UpdateLoading(IProjectLoadingScene loadScreen, float progress, float total)
    {
        float res = progress / total * 100F;
        loadScreen?.LoadingUpdateProgress("LOAD_EVENT_DATA", string.Format("{0:0}%", res));
    }

    #endregion
}