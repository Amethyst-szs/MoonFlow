using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

using Nindot.LMS.Msbp;
using Nindot.LMS.Msbt;
using Nindot.Al.Localize;

using MoonFlow.Project;
using MoonFlow.Scene.Main;

namespace MoonFlow.LMS.Msbt;

public partial class MsbtEditor : PanelContainer
{
	private MsbpFile Project = null;
	private SarcMsbtFile File = null;
	private Dictionary<string, SarcMsbtFile> FileList = null;

	private VBoxContainer EntryList = null;
	private VBoxContainer EntryContent = null;

	private Label FileTitleName = null;
	private Label FileEntryName = null;
	private LangPicker LanguagePicker = null;

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
		LanguagePicker = GetNode<LangPicker>("%LanguagePicker");

		// Setup signals with header
		var header = ProjectManager.SceneRoot.NodeHeader;
		header.Connect(Header.SignalName.ButtonSave, Callable.From(SaveFile));
	}

	private void InitEditor()
	{
		// Ensure we have a valid pointer to the file and project
		if (File == null || Project == null)
			throw new Exception("Cannot init MsbtEditor without File and Project");

		// If there is already a selection in the entry list, copy down its name for later
		string selectionName = null;
		if (IsInstanceValid(EntryListSelection))
			selectionName = EntryListSelection.Name;

		// If the EntryList already has children, empty lists
		if (EntryList.GetChildCount() > 0)
		{
			// Clear out entry list and content in case the editor is being reinitilized
			foreach (var child in EntryList.GetChildren())
			{
				EntryList.RemoveChild(child);
				child.QueueFree();
			}

			foreach (var child in EntryContent.GetChildren())
			{
				EntryContent.RemoveChild(child);
				child.QueueFree();
			}
		}

		// Create entry list sorted alphabetically
		var labelList = File.GetEntryLabels().ToArray();
		Array.Sort(labelList, string.Compare);

		foreach (var label in labelList) { CreateEntryListButton(label); }

		// Create entry content
		for (int i = 0; i < File.GetEntryCount(); i++) { CreateEntryContentEditor(i); }

		// Select either the first entry or selectionName
		selectionName ??= EntryList.GetChild(0).Name;
		CallDeferred("OnEntrySelected", selectionName, true);

		// Update entry count in other components
		int entryCount = File.GetEntryCount();
		EmitSignal(SignalName.EntryCountUpdated, [entryCount, entryCount]);
	}

	// ====================================================== //
	// ================ File Access Utilities =============== //
	// ====================================================== //

	public void OpenFile(MsbpFile project, Dictionary<string, SarcMsbtFile> msbtList, string defaultLang)
	{
		// Grab the default SarcMsbtFile using lang
		if (!msbtList.TryGetValue(defaultLang, out SarcMsbtFile defaultMsbt))
			if (!msbtList.TryGetValue("USen", out defaultMsbt))
				throw new Exception("TextFiles doesn't have default language or USen!");

		Project = project;
		File = defaultMsbt;
		FileList = msbtList;

		LanguagePicker.SetSelection(defaultLang);

		InitEditor();
	}

	public void SaveFile()
	{
		foreach (var f in FileList.Values)
			f.WriteArchive();
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

	private void OnLanguagePickerSelectedLang(int idx)
	{
		string lang = LanguageKeyTranslator.Table.Keys.ElementAt(idx);
		var newTarget = FileList[lang];

		if (newTarget == File)
			return;

		File = newTarget;
		InitEditor();
	}
}
