using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MoonFlow.Project;

public class ProjectMessageStudioText : Dictionary<string, ProjectLanguageHolder>
{
    public string Path { get; private set; } = null;
    public ProjectLanguageHolder DefaultLanguage { get; private set; } = null;

    public ProjectMessageStudioText(string projectPath, string defaultLang)
    {
        // Create path to LocalizedData folder in project
        Path = projectPath + "LocalizedData/";
        Directory.CreateDirectory(Path);

        // Also create path to LocalizedData in the Romfs
        if (!RomfsAccessor.TryGetRomfsDirectory(out string romfsPath))
            throw new RomfsAccessException("RomfsAccessor does not have path");

        romfsPath += "LocalizedData/";

        // For each directory (excluding Common) in the romfs localized data, create a new ProjectMsbtArchives
        var langPaths = Directory.GetDirectories(romfsPath);
        var langs = langPaths.Select(s => s.Split(['/', '\\']).Last()).ToList();
        langs.Remove("Common");

        foreach (var lang in langs)
            Add(lang, new ProjectLanguageHolder(projectPath, lang));

        // Assign DefaultLanguage reference to item with key defaultLang
        if (!ContainsKey(defaultLang))
        {
            if (!ContainsKey("USen"))
                throw new Exception("ProjectTextHolder doesn't have default language or USen!");

            DefaultLanguage = this["USen"];
            return;
        }

        DefaultLanguage = this[defaultLang];
    }
}