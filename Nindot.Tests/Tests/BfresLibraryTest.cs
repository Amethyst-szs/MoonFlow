using System.IO;
using BfresLibrary;
using BfresLibrary.Switch;
using BfresLibrary.Switch.Core;

using static Nindot.Tests.PathUtility;

namespace Nindot.Tests;

public class BfresLibraryTests
{
    [Fact]
    public static void TestTextureBntx()
    {
        var sarc = SarcFile.FromFilePath(ResDirectory + "TextureTest.szs");
        Assert.Contains("TextureTest.bfres", sarc.Content);

        var stream = new MemoryStream([.. sarc.Content["TextureTest.bfres"]]);
        var file = new ResFile(stream);
        Assert.NotNull(file);
        Assert.True(file.Textures.ContainsKey("ForestWorldHomeStage"));

        var tex = file.Textures["ForestWorldHomeStage"] as SwitchTexture;
        Assert.Equal("BC1_SRGB", tex.Format.ToString());
        Assert.Equal(2048, (int)tex.Height);
        Assert.Equal(2048, (int)tex.Width);
    }
}