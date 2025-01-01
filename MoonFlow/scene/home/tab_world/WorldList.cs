using Godot;
using System;

using MoonFlow.Project;
using MoonFlow.Project.Database;
using MoonFlow.Scene.EditorWorld;

namespace MoonFlow.Scene.Home;

public partial class WorldList : HBoxContainer
{
	[Export]
	private PackedScene ButtonResource;

	public override void _Ready()
	{
		if (!IsInstanceValid(ButtonResource))
			return;

		// Delete old children
		foreach (var child in GetChildren())
		{
			RemoveChild(child);
			child.QueueFree();
		}

		var db = ProjectManager.GetDB()
		?? throw new NullReferenceException("No reference to project database!");

        foreach (var world in db.WorldList)
		{
			var button = ButtonResource.Instantiate();
			AddChild(button);

			// Setup button text
			if (!button.HasMethod("init_button"))
				throw new Exception();

			button.Call("init_button", world.Display);

			// Connect to button press event
			button.Connect(Button.SignalName.Pressed,
				Callable.From(() => OnItemPressed(world.WorldName)));
		}

		// Setup focus neighbors
		var children = GetChildren();
		for (int i = 0; i < children.Count; i++)
		{
			var child = children[i];

			var prev = children[ModN(i - 1, children.Count)];
			var next = children[(i + 1) % children.Count];

			if (!child.HasMethod("assign_neighbors"))
				continue;

			child.Call("assign_neighbors", prev, next);
		}

		// Append padding on right side
		AddChild(new VSeparator());
	}

	private static void OnItemPressed(string worldName)
	{
		var world = (ProjectManager.GetDB()?.GetWorldByName(worldName))
		?? throw new Exception("Could not get reference to world!");
		
        var editor = SceneCreator<WorldEditorApp>.Create();
		editor.SetUniqueIdentifier(world.WorldName);
		ProjectManager.SceneRoot.NodeApps.AddChild(editor);

		editor.OpenWorld(world);
	}

	private static int ModN(int x, int m)
	{
		return (x % m + m) % m;
	}
}
