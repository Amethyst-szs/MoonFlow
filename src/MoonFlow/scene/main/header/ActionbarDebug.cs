using System;
using Godot;

using MoonFlow.Project;
using MoonFlow.Scene.Dev;
using MoonFlow.Scene.EditorEvent;

namespace MoonFlow.Scene.Main;

public partial class ActionbarDebug : ActionbarItemBase
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
		
		base._Ready();
		
		AssignFunction((int)MenuIds.OPEN_MSTXT_VIEWER, OnPressedOpenMstxtViewer);
		AssignFunction((int)MenuIds.OPEN_MSBP_TGG_VIEWER, OnPressedOpenMsbpTggViewer);
		AssignFunction((int)MenuIds.OPEN_MSBT_ENTRY_LOOKUP_POPUP, OnPressedOpenMsbtEntryLookup);
		AssignFunction((int)MenuIds.OPEN_EVENT_FLOW_GRAPH_PROTOTYPE, OnPressedOpenEventFlowPrototype);
	}

	private void OnPressedOpenMstxtViewer()
	{
		var mstxt = SceneCreator<MstxtViewerApp>.Create();
		ProjectManager.SceneRoot.NodeApps.AddChild(mstxt);
	}
	private void OnPressedOpenMsbpTggViewer()
	{
		var msbpTgg = SceneCreator<MsbpTggViewerApp>.Create();
		ProjectManager.SceneRoot.NodeApps.AddChild(msbpTgg);
	}
	private void OnPressedOpenMsbtEntryLookup()
	{
		var popup = SceneCreator<PopupMsbtSelectEntry>.Create();
		GetTree().CurrentScene.AddChild(popup);
		popup.Popup();
	}
	private void OnPressedOpenEventFlowPrototype()
	{
		var eventFlowGraph = SceneCreator<EventFlowApp>.Create();
		ProjectManager.SceneRoot.NodeApps.AddChild(eventFlowGraph);
	}
}
