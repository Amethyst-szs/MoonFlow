using Godot;

using MoonFlow.Fs.Rom;
using MoonFlow.Scene.Settings;

namespace MoonFlow.Scene;

[Icon("res://asset/nindot/lms/icon/TextAlign_Center.png")]
[ScenePath("res://scene/front_door/front_door.tscn")]
public partial class FrontDoor : AppScene
{
	public override void _Ready()
	{
		base._Ready();

		// Check if user needs to provide a RomFS path
		if (!RomfsAccessor.IsValid())
			ForceOpenRomfsAccessorApp();
	}

	private void ForceOpenRomfsAccessorApp()
	{
		var app = SceneCreator<RomfsAccessorConfigApp>.Create();
		Scene.NodeApps.AddChild(app);
	}
}
