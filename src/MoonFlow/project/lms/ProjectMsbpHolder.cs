using System.IO;
using Nindot;
using Nindot.LMS.Msbp;

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
    }
}