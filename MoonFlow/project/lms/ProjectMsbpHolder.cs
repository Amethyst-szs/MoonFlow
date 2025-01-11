using System;
using System.IO;
using System.Collections.Generic;
using Godot;

using Nindot;
using Nindot.LMS.Msbp;

using MoonFlow.Async;
using MoonFlow.Project.Database;
using MoonFlow.Ext;

namespace MoonFlow.Project;

public class ProjectMsbpHolder
{
    public SarcMsbpFile Project;

    private bool IsUpdateInProcess = false;

    private const string LocalFilePath = "LocalizedData/Common/ProjectData.szs";
    private const string ProjectDataFileName = "ProjectData.msbp";

    public ProjectMsbpHolder(string projectPath)
    {
        var path = projectPath + LocalFilePath;

        // If file doesn't exist in project, clone from RomfsAccessor
        if (!File.Exists(path))
        {
            if (!RomfsAccessor.TryGetRomfsDirectory(out string romfs))
                throw new RomfsAccessException("Cannot clone project data archive from romfs!");
            
            File.Copy(romfs + LocalFilePath, path);
        }

        // Read archive and ensure it contains a ProjectData.msbp
        var archive = SarcFile.FromFilePath(path);
        if (!archive.Content.ContainsKey(ProjectDataFileName))
            throw new SarcFileException("File does not contain" + ProjectDataFileName);
        
        // Get msbp from archive
        Project = archive.GetFileMSBP(ProjectDataFileName);

        GD.Print("Parsed Project MSBP");
    }

    #region Reload Routine

    public async void ReloadProjectSources()
    {
        if (IsUpdateInProcess)
            return;
        
        IsUpdateInProcess = true;
        GD.Print("Updating MSBP source database...");
        
        var run = AsyncRunner.Run(TaskRunReloadProjectSources, AsyncDisplay.Type.UpdateProjectMsbp);
        await run.Task;

        GD.Print("Completed MSBP update");
        IsUpdateInProcess = false;
    }

    private void TaskRunReloadProjectSources(AsyncDisplay display)
    {
        var arcs = ProjectManager.GetMSBTArchives();
        var db = Project.Project.Content;

        display.UpdateProgress(0, 1);

        AddAllEntriesInArc(arcs.SystemMessage, db);
        AddAllEntriesInArc(arcs.LayoutMessage, db);

        // Handle StageMessage separately
        var worldDB = ProjectManager.GetDB();
        foreach (var file in arcs.StageMessage.Content.Keys)
        {
            var world = worldDB.GetWorldInfoByStageName(file);

            // If a file doesn't have an assigned world, create an entry for every world as backup
            if (world == null)
            {
                GD.PrintRich("[i]WARNING:[/i] " + file + " does not have an assigned world");
                foreach (var backup in worldDB.WorldList)
                    PublishFile(arcs.StageMessage.Name, file, backup, db);
                
                continue;
            }
            
            PublishFile(arcs.StageMessage.Name, file, world, db);
        }

        // Sort database alphabetically
        db.Sort();

        Project.WriteArchive();
        display.UpdateProgress(1, 1);
    }

    private void AddAllEntriesInArc(SarcFile arc, List<string> db)
    {
        foreach (var file in arc.Content.Keys)
            PublishFile(arc.Name, file, db);
    }

    #endregion

    #region Utilities

    private static void PublishFile(string arc, string msbt, List<string> db)
    {
        if (arc == "StageMessage.szs")
            throw new Exception("Any request for publishing a StageMessage msbt must include WorldInfo");

        arc = arc.RemoveFileExtension();
		msbt = msbt.RemoveFileExtension();
        
        var entry = string.Format("{0}/{1}.mstxt", arc, msbt);

        if (!db.Contains(entry))
            db.Add(entry);
    }

    private static void PublishFile(string arc, string msbt, WorldInfo world, List<string> db)
    {
        if (arc != "StageMessage.szs")
            throw new Exception("Do not pass WorldInfo if archive is not StageMessage");
        
        PublishFile("StageMessage/" + world.WorldName, msbt, db);
    }

    #endregion
}