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
        { typeof(MsbtTagElementSystemRuby), "system/ruby.tscn" },
        { typeof(MsbtTagElementSystemFont), "system/font.tscn" },
        { typeof(MsbtTagElementSystemFontSize), "system/font_size.tscn" },
        { typeof(MsbtTagElementSystemColor), "system/color.tscn" },

        { typeof(MsbtTagElementEuiWait), "eui/wait.tscn" },
        { typeof(MsbtTagElementEuiSpeed), "eui/speed.tscn" },

        { typeof(MsbtTagElementNumberWithFigure), "number/number_figure.tscn" },
        { typeof(MsbtTagElementNumberTime), "number/number_time.tscn" },

        { typeof(MsbtTagElementTextAnim), "anim/text.tscn" },

        { typeof(MsbtTagElementVoice), "se/voice.tscn" },

        { typeof(MsbtTagElementString), "string/string.tscn" },

        { typeof(MsbtTagElementProjectTag), "icon/project.tscn" },

        { typeof(MsbtTagElementTimeComponent), "time/component.tscn" },

        { typeof(MsbtTagElementPictureFont), "icon/picture.tscn" },
        { typeof(MsbtTagElementDeviceFont), "icon/device.tscn" },

        { typeof(MsbtTagElementTextAlign), "align/text.tscn" },

        { typeof(MsbtTagElementGrammar), "grammar/caping.tscn" },
    };

    private static readonly string Default = "default/fallback.tscn";
    private const string LocalPath = "res://scene/editor/msbt/tag_edit/";

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