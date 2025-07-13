using Godot;
using System;

using MoonFlow.Project.Database;
using MoonFlow.Project;

namespace MoonFlow.Scene.EditorWorld;

[SceneUid("uid://c5upu8m5vshym")]
public partial class WorldShineEditor : MarginContainer
{
	public WorldInfo World { get; private set; } = null;
	public ShineInfo Shine { get; private set; } = null;

	[Export, ExportGroup("Internal References")]
	private OptionStageName OptionStageName;
	[Export]
	private LineEdit LineObjId;
	[Export]
	private LineEdit LineOptionalId;

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

	[Export]
	private SpinBox SpinTranslationX;
	[Export]
	private SpinBox SpinTranslationY;
	[Export]
	private SpinBox SpinTranslationZ;

	[Signal]
	public delegate void ContentModifiedEventHandler();

	public void InitEditor(WorldInfo world, ShineInfo shine)
	{
		World = world;
		Shine = shine;

		OptionStageName.SetSelection(this, shine.StageName);
		LineObjId.Text = shine.ObjId;
		LineOptionalId.Text = shine.OptionalId;

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
		
		SpinTranslationX.SetValueNoSignal(Shine.Trans.X);
		SpinTranslationY.SetValueNoSignal(Shine.Trans.Y);
		SpinTranslationZ.SetValueNoSignal(Shine.Trans.Z);
	}

	#region Signals

	private void OnShineStageNameChanged(string name)
	{
		if (Shine.StageName == name)
			return;

		Shine.StageName = name;
		EmitSignalContentModified();
	}
	private void OnLineObjectIdModified(string txt)
	{
		if (Shine.ObjId == txt)
			return;

		Shine.ObjId = txt;
		EmitSignalContentModified();
	}
	private void OnLineOptionalIdModified(string txt)
	{
		if (Shine.OptionalId == txt)
			return;

		if (txt == string.Empty)
			Shine.OptionalId = null;
		else
			Shine.OptionalId = txt;

		EmitSignalContentModified();
	}

	private void OnUniqueIdValueChanged(float valueF)
	{
		int value = (int)MathF.Floor(valueF);
		Shine.UniqueId = value;

		UpdateUniquenessWarnings();

		EmitSignalContentModified();
	}
	private void OnUniqueIdAutoReassign()
	{
		var db = ProjectManager.GetDB();
		Shine.ReassignUID(db);

		SpinUID.Value = Shine.UniqueId;

		EmitSignalContentModified();
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

		EmitSignalContentModified();
	}

	private void OnTypeGrandToggled(bool state)
	{
		Shine.IsGrand = state;
		EmitSignalContentModified();
	}
	private void OnTypeMoonRockToggled(bool state)
	{
		Shine.IsMoonRock = state;
		EmitSignalContentModified();
	}
	private void OnTypeAchievementToggled(bool state)
	{
		Shine.IsAchievement = state;
		EmitSignalContentModified();
	}

	private void OnScenarioBitFlagsModified(int value)
	{
		Shine.ProgressBitFlag = value;
		EmitSignalContentModified();
	}
	private void OnQuestIdModified(int idx)
	{
		Shine.MainScenarioNo = idx;
		EmitSignalContentModified();
	}

	private void OnTranslationXModified(float value)
	{
		Shine.Trans.X = value;
		EmitSignalContentModified();
	}
	private void OnTranslationYModified(float value)
	{
		Shine.Trans.Y = value;
		EmitSignalContentModified();
	}
	private void OnTranslationZModified(float value)
	{
		Shine.Trans.Z = value;
		EmitSignalContentModified();
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
