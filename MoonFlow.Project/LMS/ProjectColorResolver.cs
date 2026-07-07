using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Extension;

using YamlDotNet.Serialization;

using Nindot;
using Nindot.Byml;
using Nindot.LMS.Msbp;

namespace MoonFlow.Project;

public class ProjectColorResolver
{
    public struct ColorGradiation
    {
        public ColorGradiation(string name, Color c)
        {
            Top = c;
            Bottom = c;
            Name = name;
        }
        public ColorGradiation(string name, Color top, Color bottom)
        {
            Top = top;
            Bottom = bottom;
            Name = name;
        }
        public ColorGradiation(string name, BlockColor.Entry c)
        {
            Top = Color.Color8(c.R, c.G, c.B, c.A);
            Bottom = Top;
            Name = name;
        }

        public Color Top;
        public Color Bottom;
        public string Name;
        [YamlIgnore]
        public readonly bool IsGradient { get { return !Top.IsEqualApprox(Bottom); } }
    }

    private readonly SarcMsbpFile Project;
    private readonly ProjectConfig Config;

    public readonly List<ColorGradiation> ColorGradiationList = [];

    private const string ExtensionFileName = "ColorTagExtension.byml";

    public ProjectColorResolver(SarcMsbpFile projArc, ProjectConfig config)
    {
        Project = projArc;
        Config = config;
        Config.SetUseExtensionTextColorEdit(true);
        ColorGradiationList.Clear();

        var projArcContent = projArc.Sarc.Content;

        bool isExistGradiationData = projArcContent.TryGetValue(ExtensionFileName, out ArraySegment<byte> gradiationRawData);
        if (!Config.IsUseExtensionTextColorEdit() || !isExistGradiationData)
        {
            for (int i = 0; i < Project.Color_GetCount(); i++)
            {
                string name = projArc.Color_GetLabel(i);
                ColorGradiationList.Add(new(name, projArc.Color_Get(i)));
            }

            return;
        }

        var gradiationDataList = BymlFileAccess.ParseBytes<List<Dictionary<string, object>>>([.. gradiationRawData]);
        for (int i = 0; i < gradiationDataList.Count; i++)
        {
            Dictionary<string, object> data = gradiationDataList[i];

            string name = GetColorNameFromDictOrMsbp(data, Project, i);
            Color top = GetColorFromDict(data, "Top");
            Color bottom = GetColorFromDict(data, "Bottom");

            ColorGradiationList.Add(new(name, top, bottom));
        }

        return;
    }

    #region Saving

    public void TryWriteColorData()
    {
        if (!Config.IsUseExtensionTextColorEdit())
        {
            Project.Sarc.Content.Remove(ExtensionFileName);
            return;
        }

        using MemoryStream stream = new();
        if (!BymlFileAccess.WriteFile(stream, ColorGradiationList, new Byml.YamlTypeConverterEx()))
            return;
        
        Project.Sarc.Content[ExtensionFileName] = stream.ToArray();
        TrySyncMsbpColorDataWithGradiationData();
    }
    public void TrySyncMsbpColorDataWithGradiationData()
    {
        if (ColorGradiationList.Count == 0)
            return;

        Project.Color_RemoveAll();

        foreach (var c in ColorGradiationList)
        {
            var entry = new BlockColor.Entry((byte)c.Top.R8, (byte)c.Top.G8, (byte)c.Top.B8, (byte)c.Top.A8);
            Project.Color_AddNew(c.Name, entry);
        }

        if (Project.Color_GetCount() != ColorGradiationList.Count)
            throw new Exception("Failed to correctly sync msbp color data with gradiation data");
    }

    #endregion

    #region Utility

    public Color GetTopColor(int idx)
    {
        if (idx >= ColorGradiationList.Count)
            return Colors.White;

        return ColorGradiationList[idx].Top;
    }
    public Color GetBottomColor(int idx)
    {
        if (idx >= ColorGradiationList.Count)
            return Colors.White;

        return ColorGradiationList[idx].Bottom;
    }

    private static string GetColorNameFromDictOrMsbp(Dictionary<string, object> dict, SarcMsbpFile project, int colorIdx)
    {
        string labelName = project.Color_GetLabel(colorIdx);
        labelName ??= "Color_" + colorIdx.ToString();

        if (dict.TryGetValue("Name", out object labelNameObj))
            labelName = labelNameObj as string;
        
        return labelName;
    }
    private static Color GetColorFromDict(Dictionary<string, object> dict, string key)
    {
        Color result = Colors.White;
        if (dict.TryGetValue(key, out object colorObj))
        {
            var colorObjDict = colorObj as Dictionary<object, object>;
            result.R = (float)colorObjDict["R"];
            result.G = (float)colorObjDict["G"];
            result.B = (float)colorObjDict["B"];
            result.A = (float)colorObjDict["A"];
        }

        return result;
    }

    #endregion
}