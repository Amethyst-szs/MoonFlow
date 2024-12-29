using System.IO;
using Godot;

using Nindot;
using Nindot.LMS.Msbp;

using MoonFlow.Project.Database;
using System;

namespace MoonFlow.Project;

public class ProjectMsbpHolder
{
    public SarcMsbpFile Project;

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

    #region Utilities

    public void PublishFile(string arc, string msbt)
    {
        if (arc == "StageMessage.szs")
            throw new Exception("Any request for publishing a StageMessage msbt must include WorldInfo");

        if (arc.EndsWith(".szs"))
            arc = arc[..arc.Find(".szs")];
        
        if (msbt.EndsWith(".msbt"))
            msbt = msbt[..msbt.Find(".msbt")];
        
        var entry = string.Format("{0}/{1}.mstxt", arc, msbt);
        Project.Project_AddElement(entry);

        Project.WriteArchive();
    }

    public void PublishFile(string arc, string msbt, WorldInfo world)
    {
        if (arc != "StageMessage.szs")
            throw new Exception("Do not pass WorldInfo if archive is not StageMessage");
        
        PublishFile("StageMessage/" + world.WorldName, msbt);
    }

    public void UnpublishFile(string arc, string msbt)
    {
        if (arc.EndsWith(".szs"))
            arc = arc[..arc.Find(".szs")];
        
        if (msbt.EndsWith(".msbt"))
            msbt = msbt[..msbt.Find(".msbt")];

        var proj = Project.Project;
        var idx = proj.Content.FindIndex(s => s.StartsWith(arc) && s.EndsWith(msbt + ".mstxt"));
        
        if (idx == -1)
        {
            GD.PushWarning(string.Format("Could not find {0}/{1} in MSBP", arc, msbt));
            return;
        }
        
        proj.Content.RemoveAt(idx);

        Project.WriteArchive();
    }

    #endregion
}