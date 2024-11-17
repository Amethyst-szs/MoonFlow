using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Nindot.Al.SMO;

namespace Nindot.UnitTest;

public class UnitTester
{
    private List<Type> TestGroups = [];

    private RomfsValidation.RomfsVersion SMORomfsVersion = RomfsValidation.RomfsVersion.INVALID_VERSION;

    private int TestCount = 0;
    private int TestSkipCount = 0;
    private int TestSuccessCount = 0;
    private int TestFailureCount = 0;

    public UnitTester(string[] args)
    {
        // Get romfs directory from args
        foreach (var arg in args)
        {
            if (arg.StartsWith("--romfs=")) Test.RomfsDirectory = arg["--romfs=".Length..];
            if (arg.StartsWith("--game=")) Test.GameName = arg["--game=".Length..];
        }

        // Set romfs directory to none if the args didn't supply this value
        if (!Directory.Exists(Test.RomfsDirectory)) Test.RomfsDirectory = "None";

        // Set game name to unknown if not definied by args
        Test.GameName ??= "None";

        // If the requested game is "SMO", run romfs validation
        SMORomfsVersion = RomfsValidation.RomfsVersion.INVALID_VERSION;
        if (Test.GameName.Equals("SMO"))
            SMORomfsVersion = SetupRomfsForSMO();

        RunTests();
    }

    static void Main(string[] args)
    {
        _ = new UnitTester(args);
    }

    public void RunTests()
    {
        // Get test list and output directory
        TestGroups = GetAllUnitTests(out TestCount);
        Directory.CreateDirectory(Test.TestOutputDirectory);

        // Log to console and start tests
        Console.WriteLine(TxtColorCyan + "  - GAME: {0}", Test.GameName);

        if (SMORomfsVersion != RomfsValidation.RomfsVersion.INVALID_VERSION)
            Console.WriteLine(TxtColorCyan + "  - VERSION: {0}", SMORomfsVersion.ToString());

        // Run each test group
        foreach (var group in TestGroups)
        {
            RunTestGroup(group);
        }

        PrintTestResults();
    }

    public void RunTestGroup(Type group)
    {
        if (group.IsSubclassOf(typeof(IUnitTestGroup)))
        {
            Console.Error.WriteLine("Supplied test does not inherit IUintTest");
            return;
        }

        group.GetMethod("SetupGroup").Invoke(null, []);

        GroupStart(group);
        foreach (var method in group.GetMethods(BindingFlags.Static | BindingFlags.Public))
        {
            if (method.GetCustomAttribute(typeof(RunTest)) == null)
                continue;

            // If this is an SMO romfs test and the unit test didn't get the --game=SMO arg, skip test
            if (method.GetCustomAttribute(typeof(SmoRomfsTest)) != null && !Test.GameName.Equals("SMO"))
            {
                TestSkipCount++;
                TestSkip(group, method);
                continue;
            }

            try
            {
                TestStart(group, method);
                method.Invoke(null, []);
            }
            catch
            {
                TestFailure(group, method);
            }
            finally
            {
                TestPass(group, method);
            }
        }

        group.GetMethod("CleanupGroup").Invoke(null, []);
    }

    private static void GroupStart(Type group)
    {
        Console.WriteLine("\n" + TxtColorCyan + " Unit Test > {0} > {1}", "GROUP".PadLeft(0x20),
            group.ToString().Replace("Nindot.UnitTest.", ""));
    }
    private static void TestStart(Type group, MethodInfo method)
    {
        Console.WriteLine(TxtColorWhite + " Unit Test > {0} > {1} > {2}", method.Name.PadLeft(0x18), "START",
            group.ToString().Replace("Nindot.UnitTest.", ""));
    }
    private static void TestSkip(Type group, MethodInfo method)
    {
        Console.WriteLine(TxtColorBlue + " Unit Test > {0} > {1} > {2}", method.Name.PadLeft(0x18), " SKIP",
            group.ToString().Replace("Nindot.UnitTest.", ""));
    }
    private void TestPass(Type group, MethodInfo method)
    {
        TestSuccessCount += 1;
        Console.WriteLine(TxtColorGreen + " Unit Test > {0} > {1} > {2}", method.Name.PadLeft(0x18), "   OK",
            group.ToString().Replace("Nindot.UnitTest.", ""));
    }
    private void TestFailure(Type group, MethodInfo method)
    {
        TestFailureCount += 1;
        Console.WriteLine(TxtColorRed + " Unit Test > {0} > {1} > {2}", method.Name.PadLeft(0x18), "  ERR",
            group.ToString().Replace("Nindot.UnitTest.", ""));
    }
    private void PrintTestResults()
    {
        // Print results
        Console.WriteLine("\n" + TxtColorWhite + " Test Results:");
        Console.WriteLine(TxtColorWhite + " Passed: {0}/{1}", TestSuccessCount, TestCount - TestSkipCount);

        if (TestSkipCount > 0)
            Console.Error.WriteLine(TxtColorBlue + " Skipped: {0}", TestSkipCount);

        if (TestFailureCount > 0)
            Console.Error.WriteLine(TxtColorRed + " Error: {0}", TestFailureCount);

        if (TestCount == TestSuccessCount)
            Console.WriteLine(TxtColorYellow + "  -  All tests pass! :)");
    }

    private const string TxtColorWhite = "\u001b[37m";
    private const string TxtColorRed = "\u001b[31m";
    private const string TxtColorYellow = "\u001b[33m";
    private const string TxtColorGreen = "\u001b[32m";
    private const string TxtColorBlue = "\u001b[34m";
    private const string TxtColorCyan = "\u001b[36m";

    private static List<Type> GetAllUnitTests(out int testCount)
    {
        List<Type> tests = [];
        testCount = 0;

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetInterface("IUnitTestGroup") == null)
                    continue;

                int attributeCount = 0;

                var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
                foreach (var method in methods)
                {
                    if (method.GetCustomAttribute(typeof(RunTest)) != null) attributeCount++;
                }

                if (attributeCount == 0)
                    continue;

                testCount += attributeCount;
                tests.Add(type);
            }
        }

        return tests;
    }

    private static RomfsValidation.RomfsVersion SetupRomfsForSMO()
    {
        string path = Test.RomfsDirectory;
        bool isValid = RomfsValidation.ValidateAndUpdatePath(ref path, out RomfsValidation.RomfsVersion ver);
        Test.RomfsDirectory = path;

        if (!isValid)
        {
            Console.Error.WriteLine("Romfs directory is \"{0}\", which isn't a valid SMO romfs directory!", path);
            Environment.Exit(0);
        }

        return ver;
    }
}