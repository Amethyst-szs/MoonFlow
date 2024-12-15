using Godot;
using MoonFlow.Project;
using MoonFlow.Scene.Dev;
using System;

namespace MoonFlow.Scene.Home;

public partial class ActionbarDebug : PopupMenu
{
	private enum MenuIds : int
	{
		OPEN_MSTXT_VIEWER = 0,
		OPEN_MSBP_TGG_VIEWER = 1,
	}

	public override void _Ready()
	{
		// Free if not a debug build
		if (!OS.IsDebugBuild())
			QueueFree();
		
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
			case MenuIds.OPEN_MSTXT_VIEWER:
				var mstxt = SceneCreator<MstxtViewerApp>.Create();
				ProjectManager.SceneRoot.NodeApps.AddChild(mstxt);
				break;
			case MenuIds.OPEN_MSBP_TGG_VIEWER:
				var msbpTgg = SceneCreator<MsbpTggViewerApp>.Create();
				ProjectManager.SceneRoot.NodeApps.AddChild(msbpTgg);
				break;
			default:
				GD.PushWarning("Unknown MenuId! " + id);
				return;
		}
	}
}
