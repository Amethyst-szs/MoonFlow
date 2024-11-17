using System;

using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;
using System.Util;
using System.IO;

namespace Nindot.UnitTest;

public class UnitTestMsbtSMOParse : IUnitTest
{
    private static byte[] FileData = [];

    public static void SetupTest()
    {
        FileData = File.ReadAllBytes("./src/Nindot.Tests/lms/msbt/SmoUnitTesting.msbt");
    }

    public static void RunTest()
    {
        // Load in an msbt resource and check it's validity
        MsbtFile msbt = new(new MsbtElementFactoryProjectSmo(), FileData);
        Test.Should(msbt.IsValid());

        // // Ensure elements pass general validity checks, used by all MSBT unit tests
        // TestAllElements(msbt.Content);

        // // Test individual entries of the SmoUnitTesting.msbt file
        // MsbtEntry cur = msbt.GetEntry("UnitTest_NoTag");
        // Test.ShouldNot(cur, null);
        // Test.Should(cur.Elements.Count, 1);
        // Test.Should(cur.Elements[0].GetText(), "Hello, World!");

        // cur = msbt.GetEntry("UnitTest_PrintDelay");
        // Test.ShouldNot(cur, null);
        // Test.Should(cur.Elements.Count, 3);
        // Test.Should(((MsbtTagElementEuiWait)cur.Elements[1]).DelayFrames == 0x69);
    }

    // public static void TestAllElements(OrderedDictionary<string, MsbtEntry> msbt)
    // {
    //     foreach (var key in msbt)
    //     {
    //         foreach (MsbtBaseElement element in key.Value.Elements)
    //         {
    //             // If the current element is a text element, check validity and move on
    //             if (element.GetType() == typeof(MsbtTextElement))
    //             {
    //                 Test.Should(element.IsValid());
    //                 continue;
    //             }

    //             // At this point we know it is a tag so we can cast to the tag base class and run some more checks
    //             MsbtTagElement tag = (MsbtTagElement)element;

    //             Test.Should(tag.IsValid());
    //             Test.Should(tag.GetBytes().Length, tag.CalcDataSize() + 0x8);

    //             // Only throw a warning for this, but note down if tag group is of an unknown type, since this should
    //             // never happen under normal circumstances, but doesn't inheritely mean something is wrong with the
    //             // parser or file
    //             if (tag.GetType() == typeof(MsbtTagElementUnknown))
    //             {
    //                 string warn = string.Format("{0} is tag group {1} ({2}) and subtype {3} ({4}), which created TagElementUnknown",
    //                     key.Key, tag.GetGroupName(), Enum.GetName(typeof(TagGroup), tag.GetGroupName()),
    //                     tag.GetTagName(), tag.GetTagNameStr()
    //                 );

    //                 GD.PushWarning(warn);
    //             }
    //         }
    //     }
    // }

    public static void CleanupTest()
    {
        FileData = null;
    }
}