using Godot;
using System;

using MoonFlow.Project;

namespace MoonFlow.Scene.EditorEvent;

[ScenePath("res://scene/editor/event/block/event_block_panel.tscn")]
public partial class EventBlockPanel : PanelContainer
{
	private string Id;
	private GraphMetaBucketCommon GraphMeta;
	public GraphMetaBucketBlock BlockMeta { get; private set; }

	[Export, ExportGroup("Internal Referneces")]
	private Label LabelHeader;
	[Export]
	private LineEdit LineLabelEditor;
	[Export]
	private HSlider SliderHue;

	[Signal]
	public delegate void BlockModifiedEventHandler();

	public void InitPanel(GraphMetaBucketCommon meta, string id)
	{
		Id = id;
		GraphMeta = meta;
		BlockMeta = GraphMeta.GetBlockMetadata(id);

		Name = id;

		GlobalPosition = BlockMeta.Position;
		Size = BlockMeta.Size;

		LineLabelEditor.Text = BlockMeta.Label;
		LabelHeader.Text = BlockMeta.Label;
		if (EngineSettings.GetSetting<bool>("moonflow/event_graph/hide_block_tooltip", false))
			LabelHeader.TooltipText = null;

		OnColorHueChanged(BlockMeta.Hue);
	}

	#region Signals

	public void OnBlockMovedOrResized()
	{
		if (BlockMeta == null)
			return;

		// Clamp onto grid
		const float gridSize = 32f;

		GlobalPosition = (GlobalPosition / gridSize).Floor() * gridSize;
		Size = (Size / gridSize).Floor() * gridSize;

		// Write to metadata and alert graph
		BlockMeta.Position = GlobalPosition;
		BlockMeta.Size = Size;
		EmitSignal(SignalName.BlockModified);
	}

	public void OnColorHueChanged(float hue)
	{
		if (BlockMeta == null)
			return;

		// Update modulation
		var col = SelfModulate;
		col.H = hue;
		SelfModulate = col;

		// Update slider if needed
		if (SliderHue.Value != hue)
			SliderHue.Value = hue;

		// Update metadata
		BlockMeta.Hue = hue;
		EmitSignal(SignalName.BlockModified);
	}

	private void OnLineEditLabelChanged(string txt)
	{
		if (BlockMeta == null)
			return;

		LabelHeader.Text = txt;
		BlockMeta.Label = txt;

		EmitSignal(SignalName.BlockModified);
	}

	private void OnEditMenuOpened()
	{
		LineLabelEditor.CaretColumn = LineLabelEditor.Text.Length;
		EngineSettings.SetSetting("moonflow/event_graph/hide_block_tooltip", true);
	}

	private void OnBlockDeleteRequested()
	{
		GraphMeta.Blocks.Remove(Id);

		EmitSignal(SignalName.BlockModified);
		QueueFree();
	}

	#endregion
}
