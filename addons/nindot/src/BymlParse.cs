using Godot;
using System;

using BymlLibrary;

public partial class BymlParse : Node
{
    public static Godot.Collections.Dictionary ParseBytes(byte[] data)
    {
        return [];
    }

    public static Godot.Collections.Dictionary ParsePath(string path)
    {
        if (!DirAccess.DirExistsAbsolute(path))
            return [];
        
        return ParseBytes(FileAccess.GetFileAsBytes(path));
    }
}
