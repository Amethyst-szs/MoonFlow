#if TOOLS
using Godot;
using System;

namespace Nindot.UnitTest;

public partial class UnitTester : SceneTree
{
    protected UnitTestBase[] TestList = [
        new UnitTestEventDataRead(),
        new UnitTestEventDataWrite(),
        new UnitTestEventDataSourceValidity(),
        new UnitTestBymlRead(),
        new UnitTestBymlWrite(),
        new UnitTestMsbtSMOParse(),
        new UnitTestMsbtSMOWrite(),
        new UnitTestMsbtUSen(),
        new UnitTestMsbtAllLang(),
        new UnitTestLmsHeaderReadWrite(),
        new UnitTestMsbpSMORead(),
        new UnitTestMsbp3DWRead(),
        new UnitTestMsbpSMOWrite(),
        new UnitTestMsbp3DWWrite(),
    ];

    protected int TestCount = 0;
    protected int TestSuccessCount = 0;
    protected int TestFailureCount = 0;

    public const string _100PathKey = "application/nindot/romfs_100_path_for_debug";
    public const string _130PathKey = "application/nindot/romfs_130_path_for_debug";

    public UnitTester()
    {
        DirAccess.MakeDirAbsolute("user://unit_test/");
        RunTests();
    }

    public void RunTests()
    {
        // Set test count to the total number of tests
        TestCount = TestList.Length;

        // Run all tests
        foreach(UnitTestBase test in TestList)
        {
            // If this test requires RomFS paths and they aren't set, skip this test with warning
            if (test.IsRequireRomFs() && (!ProjectSettings.HasSetting(_100PathKey) || !ProjectSettings.HasSetting(_130PathKey))) {
                test.PrintTestSkippedRomFs();
                break;
            }

            test.PrintTestStarted();
            test.SetupTest();

            switch (test.Test())
            {
                case UnitTestResult.OK:
                    TestSuccessCount += 1;
                    test.PrintTestSuccess();
                    break;
                case UnitTestResult.SKIP:
                    test.PrintTestSkipped();
                    break;
                case UnitTestResult.FAILURE:
                    TestFailureCount += 1;
                    test.Failure();
                    test.PrintTestError();
                    break;
            }

            test.CleanupTest();
        }

        // Print results
        GD.Print("\nTest Results:");
        GD.Print(string.Format("Passed: {0}/{1}", TestSuccessCount, TestCount));

        if (TestFailureCount > 0)
            GD.PrintErr(string.Format("Error: {0}", TestFailureCount));
        
        if (TestCount == TestSuccessCount)
            GD.Print("  -  All tests pass! :)");
    }
}

#endif