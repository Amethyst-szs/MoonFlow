using Godot;
using System;

using MoonFlow.Scene.Main;

namespace MoonFlow.Scene;

[GlobalClass, Icon("res://iconS.png")]
public partial class AppScene : Control
{
	// ====================================================== //
	// ======================= Exports ====================== //
	// ====================================================== //

	[Export]
	public string AppName { get; private set; } = "Application";

	[Export]
	public string AppTaskbarTitle
	{
		get { return _appTaskbarTitle; }
		protected set
		{
			_appTaskbarTitle = value;

			if (TaskbarButton != null)
				TaskbarButton.Text = _appTaskbarTitle;
		}
	}
	private string _appTaskbarTitle = "Application";

	public string AppUniqueIdentifier { get; private set; } = "";

	[Export]
	public Texture2D AppIcon { get; private set; } = GD.Load<Texture2D>("res://iconS.png");

	[Flags]
	public enum AppFlagEnum
	{
		IsAllowUserClose = 1 << 0,
		IsFocusOnOpen = 1 << 1,
		IsOpenInFront = 1 << 2,
		IsExclusive = 1 << 3,
		IsShowHeader = 1 << 4,
		IsOnlyAllowOneInstance = 1 << 5,
		IsAllowUnsavedChanges = 1 << 6,
	}

	[Export(PropertyHint.Flags)]
	public AppFlagEnum AppFlags = AppFlagEnum.IsAllowUserClose;

	[Export]
	private PackedScene UnsavedChangesScene = null;

	[Signal]
	public delegate void ModifyStateUpdateEventHandler(bool isModified);

	private bool _isModified = false;
	protected bool IsModified
	{
		get { return _isModified; }
		set
		{
			_isModified = value;
			EmitSignal(SignalName.ModifyStateUpdate, _isModified);
		}
	}

	// ====================================================== //
	// ================== Stored References ================= //
	// ====================================================== //

	public MainSceneRoot Scene { get; protected set; } = null;

	private TaskbarButton _taskbarButton = null;
	public TaskbarButton TaskbarButton
	{
		get { return _taskbarButton; }
		set { _taskbarButton ??= value; }
	}

	// ====================================================== //
	// ================== App Initilization ================= //
	// ====================================================== //

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
			AppFocus();

		// Initilize run virtual app init function
		AppInit();

		GD.Print(string.Format("Opened App: {0} ({1})", AppName, GetType().Name));
	}

	// ====================================================== //
	// ==================== App Virtuals ==================== //
	// ====================================================== //

	protected virtual void AppInit() { }

	public virtual string GetUniqueIdentifier(string input) { throw new NotImplementedException(); }
	public void SetUniqueIdentifier(string input) { AppUniqueIdentifier = GetUniqueIdentifier(input); }

	public virtual void AppFocus()
	{
		if (!IsInstanceValid(Scene)) return;

		// Set which app is being focused (usually "this" unless special window flags say otherwise)
		var focusingApp = this;

		var activeApp = Scene.GetActiveApp();
		if (IsInstanceValid(activeApp) && !activeApp.IsQueuedForDeletion())
		{
			// If the active app is exclusive and this app isn't, don't let the focused app change
			if (activeApp.IsAppExclusive() && !IsAppExclusive())
				focusingApp = activeApp;

			// If they are both exclusive, pick item with higher index
			else if (activeApp.IsAppExclusive() && IsAppExclusive())
			{
				if (activeApp.GetIndex() > focusingApp.GetIndex())
					focusingApp = activeApp;
			}
		}

		// Select this app's taskbar button
		foreach (var node in Scene.NodeTaskbar.GetChildren())
		{
			if (node.GetType() != typeof(TaskbarButton))
				continue;

			var button = (TaskbarButton)node;

			if (button.App != focusingApp)
				button.ButtonPressed = false;
			else
				button.ButtonPressed = true;
		}

		// Show only this app's visibility
		foreach (var node in Scene.GetApps())
		{
			var control = (Control)node;

			if (control != focusingApp)
			{
				control.Hide();
				control.ProcessMode = ProcessModeEnum.Disabled;
			}
			else
			{
				control.Show();
				control.ProcessMode = ProcessModeEnum.Inherit;
			}
		}

		// Update header visibility
		Scene.NodeHeader.Visible = focusingApp.IsAppShowHeader();
	}

	public SignalAwaiter AppClose(bool isEndExclusive = false)
	{
		if (IsModified && IsAppAllowUnsavedChanges())
			return AppearUnsavedChangesDialog();
		
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

	public virtual bool TryCloseFromTreeQuit(out SignalAwaiter awaiter)
	{
		awaiter = AppClose(true);

		if (awaiter == null)
			return true;
		
		return false;
	}

	public void AppCloseForce()
	{
		AppClose(true);
	}

	// ====================================================== //
	// ================== Dialog Utilities ================== //
	// ====================================================== //

	private SignalAwaiter AppearUnsavedChangesDialog()
	{
		var dialog = UnsavedChangesScene.Instantiate() as ConfirmationDialog;
		AddChild(dialog);

		dialog.Popup();

		var sig = ConfirmationDialog.SignalName.Confirmed;
		dialog.Connect(sig, Callable.From(() => {
			IsModified = false;
			AppClose(true);
		}));
		
		return ToSignal(dialog, sig);
	}

	// ====================================================== //
	// ================ App Options Utilities =============== //
	// ====================================================== //

	public bool AppIsFocused() { return ProcessMode == ProcessModeEnum.Inherit && Visible; }

	public bool IsAppAllowUserToClose() { return (AppFlags & AppFlagEnum.IsAllowUserClose) != 0; }
	public bool IsAppFocusOnOpen() { return (AppFlags & AppFlagEnum.IsFocusOnOpen) != 0; }
	public bool IsAppOpenInFront() { return (AppFlags & AppFlagEnum.IsOpenInFront) != 0; }
	public bool IsAppExclusive() { return (AppFlags & AppFlagEnum.IsExclusive) != 0; }
	public bool IsAppShowHeader() { return (AppFlags & AppFlagEnum.IsShowHeader) != 0; }
	public bool IsAppOnlyOneInstance() { return (AppFlags & AppFlagEnum.IsOnlyAllowOneInstance) != 0; }
	public bool IsAppAllowUnsavedChanges() { return (AppFlags & AppFlagEnum.IsAllowUnsavedChanges) != 0; }
}
