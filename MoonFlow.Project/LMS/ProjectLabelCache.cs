using Godot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Nindot;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib.Smo;
using FuzzySharp;
using System.Diagnostics;

namespace MoonFlow.Project.Cache;

public class ProjectLabelCache(ProjectLanguageHolder archives)
{
    private readonly Dictionary<ArchiveType, Dictionary<string, Dictionary<string, string>>> LabelList = [];
    private readonly List<KeyValuePair<LabelTarget, string>> UnsortedLabelList = [];
    private readonly ProjectLanguageHolder Archives = archives;

    public enum ArchiveType
    {
        SYSTEM,
        STAGE,
        LAYOUT,
    }

    #region Cache Accessor

    public readonly struct LabelTarget(ArchiveType arc, string file, string label)
    {
        public readonly ArchiveType Archive = arc;
        public readonly string File = file;
        public readonly string Label = label;
    }
    public readonly struct LabelLookupResult(ArchiveType arc, string file, string label, string preview, int fuzz)
    {
        public readonly ArchiveType Archive = arc;
        public readonly string File = file;
        public readonly string Label = label;
        public readonly string PreviewText = preview;
        public readonly int FuzzValue = fuzz;
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

        var terms = label.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var file in LabelList[arc])
        {
            // Filter matches to only include items in the search label string
            var matches = file.Value.ToList().FindAll(l =>
            {
                return terms.All(s => l.Key.Contains(s)) || terms.Any(s => Fuzz.Ratio(s, file.Key) > 90);
            });

            // Sort matches by FuzzySort
            matches.Sort((a, b) =>
            {
                return Fuzz.Ratio(label, b.Key) - Fuzz.Ratio(label, a.Key);
            });

            if (matches.Count == 0)
                continue;

            var result = matches.Select(s => new LabelLookupResult(arc, file.Key, s.Key, s.Value, Fuzz.Ratio(label, s.Key)));
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

            var result = matches.Select(s => new LabelLookupResult(arc, file.Key, s.Key, s.Value, Fuzz.Ratio(label, s.Key)));
            list.AddRange(result);
        }

        return list;
    }
    public List<LabelLookupResult> CreateSortedListByFuzz(string query)
    {
        List<KeyValuePair<LabelTarget, string>> rawList = [.. UnsortedLabelList];
        List<LabelLookupResult> result = [];

        // Convert raw list to LabelLookupResult list with fuzz ratio information
        foreach (var item in rawList)
        {
            int ratio = Fuzz.PartialRatio(query, item.Value);
            result.Add(new LabelLookupResult(item.Key.Archive, item.Key.File, item.Key.Label, item.Value, ratio));
        }

        // Sort list by the fuzz ratio
        result.Sort((a, b) =>
        {
            return b.FuzzValue - a.FuzzValue;
        });

        return result;
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
        UnsortedLabelList.Clear();

        // Update each archive's cache
        UpdateArchiveCache(ArchiveType.SYSTEM);
        UpdateArchiveCache(ArchiveType.STAGE);
        UpdateArchiveCache(ArchiveType.LAYOUT);
    }

    private void UpdateArchiveCache(ArchiveType arcType)
    {
        // Fetch SarcFile
        SarcFile arc = arcType switch
        {
            ArchiveType.SYSTEM => Archives.SystemMessage,
            ArchiveType.STAGE => Archives.StageMessage,
            ArchiveType.LAYOUT => Archives.LayoutMessage,
            _ => throw new UnreachableException(),
        };

        var result = new Dictionary<string, Dictionary<string, string>>();
        var meta = Archives.Metadata;

        var nameList = arc.Content.Keys.ToList();
        nameList.Sort();

        foreach (var name in nameList)
        {
            // Ensure last modified time in metadata
            meta.GetLastModifiedTime(arc, name);

            MsbtFile file;
            try
            {
                file = MsbtFile.FromBytes([.. arc.Content[name]], name, new MsbtElementFactoryProjectSmo());
            }
            catch (MsbtEntryParserException)
            {
                throw;
            }
            catch
            {
                GD.PushWarning("Failed to read cache for ", name);
                continue;
            }

            var labels = file.GetEntryLabels().ToList();
            labels.Sort(string.Compare);

            var text = labels.Select(l => file.GetEntry(l).GetRawText(true)).ToList();

            // Create and store output in result
            var msbtOutput = labels.Zip(text, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
            result[name] = msbtOutput;

            // Dump data into unsorted label list
            foreach (var item in msbtOutput)
            {
                var target = new LabelTarget(arcType, name, item.Key);
                UnsortedLabelList.Add(new KeyValuePair<LabelTarget, string>(target, item.Value));
            }
        }

        LabelList[arcType] = result;
        return;
    }

    #endregion
}