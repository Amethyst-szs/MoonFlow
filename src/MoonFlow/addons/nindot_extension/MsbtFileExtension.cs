using System;
using System.IO;
using System.Linq;

using Nindot.LMS.Msbt.TagLib;

namespace Nindot.LMS.Msbt;

public static class MsbtFileExt
{
    public static MsbtFile FromGodotFilePath(string path, MsbtElementFactory factory)
    {
        if (!Godot.FileAccess.FileExists(path))
            throw new FileNotFoundException(path);
        
        var bytes = Godot.FileAccess.GetFileAsBytes(path);
        if (bytes.Length == 0)
            throw new FileLoadException(Enum.GetName(Godot.FileAccess.GetOpenError()));
        
        return MsbtFile.FromBytes(bytes, path.Split(['/', '\\']).Last(), factory);
    }
}