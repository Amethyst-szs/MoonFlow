using System;
using Godot;

namespace MoonFlow;

public static class EngineSettings
{
    public const string AutoloadName = "EngineSettingsCSharp";

    public static Variant GetSetting(string key, Variant def)
    {
        if (Engine.IsEditorHint())
            return ProjectSettings.GetSetting(key, def);

        return Engine.GetSingleton(AutoloadName).Call("get_setting", [key, def]);
    }
    public static T GetSetting<[MustBeVariant] T>(string key, Variant def)
    {
        if (Engine.IsEditorHint())
            return ProjectSettings.GetSetting(key, def).As<T>();

        var value = Engine.GetSingleton(AutoloadName).Call("get_setting", [key, def]);
        return value.As<T>();
    }

    public static void SetSetting(string key, Variant def)
    {
        Engine.GetSingleton(AutoloadName).Call("set_setting", [key, def]);
    }
    public static void RemoveSetting(string key)
    {
        Engine.GetSingleton(AutoloadName).Call("remove_setting", [key]);
    }

    public static void Save()
    {
        Engine.GetSingleton(AutoloadName).Call("save");
    }

    #region Utility

    public static void Connect(string signalName, Action method)
    {
        Engine.GetSingleton(AutoloadName).Connect(signalName, Callable.From(method));
    }

    public static string GetWiki()
    {
        if (Engine.IsEditorHint())
            return ProjectSettings.GetSetting("moonflow/wiki/local_source").AsString();

        return Engine.GetSingleton(AutoloadName).Call("get_wiki").AsString();
    }
    public static string GetWikiLocal()
    {
        if (Engine.IsEditorHint()) return "";
        return Engine.GetSingleton(AutoloadName).Call("get_wiki_local").AsString();
    }
    public static string GetWikiRemote()
    {
        if (Engine.IsEditorHint()) return "";
        return Engine.GetSingleton(AutoloadName).Call("get_wiki_remote").AsString();
    }

    #endregion
}