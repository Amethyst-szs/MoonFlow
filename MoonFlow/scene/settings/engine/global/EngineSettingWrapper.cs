using Godot;

namespace MoonFlow;

public static class EngineSettings
{
    public const string AutoloadName = "EngineSettingsCSharp";

    public static Variant GetSetting(string key, Variant def)
    {
        return Engine.GetSingleton(AutoloadName).Call("get_setting", [key, def]);
    }
    public static T GetSetting<[MustBeVariant] T>(string key, Variant def)
    {
        var value = Engine.GetSingleton(AutoloadName).Call("get_setting", [key, def]);
        return value.As<T>();
    }

    public static void SetSetting(string key, Variant def)
    {
        Engine.GetSingleton(AutoloadName).Call("set_setting", [key, def]);
    }

    public static void Save()
    {
        Engine.GetSingleton(AutoloadName).Call("save");
    }
}