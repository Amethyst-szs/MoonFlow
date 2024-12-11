using Godot;
using MoonFlow.Project;
using System;

namespace MoonFlow.Scene.Home;

public partial class TabWorld : Control
{
	// private void InitContent()
	// {
	// 	// Delete old content
	// 	foreach (var child in GetChildren()) child.QueueFree();

	// 	// Iterate through world database
	// 	var db = ProjectManager.GetProject().Database;
	// 	foreach (var world in db.WorldList)
	// 	{
	// 		var button = new Button()
	// 		{
	// 			Alignment = HorizontalAlignment.Center,
	// 			SizeFlagsHorizontal = SizeFlags.Expand,
	// 			Text = world.Display,
	// 		};

	// 		AddChild(button);
	// 	}
	// }

	// private void OnVisiblityChanged()
	// {
	// 	if (!Visible)
	// 		return;
		
	// 	InitContent();
	// }
}
