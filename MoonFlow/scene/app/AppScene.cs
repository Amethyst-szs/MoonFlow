using Godot;
using System;
using System.Threading.Tasks;

using MoonFlow.Scene.Main;
using MoonFlow.Async;

namespace MoonFlow.Scene;

[GlobalClass, Icon("res://iconS.png")]
public partial class AppScene : Control
{
	#region Properties

	// ~~~~~~~~~~~~~~~ Exports ~~~~~~~~~~~~~~~ //

	[Export, ExportGroup("Display")]
	public string AppName { get; private set; } = "Application";

	private string _appTaskbarTitle = "Application";
	[Export]
	public string AppTaskbarTitle
	{
		get { return _appTaskbarTitle; }
		protected set
		{
			_appTaskbarTitle = value;

			if (TaskbarButton != null)
			{
				TaskbarButton.Text = _appTaskbarTitle;
				TaskbarButton.TooltipText = _appTaskbarTitle;
			}

			EmitSignal(SignalName.AppTitleChanged);
		}
	}

	[Export]
	public Texture2D AppIcon { get; private set; } = GD.Load<Texture2D>("res://iconS.png");

	[Flags]
	public enum AppFlagEnum
	{
		IsAllowUserClose = 1 << 0,
		IsFocusOnOpen = 1 << 1,
		IsOpenInFront = 1 << 2,
		IsExclusive = 1 << 3,
		IsShowActionbar = 1 << 4,
		IsOnlyAllowOneInstance = 1 << 5,
		IsAllowUnsavedChanges = 1 << 6,
	}

	[Export(PropertyHint.Flags), ExportGroup("Flags")]
	public AppFlagEnum AppFlags = AppFlagEnum.IsAllowUserClose;

	[Export]
	private AsyncDisplay.Type AppContentSaveType = AsyncDisplay.Type.FileWrite;

	[Export, ExportGroup("Packed Scene")]
	private PackedScene UnsavedChangesScene = null;

	[Export, ExportGroup("Header Properties")]
	public WikiAccessorResource WikiPage { get; private set; }

	// ~~~~~~~~~~~~~~~~ State ~~~~~~~~~~~~~~~~ //

	private bool _isModified = false;
	protected bool IsModified
	{
		get { return _isModified; }
		set
		{
			_isModified = value;
			CallDeferred(MethodName.EmitSignal, SignalName.ModifyStateUpdate, _isModified);
		}
	}

	public string AppUniqueIdentifier { get; private set; } = null;

	// ~~~~~~~~~~~ Node References ~~~~~~~~~~~ //

	public MainSceneRoot Scene { get; protected set; } = null;

	private TaskbarButton _taskbarButton = null;
	public TaskbarButton TaskbarButton
	{
		get { return _taskbarButton; }
		set { _taskbarButton ??= value; }
	}

	// ~~~~~~~~~~~~~~~ Signals ~~~~~~~~~~~~~~~ //

	[Signal]
	public delegate void ModifyStateUpdateEventHandler(bool isModified);
	[Signal]
	public delegate void AppFocusedEventHandler();
	[Signal]
	public delegate void AppTitleChangedEventHandler();

	#endregion

	#region Initilization

	public AppScene()
	{
		Ready += InitReferences;
	}

	private void InitReferences()
	{
		// Warning check
		if (IsAppAllowUnsavedChanges() && !IsInstanceValid(UnsavedChangesScene))
			GD.PushError("No reference to unsaved changes dialog scene!");

		// Get access to the main
		var treeRoot = GetTree().CurrentScene;

		if (treeRoot.GetType() != typeof(MainSceneRoot))
			throw new Exception("Cannot initilize app outside of MainScene!");

		Scene = (MainSceneRoot)treeRoot;

		// Ensure this node was initilized from a scene, not a raw node
		if (SceneFilePath == null || SceneFilePath == string.Empty)
			throw new Exception(GetType().Name +
				" cannot be initilized without a scene! Use SceneCreator utility and ScenePath attribute.");

		// Queue taskbar item init for once main has completed ready
		if (!Scene.IsNodeReady())
		{
			Scene.Ready += InitAfterScene;
			return;
		}

		InitAfterScene();
	}

