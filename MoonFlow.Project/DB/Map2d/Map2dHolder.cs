using System;
using System.Collections.Generic;
using System.Linq;

using Godot;
using Godot.Extension.Resources;

using Nindot;

using MoonFlow.Project.Database;
using Godot.Extension;

namespace MoonFlow.Project;

public class Map2dHolder
{
    private readonly SarcFile Archive = null;

    private readonly Map2d Default;
    private readonly Dictionary<int, Map2d> ScenarioOverrides = [];

    public Map2dHolder(WorldInfo world, SarcFile sarc, BfresResource bfres)
    {
        Archive = sarc;

        // Iterate through all elements that match current world predicate
        foreach (var fileName in sarc.Content.Keys.Where(k => k.StartsWith(world.Name)))
        {
            if (!fileName.EndsWith(".byml"))
                continue;

            // Fetch texture using byml name
            var map = new Map2d(fileName, sarc, bfres);

            // Place generated map into default or scenario override DB
            var suffix = fileName.TrimSuffix(".byml").TrimPrefix(world.Name);

            if (suffix == string.Empty)
                Default = map;
            else
                ScenarioOverrides[int.Parse(suffix)] = map;

            continue;
        }
    }

    public Map2d GetMap() => Default;
    public Map2d GetMap(int scenario)
    {
        if (ScenarioOverrides.TryGetValue(scenario, out Map2d map))
            return map;

        return Default;
    }

    public void WriteMatrixArchive(string path)
    {
        Default.WriteMatrixDataToDb();
        foreach (var scenario in ScenarioOverrides.Values)
            scenario.WriteMatrixDataToDb();

        Archive.WriteArchive(path);
    }

    public void MakeScenarioUnique(int scenario)
    {
        if (ScenarioOverrides.ContainsKey(scenario))
            throw new Exception("Cannot make a scenario unique that is already unique!");

        // Duplicate default map
        var duplicate = Default.DuplicateMap();

        // Reformat file name
        var newName = duplicate.FileName.TrimSuf(".byml") + scenario.ToString() + ".byml";
        duplicate.FileName = newName;

        ScenarioOverrides.Add(scenario, duplicate);
    }
    public void MakeScenarioNotUnique(int scenario)
    {
        if (!ScenarioOverrides.TryGetValue(scenario, out Map2d map))
            throw new Exception("Cannot make scenario not unique that wasn't unique in the first place!");

        // Remove entry from map's archive and scenario override table
        Archive.Content.Remove(map.FileName);
        ScenarioOverrides.Remove(scenario);
    }

    public const string ArchivePathSuffix = "ObjectData/Texture2dMap.szs";
    public static string GetArchivePath(string root) { return root + ArchivePathSuffix; } 
}