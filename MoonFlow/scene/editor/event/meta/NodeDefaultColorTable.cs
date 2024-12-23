using System;
using System.Collections.Generic;
using Godot;

namespace MoonFlow.Scene.EditorEvent;

public static class MetaDefaultColorLookupTable
{
    private static readonly Dictionary<MetaCategoryTable.Categories, Color> Table = new()
    {
        { MetaCategoryTable.Categories.ENTRY_POINT, Color.FromHtml("#26ffa8") },

        { MetaCategoryTable.Categories.ACTOR, Colors.Tomato },
        { MetaCategoryTable.Categories.AMIIBO, Colors.PaleGreen },
        { MetaCategoryTable.Categories.AUDIO, Colors.MediumVioletRed },
        { MetaCategoryTable.Categories.CAMERA, Colors.DarkOrchid },
        { MetaCategoryTable.Categories.COIN, Colors.Gold },
        { MetaCategoryTable.Categories.DIALOUGE, Colors.DodgerBlue },
        { MetaCategoryTable.Categories.DEMO, Colors.SandyBrown },
        { MetaCategoryTable.Categories.EVENT, Colors.Lavender },
        { MetaCategoryTable.Categories.FLAGS, Colors.Maroon },
        { MetaCategoryTable.Categories.FLOW, Colors.LightSteelBlue },
        { MetaCategoryTable.Categories.ITEM, Colors.Crimson },
        { MetaCategoryTable.Categories.PLAYER, Colors.Sienna },
        { MetaCategoryTable.Categories.QUERY, Colors.Turquoise },
        { MetaCategoryTable.Categories.SHINE, Colors.Yellow },
        { MetaCategoryTable.Categories.STAGE, Colors.Violet },
        { MetaCategoryTable.Categories.SWITCH, Colors.PaleTurquoise },
        { MetaCategoryTable.Categories.WIPE, Colors.DarkGray },
    };

    public static Color Lookup(MetaCategoryTable.Categories category)
    {
        if (Table.TryGetValue(category, out Color value))
            return value;

        return Colors.White;
    }
}