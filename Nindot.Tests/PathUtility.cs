using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Nindot.Tests;

public static class PathUtility
{
    public static string GetDefaultPathSmo()
    {
        if (Instance == null) InstantiateData();

        foreach (var field in Instance.GetType().GetFields())
        {
            if (!field.Name.StartsWith("SMO"))
                continue;

            var value = field.GetValue(Instance);
            if (value is not string str || str == string.Empty)
                continue;

            return str;
        }

        throw new Exception("No valid game path was provided!");
    }

    public static string GetPathSmo100() { return GetPath("SMOv100"); }
    public static string GetPathSmo101() { return GetPath("SMOv101"); }
    public static string GetPathSmo110() { return GetPath("SMOv110"); }
    public static string GetPathSmo120() { return GetPath("SMOv120"); }
    public static string GetPathSmo130() { return GetPath("SMOv130"); }

    public const string ResDirectory = "../../../Resources/";
    public const string OutputDirectory = "../../RunOutput/";

    #region Backend

    private class Data
    {
        public string SMOv100 = "";
        public string SMOv101 = "";
        public string SMOv110 = "";
        public string SMOv120 = "";
        public string SMOv130 = "";
    }

    private static Data Instance = null;

    private static string GetPath(string key)
    {
        if (Instance == null)
            InstantiateData();

        var field = Instance.GetType()?.GetFields()?.ToList()?.Find(p => p.Name == key)
        ?? throw new Exception("Could not find requested field on data instance");

        var value = field.GetValue(Instance);
        if (value is not string str) throw new Exception();

        return str;
    }

    private static readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        IncludeFields = true,
        IgnoreReadOnlyFields = true,
    };

    private static void InstantiateData()
    {
        const string path = "../../../GamePaths.json";
        const string template = "../../../Resources/GamePathTemplate.json";

        // Attempt to access json
        if (!File.Exists(path))
        {
            File.Copy(template, path);

            const string errStr = "\n\n - MISSING GAME PATHS - \n\n go to Nindot.Tests/GamePaths.json and add your game paths!";
            Console.Write(errStr);
            throw new Exception(errStr);
        }

        var json = File.ReadAllText(path);
        Instance = JsonSerializer.Deserialize<Data>(json, jsonSerializerOptions);
    }

    #endregion
}