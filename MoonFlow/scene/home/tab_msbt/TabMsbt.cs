using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

using Nindot;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib.Smo;

using MoonFlow.Project;
using MoonFlow.Scene.EditorMsbt;
using MoonFlow.Project.Database;
using MoonFlow.Project.Templates;

using ByteSizeLib;

using CSExtensions;

namespace MoonFlow.Scene.Home;

public partial class TabMsbt : HSplitContainer
{
	private SarcMsbtFile SelectedFile = null;

	private ScrollContainer FileListRoot = null;
	private VBoxContainer SystemMessageButtons = null;
	private VBoxContainer StageMessageVBox = null;
	private VBoxContainer LayoutMessageButtons = null;

	private GDScript DropdownButton = GD.Load<GDScript>("res://addons/ui_node_ext/dropdown_checkbox.gd");
	private GDScript DoublePressButton = GD.Load<GDScript>("res://addons/ui_node_ext/double_click_button.gd");

	#region Init

	public override void _Ready()
	{
		FileListRoot = GetNode<ScrollContainer>("%FileListContent");
		SystemMessageButtons = GetNode<VBoxContainer>("%SystemMessage_VBox");
		LayoutMessageButtons = GetNode<VBoxContainer>("%LayoutMessage_VBox");
		StageMessageVBox = GetNode<VBoxContainer>("%StageMessage_VBox");

		foreach (var child in StageMessageVBox.GetChildren())
		{
			StageMessageVBox.RemoveChild(child);
			child.QueueFree();
		}

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
		foreach (var child in box.GetChildren())
		{
			box.RemoveChild(child);
			child.QueueFree();
		}

		string[] keys = [.. file.Content.Keys];
		Array.Sort(keys);

		foreach (var key in keys)
			CreateButton(file, key, box);
		
		if (file.Name == "SystemMessage.szs")
			OnFilePressed(file, keys[0], box.GetChild(0) as Button);
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

			CreateButton(arc, key, worldBox, world);
			result.Add(key);
		}

		// Create dropdown button
		var dropdown = DropdownButton.New().As<Button>();
		dropdown.Text = world.Display;
		dropdown.Set("dropdown", worldBoxMargin);

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
		dropdown.Set("dropdown", worldBoxMargin);

