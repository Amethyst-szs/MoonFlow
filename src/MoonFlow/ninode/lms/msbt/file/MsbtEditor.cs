using Godot;
using System;

using Nindot.LMS.Msbp;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib.Smo;
using System.Linq;

namespace MoonFlow.LMS.Msbt;

public partial class MsbtEditor : PanelContainer
{
	private MsbpFile Project = null;
	private MsbtFile File = null;
	private string FileName = "";

	private VBoxContainer EntryList = null;
	private VBoxContainer EntryContent = null;

	private Label FileTitleName = null;
	private Label FileEntryName = null;

	private Button EntryListSelection = null;
	private MsbtEntryEditor EntryContentSelection = null;

	[Signal]
	public delegate void EntrySelectedEventHandler(string label);

	public MsbtEditor()
	{
		// TODO: DEBUG BULLSHITTERY REMOVE LATER
		var proj = MsbpFile.FromBytes(FileAccess.GetFileAsBytes("res://example/msbt/ProjectData.msbp"));
		var file = MsbtFile.FromBytes(FileAccess.GetFileAsBytes("res://example/msbt/SphinxQuiz.msbt"), new MsbtElementFactoryProjectSmo());
		OpenFile("NA", file, proj);
	}

	public override void _Ready()
	{
		// Get access to list and content
		EntryList = GetNode<VBoxContainer>("%List");
		EntryContent = GetNode<VBoxContainer>("%Content");
		FileTitleName = GetNode<Label>("%FileTitle");
		FileEntryName = GetNode<Label>("%FileEntry");

		// Setup components
		InitHeader();

		if (File != null && Project != null)
			InitEditor();
	}

	private void InitEditor()
	{
		// Ensure we have a valid pointer to the file and project
		if (File == null || Project == null)
			throw new Exception("Cannot init MsbtEditor without File and Project");

		// Clear out entry list and content in case the editor is being reinitilized
		foreach (var child in EntryList.GetChildren()) child.QueueFree();
		foreach (var child in EntryContent.GetChildren()) child.QueueFree();

		// Create entry list sorted alphabetically
		var labelList = File.GetEntryLabels().ToArray();
		Array.Sort(labelList, string.Compare);

		foreach (var label in labelList)
		{
			var button = new Button
			{
				Name = label,
				Text = label,
				ToggleMode = true,
				TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis
			};

			button.ButtonDown += () => OnEntrySelected(label);
			button.Pressed += () => OnEntrySelected(label);
			button.MouseEntered += () => OnEntryHovered(label);
			EntryList.AddChild(button, true);
		}

		// Create entry content
		for (int i = 0; i < File.GetEntryCount(); i++)
		{
			var editor = new MsbtEntryEditor(Project, File.GetEntry(i))
			{
				Name = File.GetEntryLabel(i),
				Visible = false,
				SizeFlagsHorizontal = SizeFlags.ExpandFill,
				SizeFlagsVertical = SizeFlags.ExpandFill,
			};

			EntryContent.AddChild(editor, true);
		}

		// Select first entry
		var firstItem = (Button)EntryList.GetChild(0);
		CallDeferred("OnEntrySelected", firstItem.Name);
	}

	// ====================================================== //
	// ================ File Access Utilities =============== //
	// ====================================================== //

	public void OpenFile(string fileName, MsbtFile file, MsbpFile project)
	{
		FileName = fileName;
		Project = project;
		File = file;
	}

	public void SaveFile()
	{
		GD.Print("Save Button");
	}

	// ====================================================== //
	// ==================== Signal Events =================== //
	// ====================================================== //

	private void OnEntryHovered(string label)
	{
		if (Input.IsMouseButtonPressed(MouseButton.Left))
			OnEntrySelected(label);
	}

	private void OnEntrySelected(string label)
	{
		// Close old selection
		if (IsInstanceValid(EntryListSelection))
			EntryListSelection.ButtonPressed = false;
		
		if (IsInstanceValid(EntryContentSelection))
			EntryContentSelection.Hide();

		// Open entry
		var button = EntryList.GetNode<Button>(label);
		EntryListSelection = button;

		if (button == null)
			return;
		
		button.ButtonPressed = true;
		button.GrabFocus();

		var content = EntryContent.GetNode<MsbtEntryEditor>(label);
		EntryContentSelection = content;

		if (IsInstanceValid(content))
			content.Show();
		
		// Update file name and entry header
		FileTitleName.Text = FileName;
		FileEntryName.Text = label;
	}
}
