using Godot;

using MoonFlow.Project;
using MoonFlow.Scene.Settings;
using Nindot.Al.SMO;

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

	private void OnNewProjectPressedForDebug()
	{
        var initInfo = new ProjectInitInfo
        {
            Path = "C:/Users/evils/AppData/Roaming/Godot/app_userdata/MoonFlow/debug/",
            Version = RomfsValidation.RomfsVersion.v100,
			DefaultLanguage = "USen",
        };

        var res = ProjectManager.TryCreateProject(initInfo);
	}

	private void OnOpenProjectPressedForDebug()
	{
		var res = ProjectManager.TryOpenProject("C:/Users/evils/AppData/Roaming/Godot/app_userdata/MoonFlow/debug/");
	}
}
