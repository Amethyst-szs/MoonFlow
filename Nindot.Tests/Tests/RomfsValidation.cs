using System;

using static Nindot.Tests.PathUtility;
using static Nindot.RomfsPathUtility;
using System.IO;
using System.Linq;

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

        var source = Directory.GetFiles("D:/NCA-NSP-XCI_TO_LayeredFS_v1.6/1.6/Super-Mario-Oddyesy/Odyssey120/romfs/ObjectData/", "*.szs", SearchOption.TopDirectoryOnly);
        var list = Directory.GetFiles(path + "ObjectData/", "*.szs", SearchOption.TopDirectoryOnly);
        
        source = [.. source.Select(s => s.Split(['\\', '/']).Last())];
        list = [.. list.Select(s => s.Split(['\\', '/']).Last())];

        var dif = list.ToList().FindAll(l => !source.Contains(l));

        string output = string.Empty;

        foreach (var file in dif)
            output += "\"" + file + "\",";
        
        File.WriteAllText(PathUtility.OutputDirectory + target.ToString() + "_ObjectTable.txt", output);
    }
}