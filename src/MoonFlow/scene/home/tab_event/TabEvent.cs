using Godot;
using System;
using System.Linq;
using System.Collections.Generic; 

using Nindot;

using MoonFlow.Project;
using MoonFlow.Scene.EditorEvent;

using CSExtensions;

namespace MoonFlow.Scene.Home;

[ScenePath("res://scene/home/tab_event/tab_event.tscn")]
public partial class TabEvent : HSplitContainer
{
	[Export, ExportGroup("Internal References")]
	private VBoxContainer ArchiveHolder;

	private GDScript DropdownButton = GD.Load<GDScript>("res://addons/ui_node_ext/dropdown_checkbox.gd");
	private GDScript DoublePressButton = GD.Load<GDScript>("res://addons/ui_node_ext/double_click_button.gd");

	private Dictionary<string, SarcFile> FileList = [];

    #region Initilization

    public override void _Ready()
    {
		FileList = ProjectManager.GetProject().EventArcHolder.Content;
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

			// Create a dropdown button and margin->vbox for each file
			var margin = new MarginContainer();
			var box = new VBoxContainer()
			{
				Name = nameNoExt,
			};

			var dropdown = DropdownButton.New().As<Button>();
			dropdown.Text = name;
			dropdown.Set("dropdown", margin);

			dropdown.Connect(Button.SignalName.Toggled, Callable.From(
				new Action<bool>((b) => OnToggleArchiveFolder(b, nameNoExt))
			));

			ArchiveHolder.AddChild(dropdown);
			ArchiveHolder.AddChild(margin);
			margin.AddChild(box);
		}
	}

	private void OnToggleArchiveFolder(bool isActive, string name)
	{
		var container = ArchiveHolder.FindChild(name, true, false);
		if (!IsInstanceValid(container))
			throw new NullReferenceException("Could not lookup " + name);
		
		if (!isActive)
		{
			foreach (var child in container.GetChildren())
				child.QueueFree();
			
			return;
		}

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
			button.Alignment = HorizontalAlignment.Left;

			// These signals are automatically disconnected on free by DoublePressButton gdscript code
			button.Connect("pressed", Callable.From(() => OnEventFilePressed(arc, file, button)));
			button.Connect("double_pressed",
				Callable.From(() => OnEventFileOpened(arc, file, button)));

			container.AddChild(button);
		}
	}

    #endregion

	#region Signals

	private void OnEventFilePressed(SarcFile archive, string key, Button button)
	{
		ArchiveHolder.DeselectAllButtons();

		button.SetPressedNoSignal(true);
		button.GrabFocus();
	}

	private void OnEventFileOpened(SarcFile archive, string key, Button button)
	{
		OnEventFilePressed(archive, key, button);
		OnEventFileOpened(archive, key);
	}
	private static void OnEventFileOpened(SarcFile archive, string key)
	{
		var graph = SceneCreator<EventFlowApp>.Create();
		ProjectManager.SceneRoot.NodeApps.AddChild(graph);

		graph.OpenFile(archive, key);
	}

	#endregion
}
