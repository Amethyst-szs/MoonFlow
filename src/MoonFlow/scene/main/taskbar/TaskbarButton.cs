using Godot;
using System;

namespace MoonFlow.Scene.Main;

public partial class TaskbarButton : Button
{
	public AppScene App { get; private set; } = null;

	private Panel UnsavedDot = null;
	private static readonly StyleBoxFlat UnsavedDotStyle
		= GD.Load<StyleBoxFlat>("res://asset/theme/main/stylebox/taskbar_dot.tres");

	public TaskbarButton(AppScene app)
	{
		// Setup button
		App = app;
		app.TaskbarButton = this;

		Text = app.AppTaskbarTitle;
		Name = app.AppTaskbarTitle;
		Icon = app.AppIcon;

		ButtonMask |= MouseButtonMask.Middle;
		ActionMode = ActionModeEnum.Press;
		ToggleMode = true;
		SetPressedNoSignal(false);

		ClipText = true;
		ExpandIcon = true;
		TextOverrunBehavior = TextServer.OverrunBehavior.TrimChar;

		// Setup unsaved changes dot if app supports it
		if (app.IsAppAllowUnsavedChanges())
		{
			UnsavedDot = new()
			{
				Visible = false,
				ZIndex = 1000,
			};

			UnsavedDot.AddThemeStyleboxOverride("panel", UnsavedDotStyle);
			AddChild(UnsavedDot);

			UnsavedDot.SetAnchorsPreset(LayoutPreset.BottomRight);

			app.Connect(AppScene.SignalName.ModifyStateUpdate,
				Callable.From(new Action<bool>(OnModifyStateChanged)));
		}
	}

	public override void _Pressed()
	{
		if ((Input.GetMouseButtonMask() & MouseButtonMask.Middle) != 0)
		{
			// If the active app is exclusive (and this isn't the active app), ignore
			var activeApp = App.Scene.GetActiveApp();
			if (activeApp != App && activeApp.IsAppExclusive())
			{
				SetPressedNoSignal(false);
				return;
			}

			// Do not allow closing the home page application
			if (!App.IsAppAllowUserToClose())
			{
				SetPressedNoSignal(true);
				return;
			}

			App.AppClose(true);
			return;
		}

		// If not a middle click, just focus app normally
		App.AppFocus();
	}

	private void OnModifyStateChanged(bool isModified)
	{
		if (IsInstanceValid(UnsavedDot))
			UnsavedDot.SetDeferred(PropertyName.Visible, isModified);
	}
}
