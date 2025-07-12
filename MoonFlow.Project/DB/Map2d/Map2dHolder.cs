using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Godot;
using Godot.Extension.Resources;

using Nindot;
using Nindot.Byml;

using Syroot.Maths;

using MoonFlow.Project.Database;

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

    public const string ArchivePathSuffix = "ObjectData/Texture2dMap.szs";
    public static string GetArchivePath(string root) { return root + ArchivePathSuffix; } 
}