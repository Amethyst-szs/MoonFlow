using System.Collections.Generic;
using System.IO;

using static Nindot.Tests.PathUtility;

namespace Nindot.Tests;

public class BymlFile
{
    [Fact]
    public static void ParseByml()
    {
        Byml.BymlFile file = Byml.BymlFile.FromFilePath(ResDirectory + "UnitTest.byml");
        Assert.NotNull(file);

        var key = "String";
        Assert.True(file.ContainsKey(key));
        Assert.Equal(typeof(string), file[key].GetType());
        Assert.Equal("Hello, World!", file[key]);

        key = "Value";
        Assert.True(file.ContainsKey(key));
        Assert.Equal(typeof(int), file[key].GetType());
        Assert.Equal(32, file[key]);

        key = "Unsigned Int";
        Assert.True(file.ContainsKey(key));
        Assert.Equal(typeof(uint), file[key].GetType());
        Assert.Equal((uint)89, file[key]);

        key = "64-Bit";
        Assert.True(file.ContainsKey(key));
        Assert.Equal(typeof(long), file[key].GetType());
        Assert.Equal((long)-119, file[key]);

        key = "U64";
        Assert.True(file.ContainsKey(key));
        Assert.Equal(typeof(ulong), file[key].GetType());
        Assert.Equal((ulong)10007, file[key]);

        key = "Double-Percision Float";
        Assert.True(file.ContainsKey(key));
        Assert.Equal(typeof(double), file[key].GetType());
        Assert.Equal((double)1, file[key]);

        key = "Float";
        Assert.True(file.ContainsKey(key));
        Assert.Equal(typeof(float), file[key].GetType());
        Assert.Equal((float)16.25, file[key]);

        key = "Boolean";
        Assert.True(file.ContainsKey(key));
        Assert.Equal(typeof(bool), file[key].GetType());
        Assert.Equal(true, file[key]);

        key = "Dictionary Item";
        Assert.True(file.ContainsKey(key));
        Assert.Equal(typeof(Dictionary<object, object>), file[key].GetType());

        var dict = (Dictionary<object, object>)file[key];
        Assert.True(dict.ContainsKey("Hello"));
        Assert.Equal("World!", dict["Hello"]);

        key = "List";
        Assert.True(file.ContainsKey(key));
        Assert.Equal(typeof(List<object>), file[key].GetType());

        List<int> cmpItems = [1, 5, 3];
        var list = (List<object>)file[key];

        Assert.Equal(3, list.Count);
        for (int i = 0; i < list.Count; i++)
        {
            Assert.Equal(typeof(int), list[i].GetType());
            Assert.Equal(cmpItems[i], list[i]);
        }

        return;
    }

    [Fact]
    public static void WriteByml()
    {
        Byml.BymlFile file = Byml.BymlFile.FromFilePath(ResDirectory + "UnitTest.byml");
        Assert.NotNull(file);

        MemoryStream stream = new();
        Assert.True(file.WriteFile(stream));

        Directory.CreateDirectory(OutputDirectory);
        File.WriteAllBytes(OutputDirectory + "BymlWrite.byml", stream.ToArray());

        file = Byml.BymlFile.FromFilePath(OutputDirectory + "BymlWrite.byml");
        Assert.NotNull(file);
    }
}