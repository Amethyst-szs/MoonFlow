using System;
using System.Collections.Generic;

using System.IO;
using Nindot;
using Nindot.Byml;

namespace MoonFlow.Project.Database;

public class WorldItemType()
{
    public string CoinCollect;
    public int Shine;

    public string WorldName { get; private set; } = null;

    public static List<WorldItemType> BuildList(SarcFile file)
    {
        var filePath = ListBymlPath;
        if (!file.Content.TryGetValue(filePath, out ArraySegment<byte> data))
            throw new SarcFileException("Missing " + filePath);

        var list = BymlFileAccess.ParseBytes<List<WorldItemType>>([.. data]);
        return list;
    }

    public static void UpdateItemTypeArchive(SarcFile file, IEnumerable<WorldItemType> list)
    {
        var filePath = ListBymlPath;
        if (!file.Content.ContainsKey(filePath))
            throw new SarcFileException("Missing " + filePath);

        MemoryStream stream = new();
        if (!BymlFileAccess.WriteFile(stream, list))
            throw new Exception("Byml writer exception");
        
        file.Content[filePath] = stream.ToArray();
    }

    #region Utilities

    public static SarcFile GetItemListSarc(string arcPath)
    {
        SarcFile sarc;

        if (File.Exists(arcPath))
        {
            sarc = SarcFile.FromFilePath(arcPath);
        }
        else
        {
            if (!RomfsAccessor.TryGetRomfsDirectory(out string romDir))
                throw new Exception("RomfsAccessor could not return directory");

            sarc = SarcFile.FromFilePath(GetItemListPath(romDir));
        }

        return sarc;
    }

    public static string GetItemListPath(string root) { return root + "SystemData/ItemList.szs"; } 
    public const string ListBymlPath = "WorldItemTypeList.byml";

    #endregion
}