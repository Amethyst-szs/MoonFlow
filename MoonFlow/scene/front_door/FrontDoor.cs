using System;
using Godot;

using static Nindot.RomfsPathUtility;

using MoonFlow.Project;
using MoonFlow.Scene.Settings;

namespace MoonFlow.Scene;

[Icon("res://asset/app/icon/front_door.png")]
[ScenePath("res://scene/front_door/front_door.tscn")]
public partial class FrontDoor : AppScene
{
	private ProjectInitInfo InitInfo = new();

	private Label OpenProjectError = null;
	private OptionButton NewProjectVersionButton = null;
	private LangPicker NewProjectLangButton = null;
	private Button NewProjectCreateButton = null;

	[Signal]
	public delegate void OpenProjectEventHandler(string path);
	[Signal]
	public delegate void OpenProjectFailedEventHandler(string path);
	[Signal]
	public delegate void NewProjectPathEventHandler(string path);
	[Signal]
	public delegate void NewProjectInvalidPathEventHandler();

	protected override void AppInit()
	{
		// Obtain references to nodes in scene
		OpenProjectError = GetNode<Label>("%Label_OpenError");
		OpenProjectError.Hide();

		NewProjectVersionButton = GetNode<OptionButton>("%Option_Version");

		NewProjectLangButton = GetNode<LangPicker>("%Option_DefaultLang");
		NewProjectLangButton.SetSelection("USen");
		NewProjectLangButton.SetGameVersion(RomfsVersion.INVALID_VERSION);

		NewProjectCreateButton = GetNode<Button>("%Button_Create");
		OnUpdateCreateProjectButtonState();

		// Check if user needs to provide a RomFS path
		if (!RomfsAccessor.IsValid())
			ForceOpenRomfsAccessorApp();
	}

	private void OpenEngineSettingsApp()
	{
		var app = SceneCreator<EngineSettingsApp>.Create();
		app.SetUniqueIdentifier();
		Scene.NodeApps.AddChild(app);
	}

	private void ForceOpenRomfsAccessorApp()
	{
		var app = SceneCreator<RomfsAccessorConfigApp>.Create();
		Scene.NodeApps.AddChild(app);
	}

	private void OpenContributorPage()
	{
		var app = SceneCreator<FrontDoorContributorApp>.Create();
		app.SetUniqueIdentifier();
		Scene.NodeApps.AddChild(app);
	}

	// ====================================================== //
	// ==================== Signal Events =================== //
	// ====================================================== //

	private void OnDialogNewProjectPathSelected(string path)
	{
		if (!ProjectManager.IsValidOpenOrCreate(ref path))
		{
			EmitSignal(SignalName.NewProjectInvalidPath);
			return;
		}

		if (ProjectManager.IsProjectConfigExist(ref path, out _, false))
		{
			EmitSignal(SignalName.NewProjectInvalidPath);
			return;
		}

		InitInfo.Path = path;
		EmitSignal(SignalName.NewProjectPath, path);

		OnUpdateCreateProjectButtonState();

		GD.Print("Set InitInfo.Path to " + path);
	}

	private void OnDialogOpenProjectPathSelected(string path)
	{
		var result = ProjectManager.TryOpenProject(path, out RomfsVersion version);

		if (result == ProjectManager.ProjectManagerResult.OK)
		{
			EmitSignal(SignalName.OpenProject, path);
			return;
		}

		OpenProjectError.Show();
		OpenProjectError.Call("set_label", [Enum.GetName(result), Enum.GetName(version)]);

		EmitSignal(SignalName.OpenProjectFailed, path);
	}

	private void OnNewProjectVersionButtonPressed()
	{
		foreach (var ver in Enum.GetValues<RomfsVersion>())
		{
			if (ver == RomfsVersion.INVALID_VERSION)
				continue;

			bool isHaveValidPath = RomfsAccessor.IsHaveVersion(ver);

			var idx = NewProjectVersionButton.GetItemIndex((int)ver);
			NewProjectVersionButton.SetItemDisabled(idx, !isHaveValidPath);
		}
	}

	private void OnNewProjectVersionSelected(int id)
	{
		if (!Enum.IsDefined((RomfsVersion)id))
		{
			GD.Print("Set InitInfo.Version to INVALID_VERSION");
			InitInfo.Version = RomfsVersion.INVALID_VERSION;
			NewProjectLangButton.SetGameVersion(InitInfo.Version);

			OnUpdateCreateProjectButtonState();
			return;
		}

		var select = (RomfsVersion)id;
		InitInfo.Version = select;
		NewProjectLangButton.SetGameVersion(InitInfo.Version);

		OnUpdateCreateProjectButtonState();

		GD.Print("Set InitInfo.Version to " + Enum.GetName(InitInfo.Version));
	}

	private void OnNewProjectDefaultLangSelected(string lang, int idx)
	{
		InitInfo.DefaultLanguage = lang;
		GD.Print("Set InitInfo.DefaultLanguage to " + lang);
	}

	private void OnUpdateCreateProjectButtonState()
	{
		bool isOk = InitInfo.Path != null && InitInfo.Path != string.Empty;
		isOk &= InitInfo.Version != RomfsVersion.INVALID_VERSION;

		NewProjectCreateButton.Disabled = !isOk;
	}

	private void OnNewProjectCreate()
	{
		var res = ProjectManager.TryCreateProject(InitInfo);
		if (res != ProjectManager.ProjectManagerResult.OK)
			GD.PushError("Failed to create project!");

		OnDialogOpenProjectPathSelected(InitInfo.Path);
	}
}
