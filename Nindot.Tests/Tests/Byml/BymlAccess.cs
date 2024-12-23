using System.Collections.Generic;
using System.IO;

using Nindot.Byml;

namespace Nindot.UnitTest;

public class UnitTestBymlAccess : IUnitTestGroup
{
    public static void SetupGroup()
    {
    }

    [RunTest]
    public static void ParseByml()
    {
        BymlFile file = BymlFile.FromFilePath("./src/Nindot.Tests/Resources/UnitTest.byml");
        Test.Should(file != null);

        var key = "String";        
        Test.Should(file.ContainsKey(key));
        Test.Should(file[key].GetType(), typeof(string));
        Test.Should(file[key], "Hello, World!");

        key = "Value";
        Test.Should(file.ContainsKey(key));
        Test.Should(file[key].GetType(), typeof(int));
        Test.Should(file[key], 32);

        key = "Unsigned Int";
        Test.Should(file.ContainsKey(key));
        Test.Should(file[key].GetType(), typeof(uint));
        Test.Should(file[key], (uint)89);

        key = "64-Bit";
        Test.Should(file.ContainsKey(key));
        Test.Should(file[key].GetType(), typeof(long));
        Test.Should(file[key], (long)-119);

        key = "U64";
        Test.Should(file.ContainsKey(key));
        Test.Should(file[key].GetType(), typeof(ulong));
        Test.Should(file[key], (ulong)10007);

        key = "Double-Percision Float";
        Test.Should(file.ContainsKey(key));
        Test.Should(file[key].GetType(), typeof(double));
        Test.Should(file[key], (double)1);

        key = "Float";
        Test.Should(file.ContainsKey(key));
        Test.Should(file[key].GetType(), typeof(float));
        Test.Should(file[key], (float)16.25);

        key = "Boolean";
        Test.Should(file.ContainsKey(key));
        Test.Should(file[key].GetType(), typeof(bool));
        Test.Should(file[key], true);

        key = "Dictionary Item";
        Test.Should(file.ContainsKey(key));
        Test.Should(file[key].GetType(), typeof(Dictionary<object, object>));

        var dict = (Dictionary<object, object>)file[key];
        Test.Should(dict.ContainsKey("Hello"));
        Test.Should(dict["Hello"], "World!");

        key = "List";
        Test.Should(file.ContainsKey(key));
        Test.Should(file[key].GetType(), typeof(List<object>));

        List<int> cmpItems = [1, 5, 3];
        var list = (List<object>)file[key];

        Test.Should(list.Count == 3);
        for (int i = 0; i < list.Count; i++)
        {
            Test.Should(list[i].GetType(), typeof(int));
            Test.Should(list[i], cmpItems[i]);
        }

        return;
    }

    [RunTest]
    public static void WriteByml()
    {
        BymlFile file = BymlFile.FromFilePath("./src/Nindot.Tests/Resources/UnitTest.byml");
        Test.Should(file != null);

        MemoryStream stream = new();
        Test.Should(file.WriteFile(stream));

        File.WriteAllBytes(Test.TestOutputDirectory + "BymlWrite.byml", stream.ToArray());

        file = BymlFile.FromFilePath(Test.TestOutputDirectory + "BymlWrite.byml");
        Test.Should(file != null);
    }

    public static void CleanupGroup()
    {
    }
}