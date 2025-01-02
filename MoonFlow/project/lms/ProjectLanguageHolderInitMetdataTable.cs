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
    public void BuildMetadataTableForInit()
    {
        if (Path == null || LocalPath == null)
            return;
        
        BuildMetadataTableArchive(SystemMessage);
        BuildMetadataTableArchive(StageMessage);
        BuildMetadataTableArchive(LayoutMessage);

        Metadata.WriteFile();
    }
    private void BuildMetadataTableArchive(SarcFile file)
    {
        // Access equivelent romfs file
        if (!RomfsAccessor.TryGetRomfsDirectory(out string romfs))
            throw new RomfsAccessException("No valid romfs directory!");

        var romFilePath = romfs + LocalPath + file.Name;
        if (!File.Exists(romFilePath))
            throw new RomfsAccessException("Romfs does not contain " + romFilePath);
        
        var romFile = SarcFile.FromFilePath(romFilePath);

        // Loop through each msbt in project's file
        var factory = new MsbtElementFactoryProjectSmo();

        foreach (var source in file.Content.Keys)
        {
            SarcMsbtFile projMsbt = file.GetFileMSBT(source, factory);

            // If the rom doesn't have this msbt file, use 1 arg function
            if (!romFile.Content.ContainsKey(source))
            {
                BuildMetadataTableEntry(projMsbt);
                continue;
            }

            SarcMsbtFile romMsbt = romFile.GetFileMSBT(source, factory);
            BuildMetadataTableEntry(projMsbt, romMsbt);
        }
    }
    private void BuildMetadataTableEntry(SarcMsbtFile projFile)
    {
        Metadata.SetLastModifiedTime(projFile);

        // Mark all entries in file as modified
        foreach (var label in projFile.GetEntryLabels())
        {
            var entry = projFile.GetEntry(label);
            var meta = Metadata.GetMetadata(projFile, entry);

            meta.IsMod = true;
        }
    }
    private void BuildMetadataTableEntry(SarcMsbtFile projFile, SarcMsbtFile romFile)
    {
        bool isDif = false;

        // Loop through all labels in proj file
        foreach (var label in projFile.GetEntryLabels())
        {
            // Access project entry and metadata
            MsbtEntry projEntry = projFile.GetEntry(label);
            var meta = Metadata.GetMetadata(projFile, projEntry);
            
            // If the rom file doesn't contain this entry, mark modified
            if (!romFile.IsContainKey(label))
            {
                isDif = true;
                meta.IsMod = true;
                continue;
            }

            MsbtEntry romEntry = romFile.GetEntry(label);
            if (!projEntry.Equals(romEntry))
            {
                isDif = true;
                meta.IsMod = true;
            }
        }

        // Check if project file has any labels removed
        foreach (var label in romFile.GetEntryLabels())
            if (!projFile.IsContainKey(label))
                isDif = true;
        
        // If modified, set the last modified timestamp
        if (isDif)
            Metadata.SetLastModifiedTime(projFile);
    }
}