using System.IO;
using Godot;
using MoonFlow.Project;

using MoonFlow.Project.Database;

namespace MoonFlow.Scene.EditorWorld;

[SceneUid("uid://dnvwsx8eyi8sb")]
public partial class EditStageInfo : PanelContainer
{
	private WorldInfo World = null;
	private StageInfo Stage = null;

	[Signal]
	public delegate void RefreshListEventHandler();

	public void Setup(WorldInfo world, StageInfo stage)
	{
		World = world;
		Stage = stage;

		GetNode<Label>("%Label_Name").Text = stage.name;
		GetNode<OptionButton>("%Option_Type").Selected = (int)stage.CategoryType;
	}

	private void OnCopyToProjectRequest()
	{
		// Get list of directories
		string projPath = ProjectManager.GetPath();
		string localPath = "StageData/" + Stage.name;

		string localPathMap = localPath + "Map.szs";
		string localPathDesign = localPath + "Design.szs";
		string localPathSound = localPath + "Sound.szs";

		string romPath = RomfsAccessor.ActiveDirectory;

		// Ensure project StageData path
		Directory.CreateDirectory(projPath + "StageData/");

		// Attempt to copy each file
		TryCopyFileToProject(projPath + localPathMap, romPath + localPathMap);
		TryCopyFileToProject(projPath + localPathDesign, romPath + localPathDesign);
		TryCopyFileToProject(projPath + localPathSound, romPath + localPathSound);
	}
	private static void TryCopyFileToProject(string fullPathProj, string fullPathRom)
	{
		// Skip if file is already in the project
		if (File.Exists(fullPathProj))
			return;

		// Skip if the file doesn't exist in the romfs
		if (!File.Exists(fullPathRom))
			return;

		File.Copy(fullPathRom, fullPathProj);
	}

	private void OnDeleteRequest()
	{
		World.StageList.Remove(Stage);
		EmitSignal(SignalName.RefreshList);
	}

	private void OnTypeChanged(int id)
	{
		Stage.CategoryType = (StageInfo.CatEnum)id;
		ProjectDatabaseHolder.SortWorldStagesByType(World.StageList);

		EmitSignal(SignalName.RefreshList);
	}
}
