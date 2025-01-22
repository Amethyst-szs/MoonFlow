using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Module.RiiStudioSzs;

using static Nindot.Tests.PathUtility;

namespace Nindot.Tests;

public class SarcYaz0
{
    [Fact]
    public static void DecodeYaz0()
    {
        var data = File.ReadAllBytes(ResDirectory + "Example.szs");
        Szs.Decode(data);
    }

    [Fact]
    public static void EncodeYaz0()
    {
        var source = "Hello world, I am example text for the yaz0 compression algo!";
        Szs.Encode(Encoding.UTF8.GetBytes(source));
    }

    [Fact]
    public static void ParseSarc()
    {
        SarcFile file = SarcFile.FromFilePath(ResDirectory + "Example.szs");

        // Ensure sarc content
        Assert.Single(file.Content);
        Assert.Equal("example.txt", file.Content.Keys.ElementAt(0));

        // Ensure txt validity
        var data = Encoding.UTF8.GetString(file.Content.Values.ElementAt(0));
        Assert.Equal("Hello World!", data);
    }
}