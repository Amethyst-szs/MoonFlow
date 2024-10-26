#if UNIT_TEST
using Godot;
using System;

namespace Nindot.UnitTest;

public partial class UnitTester : SceneTree
{
    protected UnitTestBase[] TestList = [
        new UnitTestMsbtSmoParse()
    ];

    protected int TestCount = 0;
    protected int TestSuccessCount = 0;
    protected int TestWarningCount = 0;
    protected int TestFailureCount = 0;

    public UnitTester()
    {
        RunTests();
    }

    public void RunTests()
    {
        // Set test count to the total number of tests
        TestCount = TestList.Length;

        // Run all tests
        foreach(UnitTestBase test in TestList)
        {
            test.PrintTestStarted();
            test.SetupTest();

            switch (test.Test())
            {
                case UnitTestResult.OK:
                    TestSuccessCount += 1;
                    test.PrintTestSuccess();
                    break;
                case UnitTestResult.SKIP:
                    TestSuccessCount += 1;
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

        if (TestWarningCount > 0)
            GD.Print(string.Format("Warning: {0}", TestWarningCount));
        if (TestFailureCount > 0)
            GD.PrintErr(string.Format("Error: {0}", TestFailureCount));
        
        if (TestCount == TestSuccessCount)
            GD.Print("  -  All tests pass! :)");
    }
}

#endif