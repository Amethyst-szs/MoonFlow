using Godot;
using System;

using MoonFlow.Project.Database;
using MoonFlow.Scene.EditorMsbt;
using MoonFlow.Scene.Home;

namespace MoonFlow.Scene.EditorWorld;

public partial class VBoxBasicInfo : InfoBoxBase
{
	[Export]
	private LineEdit LineHomeStage;

	[Export]
	private OptionButton OptionShineType;
	[Export]
	private SpinBox CustomShineType;

	[Export]
	private OptionCoinType OptionCoinType;
	[Export]
	private LineEdit CustomCoinType;

	[Export]
	private SpinBox CountCoinCollect;

	public override void OpenWorld(WorldInfo world)
	{
		base.OpenWorld(world);

		LineHomeStage.Text = world.Name;

		OptionShineType.Call("set_selection", world.WorldItemType.Shine);
		CustomShineType.Value = world.WorldItemType.Shine;

		OptionCoinType.SetSelection(world.WorldItemType.CoinCollect);
		CustomCoinType.Text = world.WorldItemType.CoinCollect;

		CountCoinCollect.Value = world.CoinCollectInfo.CollectCoinNum;
	}

	#region Signals

	private void OnHomeStageNameModified(string txt)
	{
		Info.Name = txt;
		EmitSignal(SignalName.ModifiedWorldInfo);
	}

	private void OnOpenMSBTPressed()
	{
		var s = Info.WorldName;
		
		var editor = MsbtAppHolder.OpenAppWithSearch("SystemMessage.szs", "StageMap.msbt", s);
		editor.Editor.SetSelection("WorldName_" + s);

		editor = MsbtAppHolder.OpenAppWithSearch("SystemMessage.szs", "StageName.msbt", s);
		editor.Editor.SetSelection("WorldName_" + s);
	}

	private void OnSetShineTypeID(int id)
	{
		Info.WorldItemType.Shine = id;
		EmitSignal(SignalName.ModifiedItemInfo);
	}

	private void OnSetCoinTypeID(string id)
	{
		Info.WorldItemType.CoinCollect = id;
		EmitSignal(SignalName.ModifiedItemInfo);
	}

	private void OnSetCoinCount(int count)
	{
		Info.CoinCollectInfo.CollectCoinNum = count;
		EmitSignal(SignalName.ModifiedWorldInfo);
	}

	#endregion
}
