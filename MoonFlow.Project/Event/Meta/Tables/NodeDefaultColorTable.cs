using System;
using System.Collections.Generic;
using Godot;

namespace MoonFlow.Project;

public static class MetaDefaultColorLookupTable
{
    private static readonly Dictionary<MetaCategoryTable.Categories, Color> Table = new()
    {
        { MetaCategoryTable.Categories.ENTRY_POINT, Color.FromHtml("#26ffa8") },

        { MetaCategoryTable.Categories.FLOW, Colors.LightSteelBlue },
        { MetaCategoryTable.Categories.EVENT, Colors.Lavender },
        { MetaCategoryTable.Categories.ACTOR, Colors.Lime },
        { MetaCategoryTable.Categories.PLAYER, Colors.Tomato },
        { MetaCategoryTable.Categories.DIALOUGE, Colors.LightSkyBlue },
        { MetaCategoryTable.Categories.ITEM, Colors.Gold },
        { MetaCategoryTable.Categories.CAMERA, Colors.Orchid },
        { MetaCategoryTable.Categories.STAGE, Colors.PaleTurquoise },
        { MetaCategoryTable.Categories.QUERY, Colors.Turquoise },
        { MetaCategoryTable.Categories.DEMO, Colors.SandyBrown },
        { MetaCategoryTable.Categories.WIPE, Colors.CornflowerBlue },
        { MetaCategoryTable.Categories.FLAGS, Colors.SteelBlue },
        { MetaCategoryTable.Categories.AMIIBO, Colors.CadetBlue },
        { MetaCategoryTable.Categories.AUDIO, Colors.LightCoral },
    };

    public static Color Lookup(MetaCategoryTable.Categories category)
    {
        if (Table.TryGetValue(category, out Color value))
            return value;

        return Colors.White;
    }
}