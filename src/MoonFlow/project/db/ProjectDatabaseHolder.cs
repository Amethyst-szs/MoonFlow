using System;
using System.Collections.Generic;
using System.IO;
using Godot;

using Nindot;
using Nindot.Byml;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;

using MoonFlow.Scene;
using System.Linq;

namespace MoonFlow.Project.Database;

public class ProjectDatabaseHolder
{
    private ProjectState Parent = null;

    public readonly List<WorldInfo> WorldList = null;

    public ProjectDatabaseHolder(ProjectState parent, ProjectLoading loadScreen)
    {
        Parent = parent;

        // Retrieve WorldListFromDb.byml
        GD.Print("Loading WorldListFromDb.byml");
        loadScreen.LoadingUpdateProgress("LOAD_WORLD_LIST");
        WorldList = GetWorldListDb(parent.Path + WorldInfo.WorldListPath);

        // Fetch the display name of each world
        GD.Print("Loading world display names");
        loadScreen.LoadingUpdateProgress("LOAD_WORLD_LIST_DISPLAY_NAMES");
        SetupWorldDisplayNames();

        // Load shine lookup list for each world
        GD.Print("Accessing ShineInfo.szs");
        var shineArc = GetShineInfoSarc(parent.Path + WorldInfo.ShineInfoPath);
        foreach (var world in WorldList)
        {
            loadScreen.LoadingUpdateProgress("LOAD_SHINE_INFO", world.Display);
            world.ShineList = GetShineInfoForWorld(shineArc, world.WorldName);

            GD.Print(" - " + world.WorldName + " OK");
        }

        return;
    }

    private static List<WorldInfo> GetWorldListDb(string arcPath)
    {
        SarcFile sarc = null;

        if (File.Exists(arcPath))
        {
            sarc = SarcFile.FromFilePath(arcPath);
        }
        else
        {
            if (!RomfsAccessor.TryGetRomfsDirectory(out string romDir))
                throw new Exception("RomfsAccessor could not return directory");

            sarc = SarcFile.FromFilePath(romDir + WorldInfo.WorldListPath);
        }

        if (!sarc.Content.TryGetValue(WorldInfo.DatabaseBymlPath, out ArraySegment<byte> data))
            throw new SarcFileException("Missing " + WorldInfo.DatabaseBymlPath);

        return BymlFileAccess.ParseBytes<List<WorldInfo>>([.. data]);
    }

    private void SetupWorldDisplayNames()
    {
        var sarc = Parent.GetMsbtArchives().SystemMessage;

        // Use base factory for faster speed
        var msbt = sarc.GetFileMSBT("StageName.msbt", new MsbtElementFactory());

        foreach (var world in WorldList)
        {
            var key = "WorldName_" + world.WorldName;
            var entry = msbt.GetEntry(key);

            world.Display = entry.GetRawText();
        }
    }

    private static SarcFile GetShineInfoSarc(string arcPath)
    {
        SarcFile sarc = null;

        if (File.Exists(arcPath))
        {
            sarc = SarcFile.FromFilePath(arcPath);
        }
        else
        {
            if (!RomfsAccessor.TryGetRomfsDirectory(out string romDir))
                throw new Exception("RomfsAccessor could not return directory");

            sarc = SarcFile.FromFilePath(romDir + WorldInfo.ShineInfoPath);
        }

        return sarc;
    }

    private List<ShineInfo> GetShineInfoForWorld(SarcFile file, string world)
    {
        var filePath = "ShineList_" + world + "WorldHomeStage.byml";

        if (!file.Content.TryGetValue(filePath, out ArraySegment<byte> data))
            throw new SarcFileException("Missing " + filePath);

        var dict = BymlFileAccess.ParseBytes<Dictionary<string, List<ShineInfo>>>([.. data]);
        if (dict.Count != 1)
            throw new Exception("ShineList dictionary invalid!");
        
        return dict.Values.ElementAt(0);
    }
}