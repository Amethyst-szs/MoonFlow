using System;
using System.Numerics;
using System.Threading.Tasks;
using Godot;

using MoonFlow.Project;
using MoonFlow.Project.Database;

namespace MoonFlow.Scene.EditorWorld;

public static class Map2dRenderUtility
{
    public async static Task<ImageTexture> GetMapImageTexture(WorldInfo world, int scenario)
    {
        var map = await ProjectManager.GetDB().TryCreateOrGetMap2d(world, scenario);
        if (map == null)
            throw new NullReferenceException("Could not get map!");

        return ImageTexture.CreateFromImage(map.Texture);
    }

    public static void PositionMapIcon(TabMap ctx, Map2d map, TextureRect icon, System.Numerics.Vector3 worldPos)
    {
        var mapSize = new System.Numerics.Vector2(ctx.Size.X, ctx.Size.Y);
        var m = map.CalcMapTrans(worldPos, mapSize);

        var iconSize = new Godot.Vector2(mapSize.X, mapSize.Y) / 25.0f;

        icon.PivotOffset = iconSize / 2.0f;
        icon.Position = new Godot.Vector2(m.X, m.Y) - (iconSize / 2.0f);
        icon.Size = iconSize;
    }
}