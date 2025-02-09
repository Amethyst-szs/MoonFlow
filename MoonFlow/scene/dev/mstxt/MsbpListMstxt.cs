using Godot;
using MoonFlow.Project;

namespace MoonFlow.Scene.Dev;

public partial class MsbpListMstxt : VBoxContainer
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

		foreach (var item in proj.Project_GetContent())
		{
			AddChild(new Label()
			{
				Text = item,
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				HorizontalAlignment = HorizontalAlignment.Left,
			});
		}
	}
}
