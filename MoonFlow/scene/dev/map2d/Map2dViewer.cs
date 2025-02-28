using Godot;
using System;

using MoonFlow.Project;
using MoonFlow.Project.Database;
using System.Numerics;

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

	private static readonly Texture2D MapIcon = GD.Load<Texture2D>("res://asset/nindot/lms/icon/PictureFont_70.png");

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

		foreach (var child in TextureMap.GetChildren())
			child.QueueFree();

		foreach (var shine in world.ShineList)
		{
			var t = shine.Trans;
			var m = map.CalcMapTrans(t, new System.Numerics.Vector2(TextureMap.Size.X, TextureMap.Size.Y));

            var ico = new TextureRect
            {
                Texture = MapIcon,
				ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
				CustomMinimumSize = new Godot.Vector2(16, 16),
				TooltipText = shine.LookupDisplayName(ProjectManager.GetMSBTArchives()?.StageMessage)?.GetRawText() + "\n" + t.ToString() + "\n" + m.ToString(),
            };

            TextureMap.AddChild(ico);
			ico.Position = new Godot.Vector2(m.X, m.Y);
		}

		// var result = map.CalcMapTrans(new System.Numerics.Vector3(5710.0f, 5124.0f, -2510.0f));
		// GD.Print(result);
	}

	private void OnScenarioPicked(int scenario)
	{
		Scenario = scenario;

		if (World == null)
			return;
		
		OnWorldPicked(World);
	}
}
