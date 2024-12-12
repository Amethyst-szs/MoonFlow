using Godot;
using MoonFlow.Project;
using System;

namespace MoonFlow.Scene.Home;

public partial class ActionbarProject : PopupMenu
{
	private enum MenuIds : int
	{
		PROJECT_RELOAD = 0,
		PROJECT_CLOSE = 1,
	}

	public override void _Ready()
	{
		// Assign shortcuts
		AssignShortcut(MenuIds.PROJECT_RELOAD, "home_actionbar_reload");
		AssignShortcut(MenuIds.PROJECT_CLOSE, "home_actionbar_close");

		// Connect to signal
		Connect(SignalName.IdPressed, Callable.From(new Action<MenuIds>(OnIdPressed)));
	}

	private void AssignShortcut(MenuIds id, string actionName)
	{
		var idx = GetItemIndex((int)id);

		var action = new InputEventAction { Action = actionName };

		var shortcut = new Shortcut();
		shortcut.Events.Add(action);
		SetItemShortcut(idx, shortcut);
	}

	private void OnIdPressed(MenuIds id)
	{
		switch (id)
		{
			case MenuIds.PROJECT_RELOAD:
				var path = ProjectManager.GetProject().Path;
				ProjectManager.TryOpenProject(path, out _);
				break;
			case MenuIds.PROJECT_CLOSE:
				ProjectManager.CloseProject();
				break;
			default:
				GD.PushWarning("Unknown MenuId! " + id);
				return;
		}
	}
}
