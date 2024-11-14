using Godot;
using Godot.Collections;
using System;

using Nindot.LMS.Msbp;

namespace Nindot.LMS.Msbt;

[GlobalClass, Tool]
public partial class MsbtEntryEditor : RichTextLabel
{
	MsbpFile Project = null;
	MsbtFile File = null;
	MsbtEntry Entry = null;

	public override void _Ready()
	{
		// Setup default properties
		BbcodeEnabled = false;
		FitContent = true;
		ScrollFollowing = true;
		AutowrapMode = TextServer.AutowrapMode.WordSmart;
		ContextMenuEnabled = true;
		SelectionEnabled = true;
		Threaded = true;
	}

	public MsbtEntryEditor Setup(MsbpFile project, MsbtFile file, MsbtEntry entry)
	{
		Project = project;
		File = file;
		Entry = entry;
		return this;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _GuiInput(InputEvent @event)
	{
		return;
	}

	// ====================================================== //
	// ============== Godot Property Validition ============= //
	// ====================================================== //

	private static readonly string[] ReadOnlyProperties = [
		"bbcode_enabled",
		"fit_content",
		"scroll_following",
		"autowrap_mode",
		"context_menu_enabled",
		"selection_enabled",
		"threaded",
	];

	public override void _ValidateProperty(Dictionary property)
	{
		foreach (var item in ReadOnlyProperties)
		{
			if (property["name"].AsString() == item)
				property["usage"] = (int)(PropertyUsageFlags.Default | PropertyUsageFlags.ReadOnly);
		}

		base._ValidateProperty(property);
	}
}
