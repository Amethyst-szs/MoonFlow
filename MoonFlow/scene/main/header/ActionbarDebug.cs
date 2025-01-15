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
		TOGGLE_PROJECT_IS_DEBUG = 1000,

		OPEN_MSTXT_VIEWER = 10,
		OPEN_MSBP_TGG_VIEWER = 11,
		OPEN_MSBT_ENTRY_LOOKUP_POPUP = 12,
		OPEN_EVENT_FLOW_GRAPH_PROTOTYPE = 13,

		FORCE_EXCEPTION = 2000,
	}

	public override void _Ready()
	{
		if (!OS.IsDebugBuild())
		{
			QueueFree();
			return;
		}

		base._Ready();

		AssignFunction((int)MenuIds.TOGGLE_PROJECT_IS_DEBUG, OnToggleProjectIsDebug);
		AssignFunction((int)MenuIds.OPEN_MSTXT_VIEWER, OnPressedOpenMstxtViewer);
		AssignFunction((int)MenuIds.OPEN_MSBP_TGG_VIEWER, OnPressedOpenMsbpTggViewer);
		AssignFunction((int)MenuIds.OPEN_MSBT_ENTRY_LOOKUP_POPUP, OnPressedOpenMsbtEntryLookup);
		AssignFunction((int)MenuIds.OPEN_EVENT_FLOW_GRAPH_PROTOTYPE, OnPressedOpenEventFlowPrototype);
		AssignFunction((int)MenuIds.FORCE_EXCEPTION, OnForceException);

		AboutToPopup += OnAboutToAppear;
	}

	private void OnAboutToAppear()
	{
		var idx = GetItemIndex((int)MenuIds.TOGGLE_PROJECT_IS_DEBUG);

		SetItemAsCheckable(idx, true);
		SetItemChecked(idx, ProjectManager.IsProjectDebug());
	}

	private async void OnToggleProjectIsDebug()
	{
		// Update config
		var config = ProjectManager.GetProject().Config;
		config.SetDebugState(!config.IsDebug());

		config.WriteFile();

		// Update checkbox
		var idx = GetItemIndex((int)MenuIds.TOGGLE_PROJECT_IS_DEBUG);
		SetItemChecked(idx, config.IsDebug());

		// Reload project
		var isValidReload = await ProjectManager.SceneRoot.TryCloseAllApps();
		if (!isValidReload)
			return;
		
		var path = ProjectManager.GetProject().Path;
		ProjectManager.TryOpenProject(path, out _);
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

	private void OnForceException()
	{
		throw new MoonFlowIntentionalException();
	}
}

public class MoonFlowIntentionalException() : Exception();