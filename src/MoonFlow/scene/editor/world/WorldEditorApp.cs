using Godot;
using Godot.Collections;
using System;

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

	private WorldInfo World = null;

	private bool IsWorldInfoModified = false;
	private bool IsShineListModified = false;
	private bool IsItemInfoModified = false;

	public void OpenWorld(WorldInfo world)
	{
		// Setup basic app info
		World = world;
		AppTaskbarTitle = world.Display;

		// Initilize info boxes
		foreach (var box in InfoBoxList)
		{
			box.OpenWorld(world);

			box.Connect(InfoBoxBase.SignalName.ModifiedWorldInfo, Callable.From(OnWorldInfoModify));
			box.Connect(InfoBoxBase.SignalName.ModifiedShineList, Callable.From(OnShineListModify));
			box.Connect(InfoBoxBase.SignalName.ModifiedItemInfo, Callable.From(OnItemInfoModify));
		}

		// Setup signals with header
		var header = ProjectManager.SceneRoot.NodeHeader;
		header.Connect(Header.SignalName.ButtonSave, Callable.From(SaveFile));
	}

	public override string GetUniqueIdentifier(string input)
	{
		return "WORLD_" + input;
	}

	#region Saving
	
	public async void SaveFile()
	{
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

	private void OnModify()
	{
		IsModified = true;
	}

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
}
