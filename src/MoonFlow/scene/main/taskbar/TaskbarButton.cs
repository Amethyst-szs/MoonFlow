using Godot;
using System;

namespace MoonFlow.Scene.Main;

public partial class TaskbarButton : Button
{
	public AppScene App { get; private set; } = null;

	public TaskbarButton(AppScene app)
	{
		App = app;
		app.TaskbarButton = this;

		Text = app.AppTaskbarTitle;
		Icon = app.AppIcon;

		ButtonMask |= MouseButtonMask.Middle;
		ActionMode = ActionModeEnum.Press;
		ToggleMode = true;
		SetPressedNoSignal(false);

		ClipText = true;
		ExpandIcon = true;
		TextOverrunBehavior = TextServer.OverrunBehavior.TrimChar;
	}

	public override void _Pressed()
	{
		if ((Input.GetMouseButtonMask() & MouseButtonMask.Middle) != 0)
		{
			// Do not allow closing the home page application
			if (!App.IsAppAllowUserToClose())
			{
				SetPressedNoSignal(true);
				return;
			}

			App.CloseApp();
			return;
		}

		// If not a middle click, just focus app normally
		App.FocusApp();
	}
}
