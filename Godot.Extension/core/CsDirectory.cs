using System;
using System.IO;

namespace Godot.Extension;

public static class DirectoryExt
{
    public static void CopyFilesRecursively(string source, string target)
	{
		// Now Create all of the directories
		foreach (string dirPath in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
		{
			GD.Print("Creating directory: " + dirPath);
			Directory.CreateDirectory(dirPath.Replace(source, target));
		}

		// Delete all files currently in the target path
		foreach (string path in Directory.GetFiles(target, "*.*", SearchOption.AllDirectories))
		{
			GD.Print("Deleting file: " + path);
			File.Delete(path);
		}

		// Copy all the files & Replaces any files with the same name
		foreach (string newPath in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
		{
			GD.Print("Copying file: " + newPath);
			File.Copy(newPath, newPath.Replace(source, target), true);
		}
	}
}