using Godot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Nindot;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace MoonFlow.Project.Cache;

public class ProjectLabelCache(ProjectLanguageHolder archives)
{
    private readonly Dictionary<ArchiveType, Dictionary<string, Dictionary<string, string>>> LabelList = [];
    private readonly ProjectLanguageHolder Archives = archives;

    public enum ArchiveType
    {
        SYSTEM,
        STAGE,
        LAYOUT,
    }

    #region Cache Accessor

    public struct LabelLookupResult(ArchiveType arc, string file, string label, string preview)
    {
        public ArchiveType Archive = arc;
        public string File = file;
        public string Label = label;
        public string PreviewText = preview;
    }

    public ReadOnlyCollection<string> GetLabelsInArchive(ArchiveType arc)
    {
        var list = new List<string>();

        foreach (var file in LabelList[arc].Values)
            list.AddRange(file.Keys);

        return new ReadOnlyCollection<string>(list);
    }

    public ReadOnlyCollection<string> GetLabelsInFile(ArchiveType arc, string file)
    {
        var files = LabelList[arc];
        files.TryGetValue(file, out Dictionary<string, string> value);

        return new ReadOnlyCollection<string>([.. value.Keys]);
    }

    public List<LabelLookupResult> LookupLabelAllArc(string label)
    {
        var list = new List<LabelLookupResult>();
        list.AddRange(LookupLabel(ArchiveType.SYSTEM, label));
        list.AddRange(LookupLabel(ArchiveType.STAGE, label));
        list.AddRange(LookupLabel(ArchiveType.LAYOUT, label));
        return list;
    }

    public List<LabelLookupResult> LookupLabel(ArchiveType arc, string label)
    {
        var list = new List<LabelLookupResult>();

        foreach (var file in LabelList[arc])
        {
            var matches = file.Value.ToList().FindAll(l =>
                l.Key.Contains(label, System.StringComparison.OrdinalIgnoreCase)
            );

            if (matches.Count == 0)
                continue;

            var result = matches.Select(s => new LabelLookupResult(arc, file.Key, s.Key, s.Value));
            list.AddRange(result);
        }

        return list;
    }

    public List<LabelLookupResult> LookupLabelInFile(ArchiveType arc, string fileName, string label)
    {
        var list = new List<LabelLookupResult>();

        foreach (var file in LabelList[arc])
        {
            if (!file.Key.Contains(fileName, StringComparison.OrdinalIgnoreCase))
                continue;

            var matches = file.Value.ToList().FindAll(l =>
                l.Key.Contains(label, StringComparison.OrdinalIgnoreCase)
            );

            if (matches.Count == 0)
                continue;

            var result = matches.Select(s => new LabelLookupResult(arc, file.Key, s.Key, s.Value));
            list.AddRange(result);
        }

        return list;
    }

    public List<LabelLookupResult> LookupLabelInFileExact(ArchiveType arc, string fileName, string label)
    {
        var list = new List<LabelLookupResult>();

        foreach (var file in LabelList[arc])
        {
            if (file.Key != fileName)
                continue;

            var matches = file.Value.ToList().FindAll(l =>
                l.Key.Contains(label, StringComparison.OrdinalIgnoreCase)
            );

            if (matches.Count == 0)
                continue;

            var result = matches.Select(s => new LabelLookupResult(arc, file.Key, s.Key, s.Value));
            list.AddRange(result);
        }

        return list;
    }

    public static string GetArchiveNameFromEnum(LabelLookupResult e)
    {
        return e.Archive switch
        {
            ArchiveType.SYSTEM => "SystemMessage.szs",
            ArchiveType.STAGE => "StageMessage.szs",
            ArchiveType.LAYOUT => "LayoutMessage.szs",
            _ => throw new Exception("Invalid ArchiveType enum value"),
        };
    }

    #endregion

    #region Cache Updater

    public void UpdateCache()
    {
        // Update each archive's cache
        LabelList[ArchiveType.SYSTEM] = UpdateArchiveCache(Archives.SystemMessage);
        LabelList[ArchiveType.STAGE] = UpdateArchiveCache(Archives.StageMessage);
        LabelList[ArchiveType.LAYOUT] = UpdateArchiveCache(Archives.LayoutMessage);
    }

    private Dictionary<string, Dictionary<string, string>> UpdateArchiveCache(SarcFile arc)
    {
        var dict = new Dictionary<string, Dictionary<string, string>>();
        var meta = Archives.Metadata;

        foreach (var data in arc.Content)
        {
            // Ensure last modified time in metadata
            meta.GetLastModifiedTime(arc, data.Key);

            MsbtFile file;
            try
            {
                file = MsbtFile.FromBytes([.. data.Value], data.Key, new MsbtElementFactoryProjectSmo());
            }
            catch (MsbtEntryParserException)
            {
                throw;
            }
            catch
            {
                GD.PushWarning("Failed to read cache for ", data.Key);
                continue;
            }

            var labels = file.GetEntryLabels().ToList();
            labels.Sort(string.Compare);

            var text = labels.Select(l => file.GetEntry(l).GetRawText(true)).ToList();

            var result = labels.Zip(text, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);

            dict[data.Key] = result;
        }

        return dict;
    }

    #endregion
}