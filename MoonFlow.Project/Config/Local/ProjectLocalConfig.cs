using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Godot;

namespace MoonFlow.Project;

public class ProjectLocalConfig : ProjectFileFormatBase<ProjectLocalConfigBucketMain>
{
    #region Init

    public ProjectLocalConfig(string parentPath, ProjectConfig parent) : base("LCAL", CalculateLocalConfigPath(parentPath, parent, true)) {}
    protected override bool TryGetWriteData(out dynamic data)
    {
        data = Data;
        return true;
    }

    #endregion

    #region Access Utility

    public List<string> GetEventGraphPinned() { return Data.EventGraph.NodePins; }
    public bool IsEventGraphNodePinned(string n) { return Data.EventGraph.NodePins.Contains(n); }

    #endregion

    #region Write Utility

    public void AddEventGraphPin(string pin)
    {
        if (Data.EventGraph.NodePins.Contains(pin))
            return;

        Data.EventGraph.NodePins.Add(pin);
    }
    public void RemoveEventGraphPin(string pin) { Data.EventGraph.NodePins.Remove(pin); }

    #endregion

    #region Backend Utility

    private static string CalculateLocalConfigPath(string parentPath, ProjectConfig parent, bool isEnsureDirectory = false)
    {
        const string UserDirContainer = "localconfig";

        string basePath = string.Format("{0}/{1}/", OS.GetUserDataDir(), UserDirContainer).Replace('\\', '/');
        if (isEnsureDirectory)
            Directory.CreateDirectory(basePath);

        string fileName = CalculateFileName(parentPath, parent);
        return basePath + fileName + ".mflocal";
    }
    private static string CalculateFileName(string parentPath, ProjectConfig parent)
    {
        var input = Encoding.UTF8.GetBytes(parentPath + parent.Data.Signature + "SALT_lk2OpNx");
        byte[] hashValue = MD5.HashData(input);

        return BitConverter.ToString(hashValue).Replace("-", string.Empty);
    }

    #endregion
}