using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Threading.Tasks;

using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;

using MoonFlow.Project.Database;
using MoonFlow.Project;
using MoonFlow.Scene.Main;
using MoonFlow.Async;

namespace MoonFlow.Scene.EditorWorld;

[ScenePath("res://scene/editor/world/world_editor_app.tscn"), Icon("res://asset/app/icon/world.png")]
public partial class WorldEditorApp : AppScene
{
	[Export]
	private Array<InfoBoxBase> InfoBoxList = [];
	[Export]
	private VBoxContainer VBoxStageList;
	[Export]
	private VBoxContainer VBoxShineList;
	[Export]
	private Label LabelNewStageError;

	private WorldInfo World;
	private bool IsRunningInit = false;

	private string NewStageName = "";
	private StageInfo.CatEnum NewStageCategory = StageInfo.CatEnum.ExStage;

	private bool IsWorldInfoModified = false;
	private bool IsShineListModified = false;
	private bool IsItemInfoModified = false;

	private static readonly PackedScene StageInfoScene = GD.Load<PackedScene>(
		"res://scene/editor/world/stage/edit_stage_info.tscn"
	);

	public void OpenWorld(WorldInfo world)
	{
		// Setup basic app info
		IsRunningInit = true;

		World = world;
		AppTaskbarTitle = world.Display;

		LabelNewStageError.Hide();

		// Initilize info boxes
		foreach (var box in InfoBoxList)
		{
			box.OpenWorld(world);

			box.Connect(InfoBoxBase.SignalName.ModifiedWorldInfo, Callable.From(OnWorldInfoModify));
			box.Connect(InfoBoxBase.SignalName.ModifiedShineList, Callable.From(OnShineListModify));
			box.Connect(InfoBoxBase.SignalName.ModifiedItemInfo, Callable.From(OnItemInfoModify));
		}

		SetupStageList();
		SetupShineList();

		GetNode<OptionButton>("%Option_Type").Selected = (int)NewStageCategory;

		// Setup signals with header
		var header = ProjectManager.SceneRoot.NodeHeader;
		header.Connect(Header.SignalName.ButtonSave, Callable.From(new Action<bool>(SaveFileInternal)));

		IsRunningInit = false;
	}

	private void SetupStageList()
	{
		foreach (var child in VBoxStageList.GetChildren())
		{
			VBoxStageList.RemoveChild(child);
			child.QueueFree();
		}

		var prevCategory = StageInfo.CatEnum.Unknown;
		foreach (var stage in World.StageList)
		{
			if (stage.CategoryType != prevCategory)
			{
				prevCategory = stage.CategoryType;
				VBoxStageList.AddChild(new HSeparator());
			}

			var scene = StageInfoScene.Instantiate<EditStageInfo>();

			scene.Connect(EditStageInfo.SignalName.RefreshList, Callable.From(() =>
			{
				OnWorldInfoModify();
				SetupStageList();
			}));

			VBoxStageList.AddChild(scene);
			scene.Setup(World, stage);
		}
	}

	private void SetupShineList()
	{
		foreach (var child in VBoxShineList.GetChildren())
		{
			VBoxShineList.RemoveChild(child);
			child.QueueFree();
		}

		foreach (var shine in World.ShineList)
		{
			MsbtEntry shineDisplay = shine.LookupDisplayName();

			var scene = SceneCreator<WorldShineEditorHolder>.Create();
			VBoxShineList.AddChild(scene);

			scene.SetupShineEditor(World, shine, shineDisplay, World.ShineList.IndexOf(shine));
			scene.Connect(WorldShineEditorHolder.SignalName.ContentModified, Callable.From(OnShineListModify));
		}
	}

	public override string GetUniqueIdentifier(string input)
	{
		return "WORLD_" + input;
	}

	#region Saving

	private async void SaveFileInternal(bool isRequireFocus) { await SaveFile(isRequireFocus); }
	public override async Task SaveFile(bool isRequireFocus)
	{
		if (!AppIsFocused() && isRequireFocus)
			return;

		GD.Print("\n - Saving ", World.WorldName);

		var run = AsyncRunner.Run(TaskRunWriteFile, AsyncDisplay.Type.SaveWorldArchives);

		await run.Task;
		await ToSignal(Engine.GetMainLoop(), "process_frame");

		if (run.Task.Exception == null)
			GD.Print("Saved ", World.WorldName);
		else
			GD.Print("Saving failed for ", World.WorldName);
	}

	public void TaskRunWriteFile(AsyncDisplay display)
	{
		// Calculate total tasks
		int totalTasks =
			(IsWorldInfoModified ? 1 : 0) +
			(IsShineListModified ? 1 : 0) +
			(IsItemInfoModified ? 1 : 0);

		display.UpdateProgress(0, totalTasks);

		// Access project DB
		var db = ProjectManager.GetProject().Database;

		// Write each file in DB if needed
		if (IsWorldInfoModified)
			db.WriteWorldList();

		if (IsShineListModified)
			db.WriteShineInfo(World.WorldName);

		if (IsItemInfoModified)
			db.WriteWorldItemList();

		// Reset flags
		IsModified = false;
		IsWorldInfoModified = false;
		IsShineListModified = false;
		IsItemInfoModified = false;
	}

	#endregion

	#region Signals

	private void OnNewStageNameChanged(string str)
	{
		NewStageName = str;

		bool isValid = IsNewStageNameValid(out string errorSource);
		LabelNewStageError.Visible = !isValid && errorSource != "empty";

		if (isValid || str == string.Empty)
			return;
		
		LabelNewStageError.Text = Tr("WORLD_EDITOR_INVALID_NEW_STAGE_NAME_ERROR") + " " + errorSource;
	}

	private void OnNewStageCategoryChanged(int id)
	{
		NewStageCategory = (StageInfo.CatEnum)id;
	}

	private void OnNewStageSubmitted()
	{
		if (!IsNewStageNameValid(out _))
			return;

        // Create new StageInfo
        var info = new StageInfo
        {
            name = NewStageName,
			CategoryType = NewStageCategory,
        };

		World.StageList.Add(info);
		ProjectDatabaseHolder.SortWorldStagesByType(World.StageList);

		// Reload scene
		OnWorldInfoModify();
		SetupStageList();
    }

	private void OnShineListChildOrderChanged()
	{
		if (IsRunningInit)
			return;
		
		foreach (var child in VBoxShineList.GetChildren())
		{
			if (child is not WorldShineEditorHolder editor)
				continue;
			
			editor.UpdateShineIndex();
		}
	}

	private void OnModify() { IsModified = true; }
	private void OnWorldInfoModify()
	{
		IsWorldInfoModified = true;
		OnModify();
	}
	private void OnShineListModify()
	{
		IsShineListModified = true;
		OnModify();
	}
	private void OnItemInfoModify()
	{
		IsItemInfoModified = true;
		OnModify();
	}

	#endregion

	#region Utilities

	private bool IsNewStageNameValid(out string errorSource)
	{
		if (NewStageName == string.Empty)
		{
			errorSource = "empty";
			return false;
		}
		
		// Check if this world already has this name
		if (World.StageList.Any((s) => s.name == NewStageName))
		{
			errorSource = World.Display;
			return false;
		}

		// Check if any world already has this stage name
		foreach (var world in ProjectManager.GetDB().WorldList)
		{
			if (world.StageList.Any((s) => s.name == NewStageName))
			{
				errorSource = world.Display;
				return false;
			}
		}

		errorSource = "";
		return true;
	}

	#endregion
}
