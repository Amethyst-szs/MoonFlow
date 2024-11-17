using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Nindot.UnitTest;

public class UnitTester
{
    private readonly List<Type> Tests = [];

    private int TestSuccessCount = 0;
    private int TestFailureCount = 0;

    public UnitTester(string[] args)
    {
        // Get romfs directory from args
        foreach (var arg in args)
        {
            if (arg.StartsWith("--romfs=")) Test.RomfsDirectory = arg["--romfs=".Length..];
        }

        // Ensure romfs directory is valid
        if (!Directory.Exists(Test.RomfsDirectory))
        {
            Console.Error.WriteLine("Invalid Romfs directory! Make sure to supply a no-space directory using --romfs=");
            Environment.Exit(0);
        }

        if (!Test.RomfsDirectory.EndsWith('/')) Test.RomfsDirectory += '/';

        // Get test list and output directory
        Tests = GetAllUnitTests();
        Directory.CreateDirectory(Test.TestOutputDirectory);

        RunTests();
    }

    static void Main(string[] args)
    {
        _ = new UnitTester(args);
    }

    public void RunTests()
    {
        Console.WriteLine("Unit Test > START");

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
            Console.Error.WriteLine("Supplied test does not inherit IUintTest");
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
        Console.WriteLine("Unit Test > {0} > {1}", "BEGIN", test);
    }
    private void TestPass(Type test)
    {
        TestSuccessCount += 1;
        Console.WriteLine("Unit Test > {0} > {1}", "   OK", test);
    }
    private void TestFailure(Type test)
    {
        TestFailureCount += 1;
        Console.WriteLine("Unit Test > {0} > {1}", "  ERR", test);
    }
    private void PrintTestResults()
    {
        // Print results
        Console.WriteLine("\nTest Results:");
        Console.WriteLine("Passed: {0}/{1}", TestSuccessCount, Tests.Count);

        if (TestFailureCount > 0)
            Console.Error.WriteLine("Error: {0}", TestFailureCount);

        if (Tests.Count == TestSuccessCount)
            Console.WriteLine("  -  All tests pass! :)");
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