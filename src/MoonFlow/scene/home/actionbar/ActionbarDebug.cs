using System;
using Godot;

using MoonFlow.Project;
using MoonFlow.Scene.Dev;
using MoonFlow.Scene.EditorEvent;

namespace MoonFlow.Scene.Home;

public partial class ActionbarDebug : PopupMenu
{
	private enum MenuIds : int
	{
		OPEN_MSTXT_VIEWER = 0,
		OPEN_MSBP_TGG_VIEWER = 1,
		OPEN_MSBT_ENTRY_LOOKUP_POPUP = 2,
		OPEN_EVENT_FLOW_GRAPH_PROTOTYPE = 3,
	}

	public override void _Ready()
	{
		// Free if not a debug build
		if (!OS.IsDebugBuild())
			QueueFree();
		
		// Connect to signal
		Connect(SignalName.IdPressed, Callable.From(new Action<MenuIds>(OnIdPressed)));
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
			case MenuIds.OPEN_MSBT_ENTRY_LOOKUP_POPUP:
				var popup = SceneCreator<PopupMsbtSelectEntry>.Create();
				GetTree().CurrentScene.AddChild(popup);
				popup.Popup();
				break;
			case MenuIds.OPEN_EVENT_FLOW_GRAPH_PROTOTYPE:
				var eventFlowGraph = SceneCreator<EventFlowApp>.Create();
				ProjectManager.SceneRoot.NodeApps.AddChild(eventFlowGraph);
				break;
			default:
				GD.PushWarning("Unknown MenuId! " + id);
				return;
		}
	}
}
