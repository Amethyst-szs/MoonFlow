using Godot;
using System;

namespace MoonFlow.Scene.EditorEvent;

[ScenePath("res://scene/editor/event/meta/editor/popup_event_metadata.tscn")]
public partial class PopupEventMetadata : Popup
{
	private EventFlowNodeCommon Target;

	[Export, ExportGroup("Internal References")]
	private TextEdit TextComment;

	[Export]
	private ColorPicker ColorPicker;
	[Export]
	private Button ButtonColorSet;
	[Export]
	private Button ButtonColorReset;

	[Export]
	private HFlowContainer FlowTags;

	private static readonly Texture2D TagIcon = GD.Load<Texture2D>("res://asset/material/window/close.svg");

	public void SetupPopup(EventFlowNodeCommon target)
	{
		Target = target;
		var meta = target.Metadata;
		
		TextComment.Text = meta.Comment;
		TextComment.CallDeferred(TextEdit.MethodName.GrabFocus);
		TextComment.CallDeferred(TextEdit.MethodName.SetCaretColumn, TextComment.Text.Length);

		ButtonColorSet.SelfModulate = meta.OverrideColor;
		ButtonColorReset.Visible = meta.IsOverrideColor;

		foreach (var child in FlowTags.GetChildren())
			child.QueueFree();
		foreach (var tag in meta.Tags)
			CreateTagButton(tag);

		PopupCentered();
	}

	private void CreateTagButton(string tag)
	{
		var button = new Button()
		{
			Text = tag,
			Icon = TagIcon,
			IconAlignment = HorizontalAlignment.Right
		};

		button.Connect(Button.SignalName.Pressed, Callable.From(() => OnButtonTagRemove(button, tag)));

		FlowTags.AddChild(button);
	}

	#region Signals

	private void OnTextCommentModified()
	{
		if (Target == null) return;

		Target.SetEditorComment(TextComment.Text);
	}

	private void OnColorPickerRequested()
	{
		if (Target == null) return;

		Target.Metadata.IsOverrideColor = true;
		ColorPicker.Color = Target.Metadata.OverrideColor;

		ButtonColorSet.SelfModulate = Target.Metadata.OverrideColor;
		ButtonColorReset.Show();
	}
	private void OnColorPickerValueChanged(Color c)
	{
		if (Target == null) return;

		Target.Metadata.OverrideColor = c;
		Target.SetNodeColor();
		
		ButtonColorSet.SelfModulate = c;
	}
	private void OnColorPickerResetColor()
	{
		if (Target == null) return;

		Target.Metadata.IsOverrideColor = false;
		Target.Metadata.OverrideColor = Colors.White;

		Target.SetNodeColor();

		ButtonColorSet.SelfModulate = Colors.White;
		ButtonColorReset.Hide();
	}

	private void OnLineTagSubmitted(string tag)
	{
		if (Target == null || Target.Metadata.Tags.Contains(tag))
			return;
		
		Target.Metadata.Tags.Add(tag);
		CreateTagButton(tag);
	}
	private void OnButtonTagRemove(Button source, string tag)
	{
		if (Target == null || !Target.Metadata.Tags.Contains(tag))
			return;
		
		Target.Metadata.Tags.Remove(tag);
		source.QueueFree();
	}

	#endregion
}
