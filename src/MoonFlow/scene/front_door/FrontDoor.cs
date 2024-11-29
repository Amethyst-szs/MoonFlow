using Godot;

using MoonFlow.Project;
using MoonFlow.Scene.Settings;

namespace MoonFlow.Scene;

[Icon("res://asset/app/icon/front_door.png")]
[ScenePath("res://scene/front_door/front_door.tscn")]
public partial class FrontDoor : AppScene
{
    protected override void AppInit()
    {
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
