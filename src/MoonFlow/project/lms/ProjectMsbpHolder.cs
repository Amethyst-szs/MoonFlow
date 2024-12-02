using System.IO;
using Nindot;
using Nindot.LMS.Msbp;

namespace MoonFlow.Project;

public class ProjectMsbpHolder
{
    public string Path;
    public SarcFile Archive;
    public MsbpFile Project;

    private const string LocalFilePath = "LocalizedData/Common/ProjectData.szs";
    private const string ProjectDataFileName = "ProjectData.msbp";

    public ProjectMsbpHolder(string projectPath)
    {
        // Setup path property
        Path = projectPath + LocalFilePath;

        // If file doesn't exist in project, clone from RomfsAccessor
        if (!File.Exists(Path))
        {
            if (!RomfsAccessor.TryGetRomfsDirectory(out string romfs))
                throw new RomfsAccessException("Cannot clone project data archive from romfs!");
            
            File.Copy(romfs + LocalFilePath, Path);
        }

        // Read archive and ensure it contains a ProjectData.msbp
        Archive = SarcFile.FromFilePath(Path);
        if (!Archive.Content.ContainsKey(ProjectDataFileName))
            throw new SarcFileException("File does not contain" + ProjectDataFileName);
        
        // Get msbp from archive
        Project = Archive.GetFileMSBP(ProjectDataFileName);
        
        // To save wasted memory, clear out archive of all byte data now that project is loaded
        Archive.Content.Clear();
    }

    public bool WriteFile()
    {
        // Build stream of MSBP
        MemoryStream stream = new();
        if (!Project.WriteFile(stream))
            return false;
        
        // Write stream into archive and save to disk
        Archive.Content.Add(ProjectDataFileName, stream.ToArray());
        if (Archive.WriteArchive(Path) != null)
            return false;
        
        // To save wasted memory, clear out archive
        Archive.Content.Clear();
        
        return true;
    }
}