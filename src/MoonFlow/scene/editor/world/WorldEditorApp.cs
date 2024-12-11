using Godot;
using System;

namespace MoonFlow.Scene.EditorWorld;

[ScenePath("res://scene/editor/world/world_editor_app.tscn"), Icon("res://asset/app/icon/world.png")]
public partial class WorldEditorApp : AppScene
{
	public override string GetUniqueIdentifier(string input)
	{
		return "WORLD_" + input;
	}
}
