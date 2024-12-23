using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Nindot;
using Nindot.Byml;

using YamlDotNet.Serialization;

namespace MoonFlow.Project.Database;

public class WorldShineList(string name, List<ShineInfo> list) : List<ShineInfo>(list)
{
    #region Read and Write

    [YamlIgnore]
    public string WorldName { get; private set; } = name;

    public static WorldShineList Build(SarcFile file, string world)
    {
        var filePath = GetBymlFileName(world);
        if (!file.Content.TryGetValue(filePath, out ArraySegment<byte> data))
            throw new SarcFileException("Missing " + filePath);

        var dict = BymlFileAccess.ParseBytes<Dictionary<string, List<ShineInfo>>>([.. data]);
        if (dict.Count != 1)
            throw new Exception("ShineList dictionary invalid!");
        
        var list = dict.Values.ElementAt(0);

        // Empty all "ScenarioName" properties to just the word "Shine"
        // This has no effect on the game, reduces file size, and gets around
        // a bug in VYaml that causes an app lockup on some strange
        // unicode symbols
        foreach (var item in list)
            item.ScenarioName = "Shine";
        
        return new WorldShineList(world, list);
    }

    public void UpdateShineInfoArchive(SarcFile file)
    {
        var filePath = GetBymlFileName(WorldName);
        if (!file.Content.ContainsKey(filePath))
            throw new SarcFileException("Missing " + filePath);
        
        var input = new Dictionary<string, List<ShineInfo>>
        {
            { "ShineList", this }
        };

        MemoryStream stream = new();
        if (!BymlFileAccess.WriteFile(stream, input))
            throw new Exception("Byml writer exception");
        
        file.Content[filePath] = stream.ToArray();
    }

    #endregion

    #region Utilities

    public static SarcFile GetShineInfoSarc(string arcPath)
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

            sarc = SarcFile.FromFilePath(WorldInfo.GetShineInfoPath(romDir));
        }

        return sarc;
    }

    public static string GetBymlFileName(string worldName)
    {
        return "ShineList_" + worldName + "WorldHomeStage.byml";
    }

    #endregion
}