	private void InitAfterScene()
	{
		// Setup taskbar button
		if (!Scene.NodeTaskbar.TryAddApplication(this))
		{
			QueueFree();
			return;
		}

		// If this is the home scene, move self to the front of the taskbar
		if (IsAppOpenInFront())
		{
			TaskbarButton.GetParent().MoveChild(TaskbarButton, 0);
			Scene.NodeTaskbar.UpdateDisplay();
		}

		if (IsAppExclusive() || IsAppFocusOnOpen())
		{
			CallDeferred(MethodName.AppFocus);
		}
		else
		{
			ProcessMode = ProcessModeEnum.Disabled;
			Hide();
		}

		// Run optional virtual app init function
		AppInit();

		GD.Print(string.Format("Opened App: {0} ({1})", AppName, GetType().Name));
	}

	protected virtual void AppInit() { }

	#endregion

	#region App Controls

	public virtual string GetUniqueIdentifier(string input) { return AppName + input; }
	public void SetUniqueIdentifier(string input = "") { AppUniqueIdentifier = GetUniqueIdentifier(input); }
	public void AppFocus() { AppSceneServer.FocusApp(this); }

	public virtual SignalAwaiter AppClose(bool isEndExclusive = false)
	{
		if (IsModified && IsAppAllowUnsavedChanges())
		{
			AppearUnsavedChangesDialog(out GodotObject signalParent, out string signal);
			return ToSignal(signalParent, signal);
		}

		if (IsAppExclusive() && !isEndExclusive)
			return null;

		TaskbarButton.QueueFree();
		QueueFree();

		GD.Print(string.Format("Free App: {0} ({1})", AppName, GetType().Name));

		if (Visible)
		{
			int appIndex = TaskbarButton.GetIndex();
			bool isPreviousOK = Scene.NodeTaskbar.TrySelectAppByIndex(appIndex - 1);
			if (!isPreviousOK)
				Scene.NodeTaskbar.TrySelectAppByIndex(appIndex + 1);
		}

		return null;
	}

	public void AppCloseForce() { AppClose(true); }

	public virtual async Task<bool> TryCloseFromTreeQuit()
	{
		var result = AppClose(true);
		if (result != null)
			await result;

		return !IsModified;
	}

	#endregion

	#region Content Saving

	public async Task AppSaveContent(bool isRequireFocus)
	{
		if (!IsAppAllowUnsavedChanges())
			return;
		
		if (!AppIsFocused() && isRequireFocus)
            return;
		
		var run = AsyncRunner.Run(TaskWriteAppSaveContent, AppContentSaveType);
		
		await run.Task;
        await ToSignal(Engine.GetMainLoop(), "process_frame");

        if (!DisplayServer.WindowIsFocused())
            DisplayServer.WindowRequestAttention();

        if (run.Task.Exception == null)
            GD.Print("Saved content for " + AppTaskbarTitle);
        else
            GD.PrintErr("Saving encountered exception for " + AppTaskbarTitle);
	}

	protected virtual void TaskWriteAppSaveContent(AsyncDisplay display) { throw new NotImplementedException(); }

	#endregion

	#region Utilities

	public void AppearUnsavedChangesDialog(out GodotObject signalParent, out string signal, PackedScene scene = null, Action<bool> action = null)
	{
		AppFocus();

		scene ??= UnsavedChangesScene;
		var dialog = scene.Instantiate() as ConfirmationDialog;

		AddChild(dialog);
		dialog.Popup();

		// Create a default action if passed as null
		action ??= new Action<bool>((b) =>
		{
			if (!b)
				return;

			IsModified = false;
			AppClose(true);
		});

		signalParent = dialog;
		signal = "closed";
		dialog.Connect(signal, Callable.From(action));
	}

	public bool AppIsFocused() { return ProcessMode == ProcessModeEnum.Inherit && Visible; }

	public bool IsAppAllowUserToClose() { return (AppFlags & AppFlagEnum.IsAllowUserClose) != 0; }
	public bool IsAppFocusOnOpen() { return (AppFlags & AppFlagEnum.IsFocusOnOpen) != 0; }
	public bool IsAppOpenInFront() { return (AppFlags & AppFlagEnum.IsOpenInFront) != 0; }
	public bool IsAppExclusive() { return (AppFlags & AppFlagEnum.IsExclusive) != 0; }
	public bool IsAppShowHeader() { return (AppFlags & AppFlagEnum.IsShowActionbar) != 0; }
	public bool IsAppOnlyOneInstance() { return (AppFlags & AppFlagEnum.IsOnlyAllowOneInstance) != 0; }
	public bool IsAppAllowUnsavedChanges() { return (AppFlags & AppFlagEnum.IsAllowUnsavedChanges) != 0; }

	#endregion
}
