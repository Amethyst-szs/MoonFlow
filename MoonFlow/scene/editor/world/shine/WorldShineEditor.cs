using Godot;
using System;

using MoonFlow.Project.Database;

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
		Shine.ReassignUID();
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

	#endregion

	#region Utilities

	public void UpdateUniquenessWarnings()
	{
		TextureUIDWarning.Visible = !Shine.IsUIDUnique();
		TextureHintWarning.Visible = !Shine.IsHintIdUnique(World);
	}

	#endregion
}
