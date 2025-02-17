using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Godot.Extension.Resources;
using MoonFlow.Project.Database;
using Nindot;

namespace MoonFlow.Project;

public class Map2dHolder
{
    private readonly Map2d Default;
    private readonly Dictionary<int, Map2d> ScenarioOverrides = [];

    private readonly WorldInfo World;
    private readonly SarcFile Archive;
    public const string ArchivePathSuffix = "ObjectData/Texture2dMap.szs";

    public Map2dHolder(WorldInfo world, SarcFile sarc, BfresResource bfres)
    {
        World = world;
        Archive = sarc;

        // Iterate through all elements that match current world predicate
        foreach (var fileName in sarc.Content.Keys.Where(k => k.StartsWith(world.Name)))
        {
            if (!fileName.EndsWith(".byml"))
                continue;
            
            // Fetch texture using byml name
            var texture = fileName.TrimSuffix(".byml");
            var map = new Map2d(texture, bfres);

            // Determine texture type using file name suffix
            var suffix = texture.TrimPrefix(world.Name);

            if (suffix == string.Empty)
                Default = map;
            else
                ScenarioOverrides[int.Parse(suffix)] = map;
            
            // Load byml and handle matrix generation
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
}