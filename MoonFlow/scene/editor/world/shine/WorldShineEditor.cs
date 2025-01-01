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

	[Signal]
	public delegate void ContentModifiedEventHandler();

	public void InitEditor(WorldInfo world, ShineInfo shine)
	{
		World = world;
		Shine = shine;
		
		OptionStageName.SetSelection(this, shine.StageName);
	}

	#region Signals

	private void OnShineStageNameChanged(string name)
	{
		if (Shine.StageName == name)
			return;
		
		Shine.StageName = name;
		EmitSignal(SignalName.ContentModified);
	}

	#endregion
}
