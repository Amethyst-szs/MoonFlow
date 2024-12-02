using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nindot;

namespace MoonFlow.Project;

public class ProjectMsbtArchives
{
    public string Path = null;

    public SarcFile SystemMessage = null;
    public SarcFile StageMessage = null;
    public SarcFile LayoutMessage = null;

    // ====================================================== //
    // ==================== Initilization =================== //
    // ====================================================== //

    public ProjectMsbtArchives(string projectPath, string lang)
    {
        // Create path to this set of sarc files
        var localPath = "LocalizedData/" + lang + "/MessageData/";
        Path = projectPath + localPath;

        // Ensure directory exists
        Directory.CreateDirectory(Path);

        // Init all archives from path
        InitArchive(ref SystemMessage, Path + "SystemMessage.szs", localPath);
        InitArchive(ref StageMessage, Path + "StageMessage.szs", localPath);
        InitArchive(ref LayoutMessage, Path + "LayoutMessage.szs", localPath);
    }

    private static void InitArchive(ref SarcFile file, string filePath, string localPath)
    {
        // If a file does not exist at this directory, copy from romfs accessor
        if (!File.Exists(filePath))
        {
            if (!RomfsAccessor.TryGetRomfsDirectory(out string romfs))
                throw new RomfsAccessException("Cannot clone msbt archive from romfs!");
            
            var romfsFilePath = romfs + localPath + filePath.Split(['/', '\\']).Last();
            if (!File.Exists(romfsFilePath))
                throw new RomfsAccessException("Romfs does not contain " + romfsFilePath);
            
            File.Copy(romfsFilePath, filePath);
        }

        // Read archive from path
        file = SarcFile.FromFilePath(filePath);
    }

    // ====================================================== //
    // ======================= Writing ====================== //
    // ====================================================== //

    public void WriteArchives()
    {
        SystemMessage.WriteArchive(Path + "SystemMessage.szs");
        StageMessage.WriteArchive(Path + "StageMessage.szs");
        LayoutMessage.WriteArchive(Path + "LayoutMessage.szs");
    }
}