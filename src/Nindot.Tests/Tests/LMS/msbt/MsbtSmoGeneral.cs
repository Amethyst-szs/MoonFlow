using System;
using System.IO;

using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace Nindot.UnitTest;

public class UnitTestMsbtSMOGeneral : IUnitTestGroup
{
    private static byte[] FileData = [];

    public static void SetupGroup()
    {
        FileData = File.ReadAllBytes("./src/Nindot.Tests/Resources/SmoUnitTesting.msbt");
    }

    [RunTest]
    public static void ParseUnitTestingMsbt()
    {
        // Load in an msbt resource and check it's validity
        MsbtFile msbt = new(new MsbtElementFactoryProjectSmo(), FileData);
        Test.Should(msbt.IsValid());

        // Ensure elements pass general validity checks, used by all MSBT unit tests
        TestAllElements(msbt);

        // Test individual entries of the SmoUnitTesting.msbt file
        MsbtEntry cur = msbt.GetEntry("UnitTest_NoTag");
        Test.ShouldNot(cur, null);
        Test.Should(cur.Elements.Count, 1);
        Test.Should(cur.Elements[0].GetText(), "Hello, World!");

        cur = msbt.GetEntry("UnitTest_PrintDelay");
        Test.ShouldNot(cur, null);
        Test.Should(cur.Elements.Count, 3);
        Test.Should(((MsbtTagElementEuiWait)cur.Elements[1]).DelayFrames == 0x69);
    }

    [RunTest]
    public static void ReadWriteAndCheckMsbt()
    {
        // Load in an msbt resource and check it's validity
        MsbtFile msbt = new(new MsbtElementFactoryProjectSmo(), FileData);
        Test.Should(msbt.IsValid());

        TestAllElements(msbt);

        // Write file to stream
        MemoryStream stream = new();
        Test.Should(msbt.WriteFile(stream));

        // Write msbt to disk
        File.WriteAllBytes(Test.TestOutputDirectory + "MsbtSMO.msbt", stream.ToArray());

        // Read the msbt back in from write
        FileData = File.ReadAllBytes(Test.TestOutputDirectory + "MsbtSMO.msbt");
        msbt = new(new MsbtElementFactoryProjectSmo(), FileData);
        Test.Should(msbt.IsValid());

        TestAllElements(msbt);
    }

    public static void CleanupGroup()
    {
        FileData = null;
    }

    public static void TestAllElements(MsbtFile msbt)
    {
        foreach (var label in msbt.GetEntryLabels())
        {
            foreach (MsbtBaseElement element in msbt.GetEntry(label).Elements)
            {
                // If the current element is a text element, check validity and move on
                if (element.GetType() == typeof(MsbtTextElement))
                {
                    Test.Should(element.IsValid());
                    continue;
                }

                // At this point we know it is a tag so we can cast to the tag base class and run some more checks
                MsbtTagElement tag = (MsbtTagElement)element;

                Test.Should(tag.IsValid());
                Test.Should(tag.GetBytes().Length, tag.CalcDataSize() + 0x8);

                // Only throw a warning for this, but note down if tag group is of an unknown type, since this should
                // never happen under normal circumstances, but doesn't inheritely mean something is wrong with the
                // parser or file
                if (tag.GetType() == typeof(MsbtTagElementUnknown))
                {
                    string warn = string.Format("{0} is tag group {1} ({2}) and subtype {3} ({4}), which created TagElementUnknown",
                        label, tag.GetGroupName(), Enum.GetName(typeof(TagGroup), tag.GetGroupName()),
                        tag.GetTagName(), tag.GetTagNameStr()
                    );

                    Console.WriteLine(warn);
                }
            }
        }
    }
}