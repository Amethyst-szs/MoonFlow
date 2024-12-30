using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

using Nindot;

using MoonFlow.Project;
using MoonFlow.Scene.EditorEvent;
using MoonFlow.Scene.Main;

using CSExtensions;
using ByteSizeLib;

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

	private EventDataArchive SelectedArchive = null;
	private string SelectedEvent = null;

	#region Initilization

	public override void _Ready()
	{
		ProjectManager.SceneRoot.NodeHeader.Connect(Header.SignalName.ButtonSave,
			Callable.From(new Action<bool>(OnAnyFileSaved)), (uint)ConnectFlags.Deferred
        );

		SelectionInfoBox.Hide();
		GenerateFileList();
	}

	private void GenerateFileList()
	{
		// Reset selection
		SelectedArchive = null;
		SelectedEvent = null;

		// Get archive list
		var arcHolder = ProjectManager.GetProject().EventArcHolder;
		arcHolder.RefreshArchiveList();
		
		var arcList = arcHolder.Content;

		// Clear current file list
		foreach (var child in ArchiveHolder.GetChildren())
		{
			ArchiveHolder.RemoveChild(child);
			child.QueueFree();
		}

		// Get list of files in sorted order
		var list = arcList.Keys.ToList();
		list.Sort(string.Compare);

		foreach (var file in list)
		{
			var name = file.Split('/', '\\').Last();
			var nameNoExt = name.TrimSuffix(".szs");

			var sarc = arcList[name];

			// Create a dropdown button and margin->vbox for each file
			var container = new MarginContainer();
			var vbox = new VBoxContainer()
			{
				Name = nameNoExt,
			};

			var dropdown = DropdownButton.New().As<Button>();
			dropdown.Text = name;
			dropdown.Set("dropdown", container);

			dropdown.Connect(Button.SignalName.Pressed, Callable.From(() => OnArchiveDropdownPressed(sarc)));
			if (SelectedArchive == null)
				OnArchiveDropdownPressed(sarc);

			ArchiveHolder.AddChild(dropdown);
			ArchiveHolder.AddChild(container);
			container.AddChild(vbox);

			// If this sarc is originating from RomFs and not the project, slightly darken
			// archive button
			if (sarc.Source == EventDataArchive.ArchiveSource.ROMFS)
				dropdown.SelfModulate = Colors.Gray;

			// Add all BYML files as buttons in container
			SetupArchiveFileList(sarc, nameNoExt);
		}
	}

	private void SetupArchiveFileList(EventDataArchive arc, string nodeName)
	{
		var container = ArchiveHolder.FindChild(nodeName, true, false);
		if (!IsInstanceValid(container))
			throw new NullReferenceException("Could not lookup " + nodeName);

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

	private async void OnAnyFileSaved(bool _)
	{
		// Janky fix I know :(
		await ToSignal(GetTree().CreateTimer(0.75F), Timer.SignalName.Timeout);
		GenerateFileList();
	}

	private void OnArchiveDropdownPressed(EventDataArchive archive)
	{
		// Set fields
		SelectedArchive = archive;
		SelectedEvent = null;

		// Update info box
		SelectionLabel.Text = archive.Name;
		SelectionInfoBox.Show();
		VBoxArcInfo.Show();
		VBoxEventInfo.Hide();

		UpdateInfoBoxArchive(archive);
	}

	private void OnEventFilePressed(EventDataArchive archive, string key, Button button)
	{
		// Setup selection
		ArchiveHolder.DeselectAllButtons();

		button.SetPressedNoSignal(true);
		button.GrabFocus();

		// Set selection fields
		SelectedArchive = archive;
		SelectedEvent = key;

		// Update info box
		SelectionLabel.Text = key;
		SelectionInfoBox.Show();
		VBoxArcInfo.Hide();
		VBoxEventInfo.Show();

		UpdateInfoBoxArchive(archive);
		UpdateInfoBoxEvent(archive, key);
	}

	private static void OnEventFileOpened(SarcFile archive, string key)
	{
		EventFlowApp.OpenApp(archive, key);
	}

	private void OnFooterOpenFilePressed()
	{
		if (SelectedArchive == null || SelectedEvent == null)
			return;
		
		EventFlowApp.OpenApp(SelectedArchive, SelectedEvent);
	}

	private void OnDeleteFile()
	{
		if (SelectedArchive == null)
		{
			GD.PushError("No archive selected!");
			return;
		}

		if (SelectedEvent == null)
		{
			// Return if the archive isn't saved to project
			if (!File.Exists(SelectedArchive.FilePath))
				return;
			
			// Delete all mfgraph metadata files stored in archive
			foreach (var file in SelectedArchive.Content.Keys)
			{
				var path = GraphMetaHolder.GetPath(SelectedArchive.Name, file);
				if (File.Exists(path))
					File.Delete(path);
			}

			// Delete archive file
			File.Delete(SelectedArchive.FilePath);

			// Delete archive from project manager
			ProjectManager.GetProject().EventArcHolder.DeleteArchive(SelectedArchive);
			GenerateFileList();
		}
	}

	private void OnLineSearchTextChanged(string txt)
	{
		HomeRoot.RecursiveFileSearch(ArchiveHolder, txt);
	}

	private void OnButtonCopyGraphDebugHashPressed()
	{
		if (SelectedArchive == null || SelectedEvent == null)
			return;
		
		var hash = GraphMetaHolder.CalcNameHash(SelectedArchive.Name, SelectedEvent);
		DisplayServer.ClipboardSet(hash);

		GD.Print(hash + " added to system clipboard!");
	}

	#endregion

	#region Utilties

	private void UpdateInfoBoxArchive(SarcFile archive)
	{
		// Set archive last modification time
		if (File.Exists(archive.FilePath))
		{
			var t = archive.GetLastModifiedTime();
			GetNode<Label>("%Label_ArcDateTime").Text = t.ToShortDateString() + "\n(" + t.ToLongTimeString() + ')';
		}
		else
			GetNode<Label>("%Label_ArcDateTime").Text = "N/A";
	}

	private void UpdateInfoBoxEvent(SarcFile archive, string key)
	{
		GetNode<Label>("%Label_ArcName").Text = archive.Name;
		GetNode<Label>("%Label_Size").Text = ByteSize.FromBytes(archive.Content[key].Count).ToString();;
	}

	#endregion
}