		// Add children
		StageMessageVBox.AddChild(dropdown);
		StageMessageVBox.AddChild(worldBoxMargin);
		worldBoxMargin.AddChild(worldBox);
	}

	private void CreateButton(SarcFile file, string key, Container box, WorldInfo world = null)
	{
		var button = DoublePressButton.New().As<Button>();
		button.ToggleMode = true;
		button.Name = key;
		button.Text = key;
		button.Alignment = HorizontalAlignment.Left;

		button.TooltipText = file.Name;
		if (world != null)
			button.TooltipText += '\n' + world.Display;

		if (box.IsInsideTree())
			button.FocusNeighborLeft = box.GetPath();

		// These signals are automatically disconnected on free by DoublePressButton gdscript code
		button.Connect("pressed", Callable.From(new Action(() => OnFilePressed(file, key, button))));
		button.Connect("double_pressed",
			Callable.From(new Action(() => MsbtAppHolder.OpenApp(file.Name, key))));

		box.AddChild(button);
	}

	#endregion

	#region Signals

	private void OnFilePressed(SarcFile archive, string key, Button button)
	{
		// Remove old selection
		SystemMessageButtons.DeselectAllButtons(); 
		LayoutMessageButtons.DeselectAllButtons();
		StageMessageVBox.DeselectAllButtons();

		// Set current selection
		if (!key.EndsWith(".msbt"))
			key += ".msbt";
		
		var length = archive.Content[key].Count;
		SelectedFile = archive.GetFileMSBT(key, new MsbtElementFactoryProjectSmo());

		button.SetPressedNoSignal(true);
		button.GrabFocus();

		// Update info box
		GetNode<Label>("%Label_InfoName").Text = key;

		var t = ProjectManager.GetMSBTMetaHolder().GetLastModifiedTime(archive, key);
		if (t.ToFileTimeUtc() != DateTime.UnixEpoch.ToFileTimeUtc())
			GetNode<Label>("%Label_DateTime").Text = t.ToShortDateString() + "\n(" + t.ToLongTimeString() + ')';
		else
			GetNode<Label>("%Label_DateTime").Text = "N/A";

		GetNode<Label>("%Label_Size").Text = ByteSize.FromBytes(length).ToString();
		GetNode<Label>("%Label_EntryCount").Text = SelectedFile.GetEntryCount().ToString();

		var usage = archive.Name;
		if (archive.Name == "StageMessage.szs")
		{
			var world = ProjectManager.GetDB().GetWorldInfoByStageName(key);
			if (world == null)
				GD.PushWarning("Stage does not exist in WorldList!");
			else
				usage = world.WorldName;
		}

		GetNode<Label>("%Label_Usage").Text = usage;
	}

	private void OnFooterOpenFilePressed()
	{
		if (SelectedFile == null) return;
		MsbtAppHolder.OpenApp(SelectedFile.Sarc.Name, SelectedFile.Name);
	}

	private void OnDuplicateFileRequested()
	{
		if (SelectedFile == null) return;
		var popup = GetNode<Popup>("Popup_DuplicateMsbt");

		popup.Popup();
		popup.Call("init_data", SelectedFile.Sarc.Name, SelectedFile.Name);
	}

	private void OnNewFileRequested()
	{
		var popup = GetNode<Popup>("Popup_NewMsbt");
		popup.Popup();

		popup.Call("init_data", SelectedFile.Sarc.Name, SelectedFile.Name);
	}

	private void OnDuplicateFile(string arcName, string newName)
	{
		if (newName == string.Empty)
			return;
		
		if (!TryGetWorld(arcName, newName, out WorldInfo world))
			return;
		
		var target = GetFileName(newName);
		var arcHolder = ProjectManager.GetMSBT();

		var sourceArc = SelectedFile.Sarc;
		if (!IsFileNameValid(target, sourceArc))
			return;

		// Duplicate file in all languages
		foreach (var lang in arcHolder)
		{
			var targetArc = lang.Value.GetArchiveByFileName(arcName);
			if (targetArc == null)
				throw new NullReferenceException("Could not resolve source and/or target archive!");

			if (!targetArc.Content.TryGetValue(SelectedFile.Name, out ArraySegment<byte> data))
			{
				GD.PushWarning("Skipping duplication for " + lang.Key + " due to lack of source file");
				continue;
			}

			targetArc.Content.Add(target, data.ToArray());
			targetArc.WriteArchive();
		}

		PublishMsbtToProject(arcName, newName, world);

		// Reload file list
		_Ready();

		var buttonName = target.Replace('.', '_');
		OnFilePressed(sourceArc, newName, FindChild(buttonName, true, false) as Button);
	}

	private void OnNewFile(string arcName, string newName)
	{
		if (newName == string.Empty)
			return;
		
		if (!TryGetWorld(arcName, newName, out WorldInfo world))
			return;
		
		var target = GetFileName(newName);
		var arcHolder = ProjectManager.GetMSBT();

		var sourceArc = SelectedFile.Sarc;
		if (!IsFileNameValid(target, sourceArc))
			return;

		// Create file in all languages
		foreach (var lang in arcHolder)
		{
			var targetArc = lang.Value.GetArchiveByFileName(arcName);
			if (targetArc == null)
				throw new NullReferenceException("Could not resolve source and/or target archive!");
			
			targetArc.Content.Add(target, LmsTemplates.EmptyMsbt);
			targetArc.WriteArchive();
		}

		PublishMsbtToProject(arcName, newName, world);

		// Reload file list
		_Ready();

		var buttonName = target.Replace('.', '_');
		OnFilePressed(sourceArc, newName, FindChild(buttonName, true, false) as Button);
	}

	private void OnDeleteFile()
	{
		var arcName = SelectedFile.Sarc.Name;
		var fileName = SelectedFile.Name;

		// Remove from all archives
		var arcHolder = ProjectManager.GetMSBT();
		foreach (var lang in arcHolder)
		{
			var targetArc = lang.Value.GetArchiveByFileName(arcName);
			if (targetArc == null)
				throw new NullReferenceException("Could not resolve source and/or target archive!");
			
			targetArc.Content.Remove(fileName);
			targetArc.WriteArchive();
		}

		// Remove from MSBP
		ProjectManager.GetMSBPHolder().UnpublishFile(arcName, fileName);

		// Reload file list
		_Ready();
	}

	private void OnLineSearchTextChanged(string txt)
	{
		HomeRoot.RecursiveFileSearch(FileListRoot, txt);
	}

	private static void OnOpenMsbpColorEditor()
	{
		var app = SceneCreator<MsbpColorEditor>.Create();
		app.SetUniqueIdentifier();
		ProjectManager.SceneRoot.NodeApps.AddChild(app);
	}

	#endregion

	#region File Utility

	private static string GetFileName(string name)
	{
		if (name.EndsWith(".msbt"))
			return name;

		return name + ".msbt";
	}

	private bool IsFileNameValid(string name, SarcFile sourceArc)
	{
		if (sourceArc == null || sourceArc.Content.ContainsKey(name))
		{
			GetNode<AcceptDialog>("Dialog_CreateError_DuplicateName").Popup();
			return false;
		}

		return true;
	}

	private bool TryGetWorld(string arc, string targetName, out WorldInfo world)
	{
		if (arc != "StageMessage.szs")
		{
			world = null;
			return true;
		}

		var db = ProjectManager.GetDB();
		world = db.GetWorldInfoByStageName(targetName);

		if (world != null)
			return true;

		GetNode<AcceptDialog>("Dialog_CreateError_WorldList").Popup();
		return false;
	}

	private void PublishMsbtToProject(string arcName, string newName, WorldInfo world)
	{
		// Publish entry to ProjectData
		var msbp = ProjectManager.GetMSBPHolder();

		if (world == null)
			msbp.PublishFile(arcName, newName);
		else
			msbp.PublishFile(arcName, newName, world);
	}

	#endregion
}
