#if TOOLS
using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Nindot.UnitTest;

public partial class UnitTester : SceneTree
{
    private readonly List<Type> Tests = [];

    private int TestSuccessCount = 0;
    private int TestFailureCount = 0;

    public const string _100PathKey = "application/nindot/romfs_100_path_for_debug";
    public const string _130PathKey = "application/nindot/romfs_130_path_for_debug";

    public UnitTester()
    {
        Tests = GetAllUnitTests();
        DirAccess.MakeDirAbsolute("user://unit_test/");

#if UNIT_TEST
        RunTests();
#endif
    }

    public void RunTests()
    {
        foreach (var test in Tests)
        {
            RunTest(test);
        }

        PrintTestResults();
    }

    public bool RunTest(Type test)
    {
        if (test.IsSubclassOf(typeof(IUnitTest)))
        {
            GD.PushError("Supplied test does not inherit IUintTest");
            return false;
        }
        
        test.GetMethod("SetupTest").Invoke(null, []);

        try
        {
            TestStart(test);
            test.GetMethod("RunTest").Invoke(null, []);
        }
        catch
        {
            TestFailure(test);
            test.GetMethod("CleanupTest").Invoke(null, []);
            return false;
        }
        finally
        {
            TestPass(test);
            test.GetMethod("CleanupTest").Invoke(null, []);
        }

        return true;
    }

    private static void TestStart(Type test)
    {
        GD.Print(string.Format("Unit Test > {0} > {1}", "BEGIN", test));
    }
    private void TestPass(Type test)
    {
        TestSuccessCount += 1;
        GD.Print(string.Format("Unit Test > {0} > {1}", "   OK", test));
    }
    private void TestFailure(Type test)
    {
        TestFailureCount += 1;
        GD.PushError(string.Format("Unit Test > {0} > {1}", "  ERR", test));
    }
    private void PrintTestResults()
    {
        // Print results
        GD.Print("\nTest Results:");
        GD.Print(string.Format("Passed: {0}/{1}", TestSuccessCount, Tests.Count));

        if (TestFailureCount > 0)
            GD.PrintErr(string.Format("Error: {0}", TestFailureCount));

        if (Tests.Count == TestSuccessCount)
            GD.Print("  -  All tests pass! :)");
    }

    private static List<Type> GetAllUnitTests()
    {
        List<Type> tests = [];

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetInterface("IUnitTest") != null)
                {
                    tests.Add(type);
                }
            }
        }

        return tests;
    }
}

#endif