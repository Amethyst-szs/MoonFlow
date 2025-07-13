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
        
        // If this map doesn't have a valid texture, pull from default map
        if (map.Texture == null)
            map = (await ProjectManager.GetDB().TryCreateOrGetMap2dHolder(world)).GetMap();

        // If the map *still* doesn't have a texture, cry
        if (map.Texture == null)
            throw new NullReferenceException("Was able to get map, but no texture could be found!");

        return ImageTexture.CreateFromImage(map.Texture);
    }

    public static void RenderShineIcons(Map2d map, Godot.Vector2 mapSize, Control iconHolder, WorldShineList shineList, ShineInfo hoveredShine, Texture2D shineIcon)
    {
        // Render all shines as icon on map
        foreach (var shine in shineList)
        {
            var id = GetShineNodeId(shine);
            var icon = GetOrCreateIcon(id, shineIcon, iconHolder);

            // Write tooltip text if not already written
            if (icon.TooltipText == string.Empty)
                icon.TooltipText = shine.LookupDisplayName(ProjectManager.GetMSBTArchives()?.StageMessage)?.GetRawText();

            // Convert world position to screen position
            PositionMapIcon(map, mapSize, icon, shine.Trans);

            // Set modulation and size depending on if the shine is hovered
            if (hoveredShine == null)
            {
                icon.SetStateNothingFocused();
                continue;
            }

            if (shine == hoveredShine)
            {
                icon.SetStateFocus();
                icon.MoveToFront();
            }
            else
            {
                icon.SetStateOtherFocused();
            }
        }
    }

    public static void RenderCheckpointIcons(Map2d map, Godot.Vector2 mapSize, Control iconHolder, CheckpointFlagDbFile flags, Texture2D flagIcon)
    {
        if (flags == null)
            return;

        // Render all shines as icon on map
        foreach (var flag in flags)
        {
            var id = GetCheckpointNodeId(flag);
            var icon = GetOrCreateIcon(id, flagIcon, iconHolder);

            // Convert world position to screen position
            PositionMapIcon(map, mapSize, icon, flag.Trans);
        }
    }

    public static void RenderOriginPoint(Map2d map, Godot.Vector2 mapSize, Control iconHolder, Texture2D originIcon)
    {
        var id = GetOriginNodeId();
        var icon = GetOrCreateIcon(id, originIcon, iconHolder);

        PositionMapIcon(map, mapSize, icon, System.Numerics.Vector3.Zero);
        icon.MoveToFront();
    }

    #region Icon Creation & Fetching

    public static MapIcon GetOrCreateIcon(string id, Texture2D icon, Control iconHolder)
    {
        // Lookup node in icon holder first
        Node iconNode = iconHolder.FindChild(id, false, false);
        if (iconNode != null && iconNode is MapIcon iconNodeTex)
            return iconNodeTex;

        // Create new node if lookup failed
        var mapIcon = SceneCreator<MapIcon>.Create();
        mapIcon.Name = id;
        mapIcon.Texture = icon;

        iconHolder.AddChild(mapIcon);
        return mapIcon;
    }

    private static string GetShineNodeId(ShineInfo shine)
    {
        return string.Format("Shine_{0}_{1}", shine.UniqueId, shine.ObjId);
    }
    private static string GetCheckpointNodeId(CheckpointFlagInfo flag)
    {
        return string.Format("Flag_{0}", flag.FlagIdStr);
    }
    private static string GetOriginNodeId() => "Origin";

    #endregion

    #region Positioning

    public static void PositionMapIcon(Map2d map, Godot.Vector2 mapSize, MapIcon icon, System.Numerics.Vector3 worldPos)
    {
        var mapSizeSys = new System.Numerics.Vector2(mapSize.X, mapSize.Y);
        var m = map.CalcMapTrans(worldPos, mapSizeSys);

        var iconSize = new Godot.Vector2(mapSize.X, mapSize.Y) / 25.0f;

        icon.PivotOffset = iconSize / 2.0f;
        icon.Position = new Godot.Vector2(m.X, m.Y) - (iconSize / 2.0f);
        icon.Size = iconSize;
    }
    
    #endregion
}