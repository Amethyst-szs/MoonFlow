using System;
using System.IO;
using System.Text;

using BymlLibrary;

using static Nindot.Tests.PathUtility;
using Nindot.Al.StageData;
using System.Collections.Generic;
using System.Numerics;

namespace Nindot.Tests;

public class ReadOnlyStageDataTest
{
    [Fact]
    public static void ReadTestStageFile()
    {
        ReadOnlyStageData data = ReadOnlyStageData.FromSarcFilePath(ResDirectory + "AnimalChaseExStageMap.szs");
        TestStageFileStructure(data);

        Assert.Equal(15, data.GetScenarioCount());

        // Check specific object properties in the test stage file
        StageScenario scenario = data.GetScenario(1);

        // Check object list
        scenario.TryGetValue("ObjectList", out List<StageObject> objectList);
        Assert.NotNull(objectList);
        Assert.Equal(8, objectList.Count);

        StageObject firstObj = objectList[0];
        Assert.Equal("obj550", firstObj.GetId());
        Assert.Equal("CollectAnimalWatcher", firstObj.GetUnitConfigName());
        Assert.Equal("CollectAnimalWatcher", firstObj.GetParameterConfigName());

        Assert.Equal(new Vector3(-3250.0f, 550.0f, 12050.0f), firstObj.GetPosition());
        Assert.Equal(Vector3.Zero, firstObj.GetRotate());
        Assert.Equal(Vector3.One, firstObj.GetScale());
    }

    [Fact]
    public static void TestAgainstSmo100() { TestWithPath(GetPathSmo100() + "StageData/"); }
    [Fact]
    public static void TestAgainstSmo130() { TestWithPath(GetPathSmo130() + "StageData/"); }

    // These tests are commented out because testing every single stage file in every single version
    // takes FOREVER and is not worth it lmao
    // [Fact]
    // public static void TestAgainstSmo101() { TestWithPath(GetPathSmo101() + "StageData/"); }
    // [Fact]
    // public static void TestAgainstSmo110() { TestWithPath(GetPathSmo110() + "StageData/"); }
    // [Fact]
    // public static void TestAgainstSmo120() { TestWithPath(GetPathSmo120() + "StageData/"); }

    private static void TestWithPath(string path)
    {
        Assert.True(Directory.Exists(path));

        var filePaths = Directory.GetFiles(path, "*.szs");

        foreach (var filePath in filePaths)
        {
            ReadOnlyStageData data = ReadOnlyStageData.FromSarcFilePath(filePath);
            TestStageFileStructure(data);
        }
    }

    private static void TestStageFileStructure(ReadOnlyStageData data)
    {
        if (data.IsEmptyStageFile())
            return;

        foreach (var scenario in data.GetScenarios())
            Assert.All(scenario, s => s.Key.EndsWith("List"));
    }
}