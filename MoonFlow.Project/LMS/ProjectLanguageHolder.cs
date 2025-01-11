using System;
using System.IO;
using System.Linq;
using Godot;

using Nindot;

namespace MoonFlow.Project;

public partial class ProjectLanguageHolder
{
    public string LocalPath { get; private set; } = null;
    public string Path { get; private set; } = null;

    public SarcFile SystemMessage = null;
    public SarcFile StageMessage = null;
    public SarcFile LayoutMessage = null;

    public ProjectIconResolver ProjectIconResolver { get; private set; } = null;

    public ProjectLanguageMetaHolder Metadata { get; private set; } = null;

    // ====================================================== //
    // ==================== Initilization =================== //
    // ====================================================== //

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

    // ====================================================== //
    // ======================= Writing ====================== //
    // ====================================================== //

    public void WriteArchives()
    {
        SystemMessage.WriteArchive();
        StageMessage.WriteArchive();
        LayoutMessage.WriteArchive();
    }

    // ====================================================== //
    // ====================== Utilities ===================== //
    // ====================================================== //

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

    public bool IsMetadataOnDisk()
    {
        return File.Exists(Path + ".mfmeta");
    }
}