using Godot;
using System;
using System.Linq;

using MoonFlow.Project;

namespace MoonFlow.Scene.Dev;

public partial class MsbpListTgg : VBoxContainer
{
	public override void _Ready()
	{
		VisibilityChanged += OnVisiblityChange;
		OnVisiblityChange();
	}

	private void OnVisiblityChange()
	{
		if (!Visible)
			return;

		var proj = ProjectManager.GetMSBP();

		foreach (var child in GetChildren())
			child.QueueFree();

		foreach (var group in proj.TagGroup_GetList())
		{
			CreateLabel(group.Name, 0, 24);

			foreach (var tag in proj.Tag_GetList(group))
			{
				CreateLabel(tag.Name, 1, 16);

				foreach (var param in proj.TagParam_GetList(tag))
					CreateLabel(param.Name, 2, 10);
			}
		}
	}

	private void CreateLabel(string label, int indent, int size)
	{
		var margin = new MarginContainer();
		margin.AddThemeConstantOverride("margin_left", indent * 38);
		AddChild(margin);

		margin.AddChild(new Label()
		{
			Text = label,
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			HorizontalAlignment = HorizontalAlignment.Left,
			LabelSettings = new LabelSettings()
			{
				FontSize = size
			},
		});
	}
}
