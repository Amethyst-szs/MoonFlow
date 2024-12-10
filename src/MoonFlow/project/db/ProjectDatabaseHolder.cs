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
    #region Properties & Init

    private ProjectState Parent = null;

    public readonly List<WorldInfo> WorldList = null;

    private SarcFile ArchiveWorldList = null;
    private SarcFile ArchiveShineInfo = null;
    private SarcFile ArchiveItemList = null;

    public ProjectDatabaseHolder(ProjectState parent, ProjectLoading loadScreen)
    {
        Parent = parent;

        // Ensure SystemData folder in project
        Directory.CreateDirectory(parent.Path + "SystemData/");

        // Retrieve WorldListFromDb.byml
        GD.Print("Loading WorldListFromDb.byml");
        loadScreen.LoadingUpdateProgress("LOAD_WORLD_LIST");
        WorldList = GetWorldListDb(WorldInfo.GetWorldListPath(parent.Path));

        // Fetch the display name of each world
        GD.Print("Loading world display names");
        loadScreen.LoadingUpdateProgress("LOAD_WORLD_LIST_DISPLAY_NAMES");
        SetupWorldDisplayNames();

        // Load shine lookup list for each world
        GD.Print("Accessing ShineInfo.szs");
        ArchiveShineInfo = WorldShineList.GetShineInfoSarc(WorldInfo.GetShineInfoPath(parent.Path));

        foreach (var world in WorldList)
        {
            loadScreen.LoadingUpdateProgress("LOAD_SHINE_INFO", world.Display);
            world.ShineList = WorldShineList.Build(ArchiveShineInfo, world.WorldName);

            GD.Print(" - " + world.WorldName + " OK");
        }

        // Load Coin Collect and Shine type for each world
        GD.Print("Accessing ItemList.szs");
        loadScreen.LoadingUpdateProgress("LOAD_ITEM_LIST");

        ArchiveItemList = WorldItemType.GetItemListSarc(WorldItemType.GetItemListPath(parent.Path));
        var itemList = WorldItemType.BuildList(ArchiveItemList);

        foreach (var itemDb in itemList)
        {
            var world = WorldList.Find(s => s.WorldName == itemDb.WorldName);
            if (world == null)
                continue;
            
            world.WorldItemType = itemDb;
        }

        WriteWorldItemList();
    }

    #endregion

    #region Database Writing

    public bool WriteWorldList()
    {
        var fileName = WorldInfo.DatabaseBymlPath;

        if (!ArchiveWorldList.Content.ContainsKey(fileName))
            return false;

        MemoryStream stream = new();
        if (!BymlFileAccess.WriteFile(stream, WorldList))
            throw new Exception("Byml writer exception");

        ArchiveWorldList.Content[fileName] = stream.ToArray();
        ArchiveWorldList.WriteArchive(WorldInfo.GetWorldListPath(Parent.Path));

        return true;
    }

    public bool WriteShineInfo(string worldName, bool isWriteToDisk = true)
    {
        var info = WorldList.Find(s => s.WorldName == worldName);
        if (info == null)
            return false;

        info.ShineList.UpdateShineInfoArchive(ArchiveShineInfo);

        if (isWriteToDisk)
            ArchiveShineInfo.WriteArchive(WorldInfo.GetShineInfoPath(Parent.Path));

        return true;
    }

    public void WriteShineInfoAllWorlds()
    {
        foreach (var world in WorldList)
            WriteShineInfo(world.WorldName, false);
        
        ArchiveShineInfo.WriteArchive(WorldInfo.GetShineInfoPath(Parent.Path));
    }

    public void WriteWorldItemList()
    {
        var list = WorldList.Select(s => s.WorldItemType);
        WorldItemType.UpdateItemTypeArchive(ArchiveItemList, list);
        ArchiveItemList.WriteArchive(WorldItemType.GetItemListPath(Parent.Path));
    }

    #endregion

    #region Utilities

    private List<WorldInfo> GetWorldListDb(string arcPath)
    {
        if (File.Exists(arcPath))
        {
            ArchiveWorldList = SarcFile.FromFilePath(arcPath);
        }
        else
        {
            if (!RomfsAccessor.TryGetRomfsDirectory(out string romDir))
                throw new Exception("RomfsAccessor could not return directory");

            ArchiveWorldList = SarcFile.FromFilePath(WorldInfo.GetWorldListPath(romDir));
        }

        if (!ArchiveWorldList.Content.TryGetValue(WorldInfo.DatabaseBymlPath, out ArraySegment<byte> data))
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

    #endregion
}