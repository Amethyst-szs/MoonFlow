using System;
using System.IO;
using System.Linq;
using Godot;

using Nindot;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace MoonFlow.Project;

public partial class ProjectLanguageHolder
{
    public string LocalPath { get; private set; } = null;
    public string Path { get; private set; } = null;

    public SarcFile SystemMessage = null;
    public SarcFile StageMessage = null;
    public SarcFile LayoutMessage = null;

    public ProjectIconResolver ProjectIconResolver { get; private set; } = null;

    public ProjectLanguageMetaFile Metadata { get; private set; } = null;

    public ProjectLanguageHolder(string projectPath, string lang)
    {
        // Create path to this set of sarc files
        LocalPath = "LocalizedData/" + lang + "/MessageData/";
        Path = projectPath + LocalPath;

        // Ensure directory exists
        Directory.CreateDirectory(Path);

        // Init metadata
        Metadata = new(Path + ".mfmeta");

        // Init all archives from path
        InitArchive(ref SystemMessage, Path + "SystemMessage.szs");
        InitArchive(ref StageMessage, Path + "StageMessage.szs");
        InitArchive(ref LayoutMessage, Path + "LayoutMessage.szs");

        InitProjectIconResolver();

        GD.Print(string.Format(" - {0} OK", lang));
    }

    private void InitArchive(ref SarcFile file, string filePath)
    {
        // If a file does not exist at this directory, copy from romfs accessor
        if (!File.Exists(filePath))
        {
            if (!RomfsAccessor.TryGetRomfsDirectory(out string romfs))
                throw new RomfsAccessException("Cannot clone msbt archive from romfs!");

            var romfsFilePath = romfs + LocalPath + filePath.Split(['/', '\\']).Last();
            if (!File.Exists(romfsFilePath))
                throw new RomfsAccessException("Romfs does not contain " + romfsFilePath);

            File.Copy(romfsFilePath, filePath);
        }

        // Read archive from path
        file = SarcFile.FromFilePath(filePath);
    }

    private void InitProjectIconResolver()
    {
        const string style = "PadStyle.msbt";
        const string pair = "PadPair.msbt";

        if (!SystemMessage.Content.TryGetValue(style, out ArraySegment<byte> byteStyle))
            throw new SarcFileException("SystemMessage missing key " + style);

        if (!SystemMessage.Content.TryGetValue(pair, out ArraySegment<byte> bytePair))
            throw new SarcFileException("SystemMessage missing key " + pair);

        ProjectIconResolver = ProjectIconResolver.FromPadStyleAndPadPair([.. byteStyle], [.. bytePair]);
    }

    #region Utility

    public void WriteArchives()
    {
        SystemMessage.WriteArchive();
        StageMessage.WriteArchive();
        LayoutMessage.WriteArchive();
    }


    public SarcFile GetArchiveByFileName(string name, bool throwOnInvalid = true)
    {
        return name switch
        {
            "SystemMessage.szs" => SystemMessage,
            "StageMessage.szs" => StageMessage,
            "LayoutMessage.szs" => LayoutMessage,

            "SystemMessage" => SystemMessage,
            "StageMessage" => StageMessage,
            "LayoutMessage" => LayoutMessage,

            _ => throwOnInvalid ? throw new Exception("Unknown file name: " + name) : null,
        };
    }

    public MsbtFile GetMsbtInRomfsAccessor(SarcMsbtFile source)
    {
        if (source.Sarc != SystemMessage && source.Sarc != StageMessage && source.Sarc != LayoutMessage)
            throw new Exception("Invalid projectSarc!");

        // Access sarc from romfs accessor
        if (!RomfsAccessor.TryGetRomfsDirectory(out string romDir))
            throw new Exception("RomfsAccessor is not ready!");

        var path = romDir + LocalPath + source.Sarc.Name;
        if (!File.Exists(path))
            throw new FileNotFoundException("Could not find " + path);

        var sarc = SarcFile.FromFilePath(path);

        // Attempt to get corresponding msbt file
        if (!sarc.Content.ContainsKey(source.Name))
            return null;

        var romMsbt = sarc.GetFileMSBT(source.Name, new MsbtElementFactoryProjectSmo());
        return romMsbt;
    }

    public bool IsMetadataOnDisk()
    {
        return File.Exists(Path + ".mfmeta");
    }

    #endregion
}