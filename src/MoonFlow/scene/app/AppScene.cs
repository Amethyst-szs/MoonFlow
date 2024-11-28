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
	public string AppTaskbarTitle { get; private set; } = "Application";

	[Export]
	public Texture2D AppIcon { get; private set; } = GD.Load<Texture2D>("res://iconS.png");

	[Flags]
	public enum AppFlagEnum
	{
		IsAllowUserClose = 1 << 0,
		IsFocusOnOpen = 1 << 1,
		IsOpenInFront = 1 << 2,
		IsExclusive = 1 << 3,
	}

	[Export(PropertyHint.Flags)]
	private AppFlagEnum AppFlags = AppFlagEnum.IsAllowUserClose;

	// ====================================================== //
	// ==================== Stored Values =================== //
	// ====================================================== //

	private TaskbarButton _taskbarButton = null;
	public TaskbarButton TaskbarButton
	{
		get { return _taskbarButton; }
		set { _taskbarButton ??= value; }
	}

	private MainSceneRoot Scene = null;

	// ====================================================== //
	// ============ App Setup And Close Functions =========== //
	// ====================================================== //

	public override void _Ready()
	{
		// Get access to the main
		var treeRoot = GetTree().CurrentScene;

		if (treeRoot.GetType() != typeof(MainSceneRoot))
			throw new Exception("Cannot initilize app outside of MainScene!");

		Scene = (MainSceneRoot)treeRoot;

		// Queue taskbar item init for once main has completed ready
		Scene.Ready += InitTaskbarItem;
	}

	private void InitTaskbarItem()
	{
		Scene.NodeTaskbar.AddApplication(this);

		// If this is the home scene, move self to the front of the taskbar
		if (IsAppOpenInFront())
		{
			TaskbarButton.GetParent().MoveChild(TaskbarButton, 0);
			Scene.NodeTaskbar.UpdateDisplay();
		}

		if (IsAppExclusive() || IsAppFocusOnOpen())
			FocusApp();
	}

	public void FocusApp()
	{
		if (!IsInstanceValid(Scene)) return;

		// Set which app is being focused (usually "this" unless special window flags say otherwise)
		var focusingApp = this;
		var activeApp = Scene.GetActiveApp();
		if (IsInstanceValid(activeApp))
		{
			// If the active app is exclusive, don't let the focused app change
			if (activeApp.IsAppExclusive())
				focusingApp = activeApp;
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
	}

	public virtual void CloseApp()
	{
		if (Visible)
		{
			int appIndex = TaskbarButton.GetIndex();
			bool isPreviousOK = Scene.NodeTaskbar.TrySelectAppByIndex(appIndex - 1);
			if (!isPreviousOK)
				Scene.NodeTaskbar.TrySelectAppByIndex(appIndex + 1);
		}

		TaskbarButton.QueueFree();
		QueueFree();
	}

	// ====================================================== //
	// ================ App Options Utilities =============== //
	// ====================================================== //

	public bool IsAppAllowUserToClose() { return (AppFlags & AppFlagEnum.IsAllowUserClose) != 0; }
	public bool IsAppFocusOnOpen() { return (AppFlags & AppFlagEnum.IsFocusOnOpen) != 0; }
	public bool IsAppOpenInFront() { return (AppFlags & AppFlagEnum.IsOpenInFront) != 0; }
	public bool IsAppExclusive() { return (AppFlags & AppFlagEnum.IsExclusive) != 0; }
}
