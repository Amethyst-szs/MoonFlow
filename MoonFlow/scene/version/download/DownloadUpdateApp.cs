using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Godot;

namespace MoonFlow.Scene;

[ScenePath("res://scene/version/download/download_update_app.tscn"), Icon("res://asset/app/icon/update.png")]
public partial class DownloadUpdateApp : AppScene
{
	public const string DownloadTempFile = "working.zip";
	public const string DownloadTempTarget = "user://" + DownloadTempFile;

	public const string ExtractionPath = "extract_temp/";

	public override Task<bool> TryCloseFromTreeQuit()
	{
		return Task.FromResult(!IsExtraction);
	}

	#region Download Process

	[Export, ExportGroup("Internal References"), ExportSubgroup("Download Process")]
	private VBoxContainer ContainerDownload;
	[Export]
	private ProgressBar ProgressDownload;
	[Export]
	private Label LabelProgressDownload;

	private HttpRequest DownloadRequest = null;
	private int DownloadByteSize = -1;

	public void InitUpdate(string url, int byteSize)
	{
		SetVisibleContainer(ContainerDownload);
		DownloadByteSize = byteSize;

		// Create http request godot object
		DownloadRequest = new HttpRequest { DownloadFile = DownloadTempTarget };
		AddChild(DownloadRequest);

		DownloadRequest.RequestCompleted += OnDownloadRequestComplete;

		if (DownloadRequest.Request(url) != Error.Ok)
		{
			OnFailure(true);
			return;
		}
	}

	private void OnDownloadRequestComplete(long result, long responseCode, string[] headers, byte[] body)
	{
		DownloadRequest.QueueFree();
		DownloadRequest = null;

		if (responseCode != 200)
		{
			GD.Print("Update failed with HTTP response code: " + responseCode);
			OnFailure(false, responseCode);
			return;
		}

		BeginExtraction();
	}

	#endregion

	#region Extraction

	[Export, ExportGroup("Internal References"), ExportSubgroup("Extraction")]
	private VBoxContainer ContainerExtract;

	private bool IsExtraction = false;

	private void BeginExtraction()
	{
		IsExtraction = true;

		if (!Godot.FileAccess.FileExists(DownloadTempTarget))
			throw new FileNotFoundException("Download completed successfully but no destination file!");

		// Attempt to lookup and verify downloaded file
		var userDir = GetUserDir();

		var path = userDir + DownloadTempFile;
		if (!File.Exists(path))
			throw new FileNotFoundException("Could not resolve temp zip destination!");

		// Update user interface
		SetVisibleContainer(ContainerExtract);

		// Start async extraction task
		Task.Run(() => TaskRunExtract(path, userDir + ExtractionPath)).ContinueWith(OnExtractComplete);
	}

	private static void TaskRunExtract(string zipPath, string outPath)
	{
		if (Directory.Exists(outPath))
			Directory.Delete(outPath, true);

		ZipFile.ExtractToDirectory(zipPath, outPath);
	}

	private void OnExtractComplete(Task task)
	{
		if (task != null && task.Exception != null)
		{
			OnFailure(true);
			return;
		}

		// Get current executable path
		var executablePath = OS.GetExecutablePath().GetBaseDir().Replace('\\', '/');
		if (!executablePath.EndsWith('/')) executablePath += '/';

		var executableName = OS.GetExecutablePath().GetFile();

		try
		{
			if (executableName.Contains("godot", StringComparison.OrdinalIgnoreCase))
				throw new Exception("Cannot continue with auto-update process from Godot Editor instance");
		}
		catch
		{
			OnFailure(true);
			return;
		}

		// Ensure extracted directory has a launchable MoonFlow executable
		var exePath = GetUserDir() + ExtractionPath + executableName;

		try
		{
			if (!File.Exists(exePath))
				throw new FileNotFoundException("Could not locate " + exePath);
		}
		catch
		{
			OnFailure(true);
			return;
		}

		// Create argument list
		List<string> args = ["--"];

		args.Add(string.Format("{0}={1}", // --launchmode argument to determine initial application
			AppSceneServer.CmdlineArgKeyLaunchmode,
			AppSceneServer.CmdlineArgValueLaunchmodeUpdateReplaceOld
		));

		args.Add(string.Format("{0}=\"{1}\"", // Temp install's directory path
			ReplaceOldVersionApp.CmdlineArgKeyTempDirectory,
			GetUserDir() + ExtractionPath
		));

		args.Add(string.Format("{0}=\"{1}\"", // Current application's path
			ReplaceOldVersionApp.CmdlineArgKeyTargetDirectory,
			executablePath
		));

		foreach (var arg in args)
			GD.Print(arg);

		// Launch new process and terminate self
		OS.CreateProcess(exePath, [.. args]);
		GetTree().CallDeferred(SceneTree.MethodName.Quit);
	}

	#endregion

	#region Failure

	[Export, ExportGroup("Internal References"), ExportSubgroup("Failure")]
	private VBoxContainer ContainerFailure;
	[Export]
	private Label LabelFailInfo;
	[Export]
	private Label LabelFailExtract;

	private void OnFailure(bool isExtractFail, long httpCode = -1)
	{
		SetVisibleContainer(ContainerFailure);

		LabelFailInfo.SetDeferred(Label.PropertyName.Text, "HTTP code: " + httpCode);
		if (httpCode == -1) LabelFailInfo.CallDeferred(MethodName.Hide);

		LabelFailExtract.SetDeferred(PropertyName.Visible, isExtractFail);
	}

	private void OnAcceptFailureScreen()
	{
		AppSceneServer.CreateApp<FrontDoor>();
		AppCloseForce();
	}

	#endregion

	#region UI

	public override void _Process(double delta)
	{
		if (IsInstanceValid(DownloadRequest))
		{
			int currentByte = DownloadRequest.GetDownloadedBytes();
			string cur = ByteSizeLib.ByteSize.FromBytes(currentByte).ToString(".0");
			string max = ByteSizeLib.ByteSize.FromBytes(DownloadByteSize).ToString(".0");

			ProgressDownload.MaxValue = DownloadByteSize;
			ProgressDownload.Value = currentByte;

			LabelProgressDownload.Text = string.Format("{0}/{1}", cur, max);
		}
	}

	private void SetVisibleContainer(Control container)
	{
		ContainerDownload.CallDeferred(MethodName.Hide);
		ContainerExtract.CallDeferred(MethodName.Hide);
		ContainerFailure.CallDeferred(MethodName.Hide);

		container.CallDeferred(MethodName.Show);
	}

	#endregion

	#region Utility

	public static async void CleanupTemporaryUpdateFiles()
	{
		GD.Print("Cleaning up temporary update files...");
		
		var loop = Engine.GetMainLoop() as SceneTree;
		await loop.ToSignal(loop.CreateTimer(2.0), Timer.SignalName.Timeout);

		var userDir = GetUserDir();

		var tempZip = userDir + DownloadTempFile;
		if (File.Exists(tempZip))
			File.Delete(tempZip);
		
		var tempDir = userDir + ExtractionPath;
		if (Directory.Exists(tempDir))
			Directory.Delete(tempDir, true);
	}

	private static string GetUserDir()
	{
		string userDir = OS.GetUserDataDir().Replace('\\', '/');
		if (!userDir.EndsWith('/')) userDir += '/';

		return userDir;
	}

	#endregion
}
