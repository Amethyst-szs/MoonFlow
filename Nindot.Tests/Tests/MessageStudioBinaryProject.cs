using System.IO;
using Nindot.LMS.Msbp;

using static Nindot.Tests.PathUtility;

namespace Nindot.Tests;

public class MessageStudioBinaryProject
{
    [Fact]
    public static void MsbpParse()
    {
        // Load in file
        MsbpFile file = MsbpFile.FromFilePath(ResDirectory + "ProjectData.msbp");
        Assert.True(file.IsValid());

        // Test color results
        Assert.True(file.Color_IsFileContainData());
        Assert.Equal(8, file.Color_GetCount());

        Assert.Equal("Black", file.Color_GetLabel(0));
        Assert.Equal(new BlockColor.Entry(0, 0, 0, 255), file.Color_Get(0));

        Assert.Equal("Yellow", file.Color_GetLabel(1));
        Assert.Equal(new BlockColor.Entry(255, 255, 0, 255), file.Color_Get(1));
    }

    [Fact]
    public static void MsbpWrite()
    {
        // Load in file
        MsbpFile file = MsbpFile.FromFilePath(ResDirectory + "ProjectData.msbp");
        Assert.True(file.IsValid());

        MemoryStream stream = new();
        Assert.True(file.WriteFile(stream));

        Directory.CreateDirectory(OutputDirectory);
        File.WriteAllBytes(OutputDirectory + "ProjectData.msbp", stream.ToArray());
    }
}