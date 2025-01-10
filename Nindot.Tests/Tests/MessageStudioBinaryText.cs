using System;
using System.IO;
using System.Linq;
using Nindot.LMS;

using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;

using static Nindot.Tests.PathUtility;

namespace Nindot.Tests;

public class MessageStudioBinaryText
{
    [Fact]
    public static void MsbtParse()
    {
        // Load in an msbt resource and check it's validity
        var msbt = MsbtFile.FromFilePath(ResDirectory + "SmoUnitTesting.msbt", new MsbtElementFactoryProjectSmo());
        Assert.True(msbt.IsValid());

        // Ensure elements pass validity checks
        TestAllElements(msbt);

        // Test individual entries of the SmoUnitTesting.msbt file
        MsbtEntry cur = msbt.GetEntry("UnitTest_NoTag");
        Assert.NotNull(cur);
        Assert.Single(cur.Pages);
        Assert.Single(cur.Pages[0]);
        Assert.Equal("Hello, World!", cur.Pages[0][0].GetText());

        cur = msbt.GetEntry("UnitTest_PrintDelay");
        Assert.NotNull(cur);
        Assert.Single(cur.Pages);
        Assert.Equal(3, cur.Pages[0].Count);
        Assert.Equal((uint)0x69, ((MsbtTagElementEuiWait)cur.Pages[0][1]).DelayFrames);
    }

    [Fact]
    public static void MsbtWrite()
    {
        var msbt = MsbtFile.FromFilePath(ResDirectory + "SmoUnitTesting.msbt", new MsbtElementFactoryProjectSmo());
        Assert.True(msbt.IsValid());

        // Write file to stream
        MemoryStream stream = new();
        Assert.True(msbt.WriteFile(stream));

        // Write msbt to disk
        Directory.CreateDirectory(OutputDirectory);
        File.WriteAllBytes(OutputDirectory + "MsbtSMO.msbt", stream.ToArray());

        // Read the msbt back in from write
        msbt = MsbtFile.FromBytes(stream.ToArray(), "no_name", new MsbtElementFactoryProjectSmo());
        Assert.True(msbt.IsValid());

        TestAllElements(msbt);
    }

    [Fact]
    public static void MsbtAgainstSmo100() { TestAgainstPath(GetPathSmo100()); }
    [Fact]
    public static void MsbtAgainstSmo101() { TestAgainstPath(GetPathSmo101()); }
    [Fact]
    public static void MsbtAgainstSmo110() { TestAgainstPath(GetPathSmo110()); }
    [Fact]
    public static void MsbtAgainstSmo120() { TestAgainstPath(GetPathSmo120()); }
    [Fact]
    public static void MsbtAgainstSmo130() { TestAgainstPath(GetPathSmo130()); }

    #region Utilities

    private static void TestAllElements(MsbtFile msbt)
    {
        foreach (var label in msbt.GetEntryLabels())
        {
            foreach (MsbtPage page in msbt.GetEntry(label).Pages)
            {
                TestPage(label, page);
            }
        }
    }

    private static void TestPage(string label, MsbtPage page)
    {
        foreach (var element in page)
        {
            // If the current element is a text element, check validity and move on
            if (element.GetType() == typeof(MsbtTextElement))
            {
                Assert.True(element.IsValid());
                continue;
            }

            // At this point we know it is a tag so we can cast to the tag base class and run some more checks
            MsbtTagElement tag = (MsbtTagElement)element;

            Assert.True(tag.IsValid());
            Assert.Equal(tag.GetBytes().Length, tag.CalcDataSize() + 0x8);

            if (tag.GetType() == typeof(MsbtTagElementUnknown))
            {
                string warn = string.Format("{0} is tag group {1} ({2}) and subtype {3} ({4}), which created TagElementUnknown",
                    label, tag.GetGroupName(), Enum.GetName(typeof(TagGroup), tag.GetGroupName()),
                    tag.GetTagName(), tag.GetTagNameStr()
                );
                
                Console.WriteLine(warn);
                throw new LMSException(warn);
            }
        }
    }

    private static void TestAgainstPath(string path)
    {
        path += "LocalizedData/";
        var langs = Directory.GetDirectories(path).Select(p => p.Split(['/', '\\']).Last()).ToList();
        langs.Remove("Common");

        foreach (var lang in langs)
        {
            var localPath = path + lang + "/MessageData/";
            ReadSarcList(localPath, out SarcFile system, out SarcFile stage, out SarcFile layout);
            ScanSarcMsbt(system);
            ScanSarcMsbt(stage);
            ScanSarcMsbt(layout);
        }
    }

    private static void ReadSarcList(string path, out SarcFile system, out SarcFile stage, out SarcFile layout)
    {
        // Read in all three sarcs
        system = SarcFile.FromFilePath(path + "SystemMessage.szs");
        Assert.NotNull(system);
        Assert.NotEmpty(system.Content);

        stage = SarcFile.FromFilePath(path + "StageMessage.szs");
        Assert.NotNull(stage);
        Assert.NotEmpty(stage.Content);

        layout = SarcFile.FromFilePath(path + "LayoutMessage.szs");
        Assert.NotNull(layout);
        Assert.NotEmpty(layout.Content);
    }

    private static void ScanSarcMsbt(SarcFile sarc)
    {
        foreach (var x in sarc.Content.Keys)
        {
            Assert.Contains(".msbt", x);

            SarcMsbtFile file = sarc.GetFileMSBT(x, new MsbtElementFactoryProjectSmo());
            Assert.True(file.IsValid());

            TestAllElements(file);
        }
    }

    #endregion
}