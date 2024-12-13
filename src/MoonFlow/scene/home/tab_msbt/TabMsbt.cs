using Godot;
using System;
using System.Collections.Generic;

using Nindot;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib.Smo;

using MoonFlow.Project;
using MoonFlow.Scene.EditorMsbt;
using MoonFlow.Project.Database;
using System.Linq;

namespace MoonFlow.Scene.Home;

public partial class TabMsbt : HSplitContainer
{
	private SarcMsbtFile SelectedFile = null;

	private VBoxContainer SystemMessageButtons = null;
	private VBoxContainer StageMessageVBox = null;
	private VBoxContainer LayoutMessageButtons = null;

	private GDScript DropdownButton = GD.Load<GDScript>("res://addons/ui_node_ext/dropdown_checkbox.gd");
	private GDScript DoublePressButton = GD.Load<GDScript>("res://addons/ui_node_ext/double_click_button.gd");

	public override void _Ready()
	{
		SystemMessageButtons = GetNode<VBoxContainer>("%SystemMessage_VBox");
		LayoutMessageButtons = GetNode<VBoxContainer>("%LayoutMessage_VBox");

		StageMessageVBox = GetNode<VBoxContainer>("%StageMessage_VBox");

		// Setup vbox buttons for system and layout
		var archives = ProjectManager.GetMSBTArchives();
		SetupGenericVBox(SystemMessageButtons, archives.SystemMessage);
		SetupGenericVBox(LayoutMessageButtons, archives.LayoutMessage);

		// Setup buttons for stage messages
		var db = ProjectManager.GetProject().Database;
		var stageFiles = archives.StageMessage.Content.Keys.ToList();

		foreach (var world in db.WorldList)
		{
			var content = SetupWorldVBox(world, archives.StageMessage);
			foreach (var item in content)
				stageFiles.Remove(item);
		}

		SetupWorldVBoxWithoutWorld(stageFiles, archives.StageMessage);
	}

	private void SetupGenericVBox(VBoxContainer box, SarcFile file)
	{
		string[] keys = [.. file.Content.Keys];
		Array.Sort(keys);

		foreach (var key in keys)
			CreateButton(file, key, box);
	}

	private List<string> SetupWorldVBox(WorldInfo world, SarcFile arc)
	{
		// Output stage name list
		List<string> result = [];

		// Create new container
		var worldBoxMargin = new MarginContainer();
		var worldBox = new VBoxContainer();

		// Fill container with buttons
		var prevCategory = StageInfo.CatEnum.MainStage;

		foreach (var stage in world.StageList)
		{
			var key = stage.name + ".msbt";

			// If the archive doesn't have a file for the stage, skip
			if (!arc.Content.ContainsKey(key))
				continue;

			if (stage.CategoryType != prevCategory)
			{
				prevCategory = stage.CategoryType;
				worldBox.AddChild(new HSeparator());
			}

			CreateButton(arc, key, worldBox);
			result.Add(key);
		}

		// Create dropdown button
		var dropdown = DropdownButton.New().As<Button>();
		dropdown.Text = world.Display;
		dropdown.Set("dropdown", worldBox);

		// Add children
		StageMessageVBox.AddChild(dropdown);
		StageMessageVBox.AddChild(worldBoxMargin);
		worldBoxMargin.AddChild(worldBox);

		return result;
	}

	private void SetupWorldVBoxWithoutWorld(List<string> files, SarcFile arc)
	{
		if (files.Count == 0)
			return;

		// Create new container
		var worldBoxMargin = new MarginContainer();
		var worldBox = new VBoxContainer();

		// Fill container with buttons
		foreach (var file in files)
			CreateButton(arc, file, worldBox);

		// Create dropdown button
		var dropdown = DropdownButton.New().As<Button>();
		dropdown.Text = "Misc.";
		dropdown.Set("dropdown", worldBox);

		// Add children
		StageMessageVBox.AddChild(dropdown);
		StageMessageVBox.AddChild(worldBoxMargin);
		worldBoxMargin.AddChild(worldBox);
	}

	private void CreateButton(SarcFile file, string key, VBoxContainer box)
	{
		var button = DoublePressButton.New().As<Button>();
		button.Text = key;
		button.Alignment = HorizontalAlignment.Left;

		if (box.IsInsideTree())
			button.FocusNeighborLeft = box.GetPath();

		// These signals are automatically disconnected on free by DoublePressButton gdscript code
		button.Connect("pressed", Callable.From(new Action(() => OnFilePressed(file, key))));
		button.Connect("double_pressed",
			Callable.From(new Action(() => MsbtAppHolder.OpenApp(file.Name, key))));

		box.AddChild(button);
	}

	private void OnFilePressed(SarcFile archive, string key)
	{
		SelectedFile = archive.GetFileMSBT(key, new MsbtElementFactoryProjectSmo());
	}
}
