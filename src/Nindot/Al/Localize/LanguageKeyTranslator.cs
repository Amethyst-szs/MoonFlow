using System.Collections.Generic;

namespace Nindot.Al.Localize;

public static class LanguageKeyTranslator
{
    public readonly static Dictionary<string, string> Table = new()
    {
        {"CNzh", "简体中文"},
        {"EUde", "Deutsch"},
        {"EUen", "English (EU)"},
        {"EUes", "Español (España)"},
        {"EUfr", "Français (France)"},
        {"EUit", "Italiano"},
        {"EUnl", "Nederlands"},
        {"EUru", "Русский"},
        {"JPja", "日本語"},
        {"KRko", "한국어"},
        {"TWzh", "繁體中文"},
        {"USen", "English (US)"},
        {"USes", "Español (Latinoamérica)"},
        {"USfr", "Français (Canada)"},
    };
}