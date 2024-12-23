using System;
using System.IO;
using System.Linq;

namespace Nindot.LMS.Msbp;

public static class MsbpFileExt
{
    public static MsbpFile FromGodotFilePath(string path)
    {
        if (!Godot.FileAccess.FileExists(path))
            throw new FileNotFoundException(path);

        var bytes = Godot.FileAccess.GetFileAsBytes(path);
        if (bytes.Length == 0)
            throw new FileLoadException(Enum.GetName(Godot.FileAccess.GetOpenError()));

        return MsbpFile.FromBytes(bytes, path.Split(['/', '\\']).Last());
    }
}