using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Godot;

namespace MoonFlow.Scene;

[ScenePath("res://scene/version/replace_old/replace_old_version_app.tscn"), Icon("res://asset/app/icon/update.png")]
public partial class ReplaceOldVersionApp : AppScene
{
	public const string CmdlineArgKeyTempDirectory = "--local_temp_dir";
	public const string CmdlineArgKeyTargetDirectory = "--target_dir";

	public override Task<bool> TryCloseFromTreeQuit()
	{
		return Task.FromResult(false);
	}

	public override async void _Ready()
	{
		base._Ready();

		await ToSignal(GetTree().CreateTimer(2.0), Timer.SignalName.Timeout);

		// Get paths from command line arguments
		var args = Cmdline.GetArgs();

		if (!args.TryGetValue(CmdlineArgKeyTempDirectory, out string tempDir))
			throw new ArgumentException("Missing cmdline arg " + CmdlineArgKeyTempDirectory);

		if (!args.TryGetValue(CmdlineArgKeyTargetDirectory, out string targetDir))
			throw new ArgumentException("Missing cmdline arg " + CmdlineArgKeyTargetDirectory);

		tempDir = tempDir.Replace("\"", "");
		targetDir = targetDir.Replace("\"", "");

		if (!tempDir.EndsWith('/')) tempDir += '/';
		if (!targetDir.EndsWith('/')) targetDir += '/';

		GD.Print("Temporary directory: " + tempDir);
		GD.Print("Target directory: " + targetDir);

		// Handle directory copy
		CopyFilesRecursively(tempDir, targetDir);

		// Launch new main executable with update cleanup launchmode
		var executableName = OS.GetExecutablePath().GetFile();
		var exePath = targetDir + executableName;

		List<string> newArgs = ["--"];

		newArgs.Add(string.Format("{0}={1}", // --launchmode argument to determine initial application
			AppSceneServer.CmdlineArgKeyLaunchmode,
			AppSceneServer.CmdlineArgValueLaunchmodeUpdateCleanup
		));

		OS.CreateProcess(exePath, [.. newArgs]);

		// Terminate this intermediary application
		GetTree().Quit();
	}

	private static void CopyFilesRecursively(string sourcePath, string targetPath)
	{
		// Now Create all of the directories
		foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
		{
			GD.Print("Creating directory: " + dirPath);
			Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
		}

		// Delete all files currently in the target path
		foreach (string path in Directory.GetFiles(targetPath, "*.*", SearchOption.AllDirectories))
		{
			GD.Print("Deleting file: " + path);
			File.Delete(path);
		}

		// Copy all the files & Replaces any files with the same name
		foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
		{
			GD.Print("Copying file: " + newPath);
			File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
		}
	}
}
