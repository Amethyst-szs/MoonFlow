using Godot;
using System;
using System.Linq;
using System.IO;

using Nindot;

using MoonFlow.Project;
using MoonFlow.Scene.EditorEvent;
using MoonFlow.Scene.Main;

using MoonFlow.Ext;
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
	[Export]
	private Godot.Collections.Array<Button> DisableWhenNoGraphSelected = [];

	private GDScript DropdownButton = GD.Load<GDScript>("res://addons/ui_node_ext/dropdown_checkbox.gd");
	private GDScript DoublePressButton = GD.Load<GDScript>("res://addons/ui_node_ext/double_click_button.gd");

	public EventDataArchive SelectedArchive { get; private set; } = null;
	public string SelectedEvent { get; private set; } = null;

	#region Initilization

	public override void _Ready()
	{
		SelectionInfoBox.Hide();
		GenerateFileList();
	}

	public void GenerateFileList()
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
			dropdown.Name = name.Replace(".", "");
			dropdown.Text = name;
			dropdown.Set("dropdown", container);
			UpdateDropdownButtonModulate(sarc, dropdown);

			var call = Callable.From(() => OnArchiveDropdownPressed(sarc, dropdown));
			dropdown.Connect(Button.SignalName.Pressed, call);
			dropdown.Connect(Button.SignalName.FocusEntered, call);

			if (SelectedArchive == null)
				OnArchiveDropdownPressed(sarc, dropdown);

			ArchiveHolder.AddChild(dropdown);
			ArchiveHolder.AddChild(container);
			container.AddChild(vbox);

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
			button.Name = file.Replace(".", "");
			button.Text = item;
			button.TooltipText = arc.Name;
			button.Alignment = HorizontalAlignment.Left;

			// These signals are automatically disconnected on free by DoublePressButton gdscript code
			var pressCall = Callable.From(() => OnEventFilePressed(arc, file, button));
			button.Connect(Button.SignalName.Pressed, pressCall);
			button.Connect(Button.SignalName.FocusEntered, pressCall);

			button.Connect("double_pressed",
				Callable.From(() => OnEventFileOpened(arc, file)));

			container.AddChild(button);
		}
	}

	#endregion

	#region Signals

	private void OnArchiveDropdownPressed(EventDataArchive archive, Button dropdown)
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

		// Update dropdown color
		UpdateDropdownButtonModulate(archive, dropdown);

		// Update button states
		foreach (var item in DisableWhenNoGraphSelected)
			item.Disabled = true;
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

		// Update dropdown color
		var buttonName = archive.Name.Replace(".", "");
		if (ArchiveHolder.FindChild(buttonName, false, false) is Button dropdown)
			UpdateDropdownButtonModulate(archive, dropdown);

		// Update button states
		foreach (var item in DisableWhenNoGraphSelected)
			item.Disabled = false;
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

	public void ReloadInterface(bool isRunReady)
	{
		if (isRunReady)
			GenerateFileList();

		if (SelectedArchive == null)
			return;

		if (SelectedEvent == null)
		{
			if (ArchiveHolder.FindChild(SelectedArchive.Name.Replace(".", ""), false, false) is not Button dropdown)
				return;

			OnArchiveDropdownPressed(SelectedArchive, dropdown);
			return;
		}

		if (ArchiveHolder.FindChild(SelectedEvent.Replace(".", ""), true, false) is not Button button)
			return;

		OnEventFilePressed(SelectedArchive, SelectedEvent, button);
	}

	private static void UpdateDropdownButtonModulate(EventDataArchive arc, Button button)
	{
		if (arc.Source == EventDataArchive.ArchiveSource.ROMFS)
			button.SelfModulate = Colors.Gray;
		else
			button.SelfModulate = Colors.White;
	}

	private void UpdateInfoBoxArchive(SarcFile archive)
	{
		// Set archive last modification time
		if (File.Exists(archive.FilePath))
		{
			var t = archive.GetLastModifiedTime();
			GetNode<Label>("%Label_ArcDateTime").Text = t.ToShortDateString() + "\n(" + t.ToLongTimeString() + ')';
			GetNode<Control>("%ArcNotWithinProject").Visible = false;
		}
		else
		{
			GetNode<Label>("%Label_ArcDateTime").Text = "N/A";
			GetNode<Control>("%ArcNotWithinProject").Visible = true;
		}
	}

	private void UpdateInfoBoxEvent(SarcFile archive, string key)
	{
		GetNode<Label>("%Label_ArcName").Text = archive.Name;
		GetNode<Label>("%Label_Size").Text = ByteSize.FromBytes(archive.Content[key].Count).ToString();
	}

	#endregion
}
