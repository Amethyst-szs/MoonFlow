using Godot;
using System;

using MoonFlow.Project.Database;
using MoonFlow.Project;

namespace MoonFlow.Scene.EditorWorld;

public partial class WorldShineEditor : MarginContainer
{
	public WorldInfo World { get; private set; } = null;
	public ShineInfo Shine { get; private set; } = null;

	[Export, ExportGroup("Internal References")]
	private OptionStageName OptionStageName;
	[Export]
	private LineEdit LineObjId;

	[Export]
	private SpinBox SpinUID;
	[Export]
	private SpinBox SpinHint;
	[Export]
	private TextureRect TextureUIDWarning;
	[Export]
	private TextureRect TextureHintWarning;

	[Export]
	private Button ButtonTypeGrand;
	[Export]
	private Button ButtonTypeMoonRock;
	[Export]
	private Button ButtonTypeAchievement;

	[Export]
	private VBoxContainer QuestBitFlags;
	[Export]
	private VBoxContainer ScenarioBitFlags;

	[Signal]
	public delegate void ContentModifiedEventHandler();

	public void InitEditor(WorldInfo world, ShineInfo shine)
	{
		World = world;
		Shine = shine;

		OptionStageName.SetSelection(this, shine.StageName);
		LineObjId.Text = shine.ObjId;

		SpinUID.SetValueNoSignal(shine.UniqueId);
		SpinHint.SetValueNoSignal(shine.HintIdx);
		UpdateUniquenessWarnings();

		ButtonTypeGrand.SetPressedNoSignal(shine.IsGrand);
		ButtonTypeMoonRock.SetPressedNoSignal(shine.IsMoonRock);
		ButtonTypeAchievement.SetPressedNoSignal(shine.IsAchievement);

		BitFlagButtonHolder.SetValue(ScenarioBitFlags, shine.ProgressBitFlag);
		BitFlagButtonHolder.ConnectValueChanged(ScenarioBitFlags,
			new Action<int>(OnScenarioBitFlagsModified));
		
		BitFlagButtonHolder.SetPrimaryBit(QuestBitFlags, shine.MainScenarioNo);
		BitFlagButtonHolder.ConnectPrimaryBitChanged(QuestBitFlags,
			new Action<int>(OnQuestIdModified));
	}

	#region Signals

	private void OnShineStageNameChanged(string name)
	{
		if (Shine.StageName == name)
			return;

		Shine.StageName = name;
		EmitSignal(SignalName.ContentModified);
	}
	private void OnLineObjectIdModified(string txt)
	{
		if (Shine.ObjId == txt)
			return;

		Shine.ObjId = txt;
		EmitSignal(SignalName.ContentModified);
	}

	private void OnUniqueIdValueChanged(float valueF)
	{
		int value = (int)MathF.Floor(valueF);
		Shine.UniqueId = value;

		UpdateUniquenessWarnings();
	}
	private void OnUniqueIdAutoReassign()
	{
		var db = ProjectManager.GetDB();
		Shine.ReassignUID(db);

		SpinUID.Value = Shine.UniqueId;

		EmitSignal(SignalName.ContentModified);
	}

	private void OnHintIdValueChanged(float valueF)
	{
		int value = (int)MathF.Floor(valueF);
		Shine.HintIdx = value;

		UpdateUniquenessWarnings();
	}
	private void OnHintIdAutoReassign()
	{
		Shine.ReassignHintId(World);
		SpinHint.Value = Shine.HintIdx;

		EmitSignal(SignalName.ContentModified);
	}

	private void OnTypeGrandToggled(bool state)
	{
		Shine.IsGrand = state;
		EmitSignal(SignalName.ContentModified);
	}
	private void OnTypeMoonRockToggled(bool state)
	{
		Shine.IsMoonRock = state;
		EmitSignal(SignalName.ContentModified);
	}
	private void OnTypeAchievementToggled(bool state)
	{
		Shine.IsAchievement = state;
		EmitSignal(SignalName.ContentModified);
	}

	private void OnScenarioBitFlagsModified(int value)
	{
		Shine.ProgressBitFlag = value;
		EmitSignal(SignalName.ContentModified);
	}
	private void OnQuestIdModified(int idx)
	{
		Shine.MainScenarioNo = idx;
		EmitSignal(SignalName.ContentModified);
	}

	#endregion

	#region Utilities

	public void UpdateUniquenessWarnings()
	{
		var db = ProjectManager.GetDB();
		TextureUIDWarning.Visible = !Shine.IsUIDUnique(db);
		
		TextureHintWarning.Visible = !Shine.IsHintIdUnique(World);
	}

	#endregion
}
