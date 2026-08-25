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

[SceneUid("uid://7jgd1gt2u83b"), Icon("res://asset/app/icon/world.png")]
public partial class WorldEditorApp : AppScene
{
	[Export, ExportGroup("Internal References"), ExportSubgroup("Header")]
	private Label LabelHeaderWorldName;
	[Export]
	private Label LabelHeaderInternalName;

	[Export, ExportGroup("Internal References"), ExportSubgroup("Tabs")]
	private Array<InfoBoxBase> InfoBoxList = [];
	[Export]
	private TabMap TabMap;
	[Export]
	private WorldEditorTabStages TabStageList;
	[Export]
	private VBoxContainer VBoxShineList;

	public WorldInfo World { get; private set; }
	private bool IsRunningInit = false;

	private bool IsWorldInfoModified = false;
	private bool IsShineListModified = false;
	private bool IsItemInfoModified = false;
	private bool IsMapInfoModified = false;

	public void OpenWorld(WorldInfo world)
	{
		// Setup basic app info
		IsRunningInit = true;
		World = world;
		AppTaskbarTitle = world.Display;

		// Setup header labels
		LabelHeaderWorldName.Text = world.Display;
		LabelHeaderInternalName.Text = world.WorldName;

		// Initilize info boxes
		foreach (var box in InfoBoxList)
		{
			box.OpenWorld(world);

			box.Connect(InfoBoxBase.SignalName.ModifiedWorldInfo, Callable.From(OnWorldInfoModify));
			box.Connect(InfoBoxBase.SignalName.ModifiedShineList, Callable.From(OnShineListModify));
			box.Connect(InfoBoxBase.SignalName.ModifiedItemInfo, Callable.From(OnItemInfoModify));
		}

		TabStageList.Init(world);
		SetupShineList();
        _ = TabMap.InitMap();

		// Setup signals with header
		var header = ProjectManager.SceneRoot.NodeHeader;
		header.Connect(Header.SignalName.ButtonSave, Callable.From(new Action<bool>(SaveFileInternal)));

		IsRunningInit = false;
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

		var callHover = Callable.From(new Action<WorldShineEditorHolder>(TabMap.OnShineHovered));
		var callUnhover = Callable.From(TabMap.OnShineUnhovered);

		scene.Connect(WorldShineEditorHolder.SignalName.ShineHovered, callHover);
		scene.Connect(WorldShineEditorHolder.SignalName.ShineUnhovered, callUnhover);
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
		int progressTask = -1;
		int totalTasks =
			(IsWorldInfoModified ? 1 : 0) +
			(IsShineListModified ? 1 : 0) +
			(IsItemInfoModified ? 1 : 0) +
			(IsMapInfoModified ? 1 : 0);

		IncrementTaskProgress(display, ref progressTask, totalTasks);

		// Access project DB
		var db = ProjectManager.GetProject().Database;

		// Write each file in DB if needed
		if (IsWorldInfoModified)
		{
			db.WriteWorldList();
			IncrementTaskProgress(display, ref progressTask, totalTasks);
		}

		if (IsShineListModified)
		{
			db.WriteShineInfo(World.WorldName);
			IncrementTaskProgress(display, ref progressTask, totalTasks);
		}

		if (IsItemInfoModified)
		{
			db.WriteWorldItemList();
			IncrementTaskProgress(display, ref progressTask, totalTasks);
		}

		if (IsMapInfoModified)
		{
			db.TrySaveMap2dDatabaseAndTextures(World);
			IncrementTaskProgress(display, ref progressTask, totalTasks);
		}

		// Reset flags
		IsModified = false;
		IsWorldInfoModified = false;
		IsShineListModified = false;
		IsItemInfoModified = false;
		IsMapInfoModified = false;
	}

	private static void IncrementTaskProgress(AsyncDisplay display, ref int progress, int max)
	{
		progress++;
		display.UpdateProgress(progress, max);
	}

	#endregion

	#region Signals

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
			ObjectName = "シャイン", // Translates to Shine
			ObjId = "obj0",
			OptionalId = null,

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
	public void OnWorldInfoModify()
	{
		IsWorldInfoModified = true;
		OnModify();
	}
	public void OnShineListModify()
	{
		IsShineListModified = true;
		OnModify();
	}
	public void OnItemInfoModify()
	{
		IsItemInfoModified = true;
		OnModify();
	}
	public void OnMapInfoModify()
	{
		IsMapInfoModified = true;
		OnModify();
	}

	#endregion
}
