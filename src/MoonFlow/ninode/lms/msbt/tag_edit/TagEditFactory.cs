using System;
using System.Collections.Generic;
using Nindot.LMS.Msbt.TagLib.Smo;
using Nindot.LMS.Msbt.TagLib;

using Godot;

namespace MoonFlow.LMS.Msbt;

public static class TagEditFactory
{
    private static readonly Dictionary<Type, string> FactoryEntries = new()
    {
        { typeof(MsbtTagElementSystemFont), "system/font.tscn" },
        { typeof(MsbtTagElementSystemFontSize), "system/font_size.tscn" },

        { typeof(MsbtTagElementEuiWait), "eui/wait.tscn" },

        { typeof(MsbtTagElementVoice), "se/voice.tscn" },
    };

    private static readonly string Default = "default/fallback.tscn";
    private const string LocalPath = "res://ninode/lms/msbt/tag_edit/";

    public static TagEditScene Create(MsbtTagElement tag)
    {
        var type = tag.GetType();

        // Retrive local scene path from factory
        if (!FactoryEntries.TryGetValue(type, out string target))
            target = Default;
        
        var pack = GD.Load<PackedScene>(LocalPath + target);
        if (pack == null)
            return null;
        
        var scene = pack.Instantiate();
        if (scene.GetType() != typeof(TagEditScene) && !scene.GetType().IsSubclassOf(typeof(TagEditScene)))
            throw new Exception("TagEdit scene is not of type TagEditScene!");
        
        return scene as TagEditScene;
    }
}