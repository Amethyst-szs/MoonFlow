using Godot;
using System;

using MoonFlow.Project;
using MoonFlow.Project.Database;

namespace MoonFlow.Scene.Dev;

[ScenePath("res://scene/dev/map2d/map2d_viewer.tscn")]
public partial class Map2dViewer : AppScene
{
	[Export]
	private VBoxContainer ContainerMapList;
	[Export]
	private TextureRect TextureMap;

	private WorldInfo World;
	private int Scenario = -1;

	public override void _Ready()
	{
		// Generate world list
		foreach (var world in ProjectManager.GetDB().WorldList)
		{
			var button = new Button
			{
				Text = world.Name
			};

			ContainerMapList.AddChild(button);

			button.Connect(Button.SignalName.Pressed, Callable.From(() => OnWorldPicked(world)));
		}
	}

	private void OnWorldPicked(WorldInfo world)
	{
		World = world;
		
		var map = world.MapInfo.GetMap(Scenario);
		TextureMap.Texture = ImageTexture.CreateFromImage(map.Texture);
	}

	private void OnScenarioPicked(int scenario)
	{
		Scenario = scenario;

		if (World == null)
			return;
		
		OnWorldPicked(World);
	}
}
