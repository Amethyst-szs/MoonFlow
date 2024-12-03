using Godot;
using System;
using System.Linq;

using Nindot.LMS.Msbp;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace MoonFlow.LMS.Msbt;

public partial class MsbtEditor : PanelContainer
{
	private MsbpFile Project = null;
	private MsbtFile File = null;

	private VBoxContainer EntryList = null;
	private VBoxContainer EntryContent = null;

	private Label FileTitleName = null;
	private Label FileEntryName = null;

	private Button EntryListSelection = null;
	private MsbtEntryEditor EntryContentSelection = null;

	[Signal]
	public delegate void EntrySelectedEventHandler(string label);

	[Signal]
	public delegate void EntryCountUpdatedEventHandler(int total, int matchSearch);

	public override void _Ready()
	{
		// Get access to list and content
		EntryList = GetNode<VBoxContainer>("%List");
		EntryContent = GetNode<VBoxContainer>("%Content");
		FileTitleName = GetNode<Label>("%FileTitle");
		FileEntryName = GetNode<Label>("%FileEntry");
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

		foreach (var label in labelList) { CreateEntryListButton(label); }

		// Create entry content
		for (int i = 0; i < File.GetEntryCount(); i++) { CreateEntryContentEditor(i); }

		// Select first entry
		var firstItem = (Button)EntryList.GetChild(0);
		CallDeferred("OnEntrySelected", firstItem.Name, true);

		// Update entry count in other components
		int entryCount = File.GetEntryCount();
		EmitSignal(SignalName.EntryCountUpdated, [entryCount, entryCount]);
	}

	// ====================================================== //
	// ================ File Access Utilities =============== //
	// ====================================================== //

	public void OpenFile(MsbtFile file, MsbpFile project)
	{
		Project = project;
		File = file;

		InitEditor();
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

	private void OnEntrySelected(string label, bool isGrabFocus = true)
	{
		// Close old selection
		if (IsInstanceValid(EntryListSelection))
			EntryListSelection.ButtonPressed = false;

		if (IsInstanceValid(EntryContentSelection))
			EntryContentSelection.Hide();

		// Ensure string is not empty
		if (label == string.Empty)
			return;

		// Open entry
		var button = EntryList.GetNode<Button>(label);
		EntryListSelection = button;

		if (button == null)
			return;

		button.ButtonPressed = true;
		if (isGrabFocus)
			button.GrabFocus();

		var content = EntryContent.GetNode<MsbtEntryEditor>(label);
		EntryContentSelection = content;

		if (IsInstanceValid(content))
			content.Show();

		// Update file name and entry header
		FileTitleName.Text = File.Name;
		FileEntryName.Text = label;
	}
}
