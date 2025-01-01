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

        // Load coin collect total count for each world
        GD.Print("Accessing CollectCoinNum.byml");
        loadScreen.LoadingUpdateProgress("LOAD_COIN_COLLECT_NUM");

        var coinCollectCountInfo = GetCoinCollectCountInfo();
        foreach (var info in coinCollectCountInfo)
        {
            var world = WorldList.Find(s => s.WorldName == info.WorldName);
            if (world == null)
                continue;

            world.CoinCollectInfo = info;
        }
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

    private List<CollectCoinCountInfo> GetCoinCollectCountInfo()
    {
        byte[] bytes = [.. ArchiveWorldList.Content[CollectCoinCountInfo.BymlPath]];
        return BymlFileAccess.ParseBytes<List<CollectCoinCountInfo>>(bytes);
    }

    #endregion

    #region Database Writing

    public void WriteWorldList()
    {
        WriteBymlToWorldListArchive(WorldInfo.DatabaseBymlPath, WorldList);

        var coinCollectInfo = WorldList.Select((s) => s.CoinCollectInfo);
        WriteBymlToWorldListArchive(CollectCoinCountInfo.BymlPath, coinCollectInfo);

        ArchiveWorldList.WriteArchive(WorldInfo.GetWorldListPath(Parent.Path));
    }

    private void WriteBymlToWorldListArchive<T>(string name, T data)
    {
        if (!ArchiveWorldList.Content.ContainsKey(name))
            throw new Exception(name + "does not exist in WorldList.szs");

        MemoryStream stream = new();
        if (!BymlFileAccess.WriteFile(stream, data))
            throw new Exception("Byml writer exception");

        ArchiveWorldList.Content[name] = stream.ToArray();
    }

    public void WriteShineInfo(string worldName, bool isWriteToDisk = true)
    {
        var info = WorldList.Find(s => s.WorldName == worldName);
        if (info == null)
            throw new Exception("Could not find ShineInfo for world " + worldName);

        info.ShineList.UpdateShineInfoArchive(ArchiveShineInfo);

        if (isWriteToDisk)
            ArchiveShineInfo.WriteArchive(WorldInfo.GetShineInfoPath(Parent.Path));
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

    public WorldInfo GetWorldInfoByStageName(string name)
    {
        // Remove file extension
        int dotIdx = name.Find('.');
        if (dotIdx != -1)
            name = name[..dotIdx];
        
        // Lookup stage name
        foreach (var world in WorldList)
        {
            if (world.StageList.Any((s) => s.name == name))
                return world;
        }

        return null;
    }

    public WorldInfo GetWorldByName(string name)
    {
        return WorldList.Find((w) => w.WorldName == name);;
    }

    public ShineInfo GetShineByUID(int uid)
    {
        foreach (var world in WorldList)
        {
            var shine = world.ShineList.Find(s => s.UniqueId == uid);
            if (shine != null)
                return shine;
        }

        return null;
    }

    public int GetShineCountWithUID(int uid)
    {
        int count = 0;
        foreach (var world in WorldList)
            count += world.ShineList.Count(s => s.UniqueId == uid);

        return count;
    }

    private List<WorldInfo> GetWorldListDb(string arcPath)
    {
        // Attempt to access archive
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

        // Get byml from archive and convert to list
        if (!ArchiveWorldList.Content.TryGetValue(WorldInfo.DatabaseBymlPath, out ArraySegment<byte> data))
            throw new SarcFileException("Missing " + WorldInfo.DatabaseBymlPath);

        var list = BymlFileAccess.ParseBytes<List<WorldInfo>>([.. data]);

        // Sort stages in world by their type
        foreach (var world in list)
            SortWorldStagesByType(world.StageList);

        return list;
    }

    public static void SortWorldStagesByType(List<StageInfo> list)
    {
        list.Sort((a, b) => a.CompareTo(b));
    }

    #endregion
}