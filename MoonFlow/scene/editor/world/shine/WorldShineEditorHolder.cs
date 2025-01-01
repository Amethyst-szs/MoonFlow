using Godot;
using System;

using Nindot.LMS.Msbt;
using MoonFlow.Project.Database;
using MoonFlow.Project;
using MoonFlow.Ext;

namespace MoonFlow.Scene.EditorWorld;

[ScenePath("res://scene/editor/world/shine/world_shine_editor_holder.tscn")]
public partial class WorldShineEditorHolder : PanelContainer
{
	public WorldInfo World { get; private set; } = null;
	public ShineInfo Shine { get; private set; } = null;

	private ScrollContainer ParentScroll;

	[Export, ExportGroup("Internal References")]
	private Label LabelShineName;
	[Export]
	private Label LabelStageName;
	[Export]
	private TextureRect IconType;
	[Export]
	private TextureRect IconGrand;
	[Export]
	private SpinBox SpinIndex;

	[Signal]
	public delegate void ContentModifiedEventHandler();

	private static readonly Texture2D TextureIconAchievement = GD.Load<Texture2D>(
		"res://asset/nindot/lms/icon/PictureFont_31.png"
	);
	private static readonly Texture2D TextureIconMoonRock = GD.Load<Texture2D>(
		"res://asset/nindot/lms/icon/PictureFont_30.png"
	);

    public override void _Ready()
    {
        ParentScroll = this.FindParentByType<ScrollContainer>();
    }

    public void SetupShineEditor(WorldInfo world, ShineInfo shine, MsbtEntry entry, int idx)
	{
		// Copy references
		World = world;
		Shine = shine;

		// Setup visual display
		LabelShineName.Text = entry.GetRawText();
		LabelStageName.Text = shine.StageName;

		if (shine.IsAchievement) IconType.Texture = TextureIconAchievement;
		else if (shine.IsMoonRock) IconType.Texture = TextureIconMoonRock;

		IconGrand.Visible = shine.IsGrand;

		// Setup index spinner
		SpinIndex.MinValue = 0;
		SpinIndex.MaxValue = world.ShineList.Count - 1;
		SpinIndex.SetValueNoSignal(idx);
	}

	#region Signals

	private void OnSpinIndexValueSet(float idxF)
	{
		var idx = (int)MathF.Floor(idxF);

		World.ShineList.Remove(Shine);
		World.ShineList.Insert(idx, Shine);

		GetParent().MoveChild(this, idx);
		ParentScroll.CallDeferred(ScrollContainer.MethodName.EnsureControlVisible, this);

		EmitSignal(SignalName.ContentModified);
	}

	#endregion

	#region Utility

	public void UpdateShineIndex()
	{
		SpinIndex.SetValueNoSignal(GetIndex());
	}

	#endregion
}
