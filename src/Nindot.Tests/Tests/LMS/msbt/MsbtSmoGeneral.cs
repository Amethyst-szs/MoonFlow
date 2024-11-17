using System.IO;

using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace Nindot.UnitTest;

public class UnitTestMsbtSMOWrite : IUnitTestGroup
{
    private static byte[] FileData = [];

    public static void SetupGroup()
    {
        FileData = File.ReadAllBytes("./src/Nindot.Tests/Resources/SmoUnitTesting.msbt");
    }

    [RunTest]
    public static void ReadWriteAndCheckMsbt()
    {
        // Load in an msbt resource and check it's validity
        MsbtFile msbt = new(new MsbtElementFactoryProjectSmo(), FileData);
        Test.Should(msbt.IsValid());

        // UnitTestMsbtSMOParse.TestAllElements(msbt.Content);

        // Write file to stream
        MemoryStream stream = new();
        Test.Should(msbt.WriteFile(stream));

        // Write msbt to disk
        File.WriteAllBytes(Test.TestOutputDirectory + "MsbtSMO.msbt", stream.ToArray());

        // Read the msbt back in from write
        FileData = File.ReadAllBytes(Test.TestOutputDirectory + "MsbtSMO.msbt");
        msbt = new(new MsbtElementFactoryProjectSmo(), FileData);
        Test.Should(msbt.IsValid());
    }

    public static void CleanupGroup()
    {
        FileData = null;
    }
}