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
            var byml = sarc.GetFileBYML(fileName);

            if (byml.TryGetValue(out List<object> proj, "ProjMatrix"))
                map.ProjMatrix = ImportMatrix4x4Data(proj.Cast<float>());

            if (byml.TryGetValue(out List<object> view, "ViewMatrix"))
                map.ViewMatrix = ImportMatrix4x3Data(view.Cast<float>());

            if (byml.TryGetValue(out List<object> viewproj, "ViewProjMatrix"))
                map.ViewProjMatrix = ImportMatrix4x4Data(viewproj.Cast<float>());

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

    #region Utility

    private static Matrix4x4 ImportMatrix4x4Data(IEnumerable<float> data)
    {
        if (data.Count() < (4 * 4))
            throw new Exception("Not enough data to build matrix!");

        var matrix = new Matrix4x4();
        int offset = 0;

        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                matrix[row, col] = data.ElementAt(offset);
                offset++;
            }
        }

        return matrix;
    }

    private static Matrix4x3 ImportMatrix4x3Data(IEnumerable<float> data)
    {
        if (data.Count() < (3 * 4))
            throw new Exception("Not enough data to build matrix!");

        var matrix = new Matrix4x3();
        int offset = 0;

        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                matrix[row, col] = data.ElementAt(offset);
                offset++;
            }
        }

        return matrix;
    }


    #endregion
}