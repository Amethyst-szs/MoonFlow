using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Threading.Tasks;

using Nindot;
using Nindot.LMS.Msbt;

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
		VBoxStageList.QueueFreeAllChildren();

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
		VBoxShineList.QueueFreeAllChildren();

		var stageMessage = ProjectManager.GetMSBTArchives()?.StageMessage
		?? throw new NullReferenceException("Could not get StageMessage!");

		foreach (var shine in World.ShineList)
			SetupShineEditorContainer(shine, stageMessage);
	}

	private void SetupShineEditorContainer(ShineInfo shine, SarcFile stageMessage)
	{
		MsbtEntry shineDisplay = shine.LookupDisplayName(stageMessage);

		var scene = SceneCreator<WorldShineEditorHolder>.Create();
		VBoxShineList.AddChild(scene);

		scene.SetupShineEditor(World, shine, shineDisplay, World.ShineList.IndexOf(shine));
		scene.Connect(WorldShineEditorHolder.SignalName.ContentModified, Callable.From(OnShineListModify));
	}

	public override string GetUniqueIdentifier(string input)
	{
		return "WORLD_" + input;
	}

	#region Saving

	private async void SaveFileInternal(bool isRequireFocus) { await AppSaveContent(isRequireFocus); }
	protected override void TaskWriteAppSaveContent(AsyncDisplay display)
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

	private void OnAddNewShine()
	{
		var db = ProjectManager.GetDB()
		?? throw new NullReferenceException("Could not get DB!");

		var stageMessage = ProjectManager.GetMSBTArchives()?.StageMessage
		?? throw new NullReferenceException("Could not get StageMessage!");

		// Create shine info
		var info = new ShineInfo()
		{
			StageName = World.Name,
			ScenarioName = "",
			ObjId = "obj0",

			MainScenarioNo = -1,
			ProgressBitFlag = 32767, // 0111-1111-1111-1111

			IsAchievement = false,
			IsGrand = false,
			IsMoonRock = false,
			Trans = System.Numerics.Vector3.Zero,
		};

		info.ReassignUID(db);
		info.ReassignHintId(World);

		// Add to world
		World.ShineList.Add(info);

		// Create new editor container
		SetupShineEditorContainer(info, stageMessage);
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
