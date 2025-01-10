using System;

using Nindot.Al.SMO;

using static Nindot.Tests.PathUtility;
using static Nindot.Al.SMO.RomfsValidation;
using System.IO;
using System.Linq;
using Xunit.Internal;

namespace Nindot.Tests;

public class ValidateRomfsValidation
{
    [Fact]
    public static void ValidatePathSmo100() { ValidateVersionAtPath(GetPathSmo100(), RomfsVersion.v100); }
    [Fact]
    public static void ValidatePathSmo101() { ValidateVersionAtPath(GetPathSmo101(), RomfsVersion.v101); }
    [Fact]
    public static void ValidatePathSmo110() { ValidateVersionAtPath(GetPathSmo110(), RomfsVersion.v110); }
    [Fact]
    public static void ValidatePathSmo120() { ValidateVersionAtPath(GetPathSmo120(), RomfsVersion.v120); }
    [Fact]
    public static void ValidatePathSmo130() { ValidateVersionAtPath(GetPathSmo130(), RomfsVersion.v130); }

    private static void ValidateVersionAtPath(string path, RomfsVersion target)
    {
        // Validate and access path
        ValidateAndUpdatePath(ref path, out RomfsVersion ver);
        if (ver != target)
        {
            var msg = string.Format("Invalid game version at path {0} ({1} -!-> {2})",
                path,
                Enum.GetName(ver),
                Enum.GetName(target)
            );

            throw new Exception(msg);
        }
    }
}