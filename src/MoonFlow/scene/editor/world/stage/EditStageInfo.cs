using Godot;
using System;

using MoonFlow.Project.Database;

namespace MoonFlow.Scene.EditorWorld;

public partial class EditStageInfo : PanelContainer
{
	private WorldInfo World = null;
	private StageInfo Stage = null;

	[Signal]
	public delegate void RefreshListEventHandler();

	public void Setup(WorldInfo world, StageInfo stage)
	{
		World = world;
		Stage = stage;

		GetNode<Label>("%Label_Name").Text = stage.name;
		GetNode<OptionButton>("%Option_Type").Selected = (int)stage.CategoryType;
	}

	private void OnDeleteRequest()
	{
		World.StageList.Remove(Stage);
		EmitSignal(SignalName.RefreshList);
	}

	private void OnTypeChanged(int id)
	{
		Stage.CategoryType = (StageInfo.CatEnum)id;
		ProjectDatabaseHolder.SortWorldStagesByType(World.StageList);

		EmitSignal(SignalName.RefreshList);
	}
}
