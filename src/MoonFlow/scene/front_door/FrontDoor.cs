using System;
using Godot;

using Nindot.Al.SMO;

using MoonFlow.Project;
using MoonFlow.Scene.Settings;
using MoonFlow.Scene.EditorEvent;

namespace MoonFlow.Scene;

[Icon("res://asset/app/icon/front_door.png")]
[ScenePath("res://scene/front_door/front_door.tscn")]
public partial class FrontDoor : AppScene
{
	private Label NewProjectError = null;
	private Label OpenProjectError = null;

	[Signal]
	public delegate void OpenProjectEventHandler(string path);

	protected override void AppInit()
	{
		// Obtain references to nodes in scene
		NewProjectError = GetNode<Label>("%Label_NewError");
		NewProjectError.Hide();
		OpenProjectError = GetNode<Label>("%Label_OpenError");
		OpenProjectError.Hide();

		// Check if user needs to provide a RomFS path
		if (!RomfsAccessor.IsValid())
			ForceOpenRomfsAccessorApp();
	}

	private void ForceOpenRomfsAccessorApp()
	{
		var app = SceneCreator<RomfsAccessorConfigApp>.Create();
		Scene.NodeApps.AddChild(app);
	}

	private void OpenContributorPage()
	{
		var app = SceneCreator<FrontDoorContributorApp>.Create();
		Scene.NodeApps.AddChild(app);
	}

	// ====================================================== //
	// ==================== Signal Events =================== //
	// ====================================================== //

	private void OnDialogNewProjectPathSelected(string path)
	{
		var initInfo = new ProjectInitInfo()
		{
			Path = path,
			DefaultLanguage = "USen",
			Version = RomfsValidation.RomfsVersion.v100
		};

		var result = ProjectManager.TryCreateProject(initInfo);

		if (result == ProjectManager.ProjectManagerResult.OK)
			return;

		NewProjectError.Show();
		NewProjectError.Call("set_label", [Enum.GetName(result), Enum.GetName(initInfo.Version)]);
	}

	private void OnDialogOpenProjectPathSelected(string path)
	{
		var result = ProjectManager.TryOpenProject(path, out RomfsValidation.RomfsVersion version);

		if (result == ProjectManager.ProjectManagerResult.OK)
		{
			EmitSignal(SignalName.OpenProject, path);
			return;
		}

		OpenProjectError.Show();
		OpenProjectError.Call("set_label", [Enum.GetName(result), Enum.GetName(version)]);
	}
}
