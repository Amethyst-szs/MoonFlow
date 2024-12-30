using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

using Nindot;

using MoonFlow.Project;
using MoonFlow.Scene.EditorEvent;

using CSExtensions;
using ByteSizeLib;
using System.IO;

namespace MoonFlow.Scene.Home;

[ScenePath("res://scene/home/tab_event/tab_event.tscn")]
public partial class TabEvent : HSplitContainer
{
	[Export, ExportGroup("Internal References")]
	private VBoxContainer ArchiveHolder;
	[Export]
	private Label SelectionLabel;
	[Export]
	private MarginContainer SelectionInfoBox;
	[Export]
	private VBoxContainer VBoxArcInfo;
	[Export]
	private VBoxContainer VBoxEventInfo;

	private GDScript DropdownButton = GD.Load<GDScript>("res://addons/ui_node_ext/dropdown_checkbox.gd");
	private GDScript DoublePressButton = GD.Load<GDScript>("res://addons/ui_node_ext/double_click_button.gd");

	private Dictionary<string, SarcFile> FileList = [];

	private string SelectedArchive = null;
	private string SelectedEvent = null;

	#region Initilization

	public override void _Ready()
	{
		FileList = ProjectManager.GetProject().EventArcHolder.Content;

		SelectionInfoBox.Hide();

		GenerateFileList();
	}

	private void GenerateFileList()
	{
		// Clear current file list
		foreach (var child in ArchiveHolder.GetChildren())
		{
			ArchiveHolder.RemoveChild(child);
			child.QueueFree();
		}

		// Get list of files in sorted order
		var list = FileList.Keys.ToList();
		list.Sort(string.Compare);

		foreach (var file in list)
		{
			var name = file.Split('/', '\\').Last();
			var nameNoExt = name.TrimSuffix(".szs");

			SelectedArchive ??= name;

			// Create a dropdown button and margin->vbox for each file
			var container = new MarginContainer();
			var vbox = new VBoxContainer()
			{
				Name = nameNoExt,
			};

			var dropdown = DropdownButton.New().As<Button>();
			dropdown.Text = name;
			dropdown.Set("dropdown", container);

			dropdown.Connect(Button.SignalName.Pressed, Callable.From(() => OnArchiveDropdownPressed(FileList[name])));

			ArchiveHolder.AddChild(dropdown);
			ArchiveHolder.AddChild(container);
			container.AddChild(vbox);

			// Add all BYML files as buttons in container
			SetupArchiveFileList(nameNoExt);
		}
	}

	private void SetupArchiveFileList(string name)
	{
		var container = ArchiveHolder.FindChild(name, true, false);
		if (!IsInstanceValid(container))
			throw new NullReferenceException("Could not lookup " + name);

		if (!FileList.TryGetValue(name + ".szs", out SarcFile arc))
			throw new Exception("Could not find archive " + name);

		var list = arc.Content.Keys.ToList();
		list.Sort(string.Compare);

		foreach (var file in list)
		{
			var item = file.TrimSuffix(".byml");

			var button = DoublePressButton.New().As<Button>();
			button.ToggleMode = true;
			button.Name = item;
			button.Text = item;
			button.TooltipText = arc.Name;
			button.Alignment = HorizontalAlignment.Left;

			// These signals are automatically disconnected on free by DoublePressButton gdscript code
			button.Connect("pressed", Callable.From(() => OnEventFilePressed(arc, file, button)));
			button.Connect("double_pressed",
				Callable.From(() => OnEventFileOpened(arc, file)));

			container.AddChild(button);
		}
	}

	#endregion

	#region Signals

	private void OnArchiveDropdownPressed(SarcFile archive)
	{
		// Set fields
		SelectedArchive = archive.Name;
		SelectedEvent = null;

		// Update info box
		SelectionLabel.Text = archive.Name;
		SelectionInfoBox.Show();
		VBoxArcInfo.Show();
		VBoxEventInfo.Hide();

		UpdateInfoBoxArchive(archive);
	}

	private void OnEventFilePressed(SarcFile archive, string key, Button button)
	{
		// Setup selection
		ArchiveHolder.DeselectAllButtons();

		button.SetPressedNoSignal(true);
		button.GrabFocus();

		// Set selection fields
		SelectedArchive = archive.Name;
		SelectedEvent = key;

		// Update info box
		SelectionLabel.Text = key;
		SelectionInfoBox.Show();
		VBoxArcInfo.Show();
		VBoxEventInfo.Show();

		UpdateInfoBoxArchive(archive);
		UpdateInfoBoxEvent(archive, key);
	}

	private static void OnEventFileOpened(SarcFile archive, string key)
	{
		EventFlowApp.OpenApp(archive, key);
	}

	private void OnLineSearchTextChanged(string txt)
	{
		HomeRoot.RecursiveFileSearch(ArchiveHolder, txt);
	}

	#endregion

	#region Utilties

	private void UpdateInfoBoxArchive(SarcFile archive)
	{
		if (File.Exists(archive.FilePath))
		{
			long length = new FileInfo(archive.FilePath).Length;
			GetNode<Label>("%Label_ArcSize").Text = ByteSize.FromBytes(length).ToString();

			var t = archive.GetLastModifiedTime();
			GetNode<Label>("%Label_ArcDateTime").Text = t.ToShortDateString() + "\n(" + t.ToLongTimeString() + ')';
		}
		else
		{
			GetNode<Label>("%Label_ArcSize").Text = "";
			GetNode<Label>("%Label_ArcDateTime").Text = "N/A";
		}
	}

	private void UpdateInfoBoxEvent(SarcFile archive, string key)
	{

	}

	#endregion
}
