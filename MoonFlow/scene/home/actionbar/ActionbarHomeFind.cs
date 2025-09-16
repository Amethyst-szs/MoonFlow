using Godot;
using MoonFlow.Project;
using MoonFlow.Scene.Main;
using MoonFlow.Scene.Settings;

namespace MoonFlow.Scene.Home;

public partial class ActionbarHomeFind : ActionbarItemBase
{
	private enum MenuIds : int
	{
		SEARCH_FOR_STRING_IN_MSBT = 0,
	}

	public override void _Ready()
	{
		base._Ready();

		AssignFunction((int)MenuIds.SEARCH_FOR_STRING_IN_MSBT, OnSearchForStringInMsbt, "ui_find_search");
	}

	private void OnSearchForStringInMsbt()
	{
		var popup = SceneCreator<PopupMsbtFindSearchEntry>.Create();
		GetTree().CurrentScene.AddChild(popup);
		popup.Popup();
	}
}
