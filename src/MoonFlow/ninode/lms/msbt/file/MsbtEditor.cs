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
	public SarcMsbpFile Project { get; private set; } = null;
	public SarcMsbtFile File { get; private set; } = null;
	public Dictionary<string, SarcMsbtFile> FileList { get; private set; } = null;
	public string DefaultLanguage { get; private set; } = "USen";
	public string CurrentLanguage { get; private set; } = "USen";

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

		// Set file title
		FileTitleName.Text = File.Name;
	}

	// ====================================================== //
	// ================ File Access Utilities =============== //
	// ====================================================== //

	public void OpenFile(SarcMsbpFile project, Dictionary<string, SarcMsbtFile> msbtList, string defaultLang)
	{
		// Grab the default SarcMsbtFile using lang
		if (!msbtList.TryGetValue(defaultLang, out SarcMsbtFile defaultMsbt))
		{
			defaultLang = "USen";
			if (!msbtList.TryGetValue("USen", out defaultMsbt))
				throw new Exception("TextFiles doesn't have default language or USen!");
		}

		Project = project;
		File = defaultMsbt;
		FileList = msbtList;

		DefaultLanguage = ProjectManager.GetProject().Config.DefaultLanguage;
		CurrentLanguage = defaultLang;
		LanguagePicker.SetSelection(CurrentLanguage);

		InitEditor();
	}

	public void SaveFile()
	{
		// Get access to the default language's SarcMsbtFile
		var fileDL = FileList[DefaultLanguage];

		// Iterate through each language's version of this MSBT
		// This is used to perform language syncing
		bool isAnythingAnyLanguageModified = false;

		foreach (var f in FileList)
		{
			var metaHolder = ProjectManager.GetMSBTMetaHolder(f.Key);
			bool isAnythingModified = false;

			foreach (var entryLabel in f.Value.GetEntryLabels())
			{
				// Get entry and meta for this language
				var entry = f.Value.GetEntry(entryLabel);
				var meta = metaHolder.GetMetadata(f.Value, entry);

				// If language syncing is disabled for this entry, continue
				if (meta.IsDisableSync)
				{
					isAnythingModified |= entry.IsModified;
					entry.ResetModifiedFlag();
					continue;
				}

				// If the default language entry was modified, replace this language's entry
				var entryDL = fileDL.GetEntry(entryLabel);
				if (entryDL.IsModified)
				{
					isAnythingModified = true;

					var newEntry = entryDL.CloneDeep();
					newEntry.ResetModifiedFlag();

					f.Value.ReplaceEntry(entryLabel, newEntry);
				}
			}

			// Write updated archive to disk
			if (isAnythingModified)
			{
				isAnythingAnyLanguageModified = true;
				f.Value.WriteArchive();
			}
		}

		// Remove the modified icon from all entry buttons in editor
		foreach (var button in EntryList.GetChildren())
		{
			if (button.GetType() != typeof(Button))
				continue;

			((Button)button).Icon = null;
		}

		// Remove appended modified icon in title
		FileTitleName.Text = FileTitleName.Text.TrimSuffix("*");

		// Write metadata
		if (isAnythingAnyLanguageModified)
		{
			var metadataAccessor = ProjectManager.GetMSBTMetaHolder(CurrentLanguage);
			metadataAccessor.WriteMetadata();
		}
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

		// Update entry header
		FileEntryName.Text = label;
	}

	private void OnLanguagePickerSelectedLang(int idx)
	{
		// Save file before switching languages
		SaveFile();

		// Update current language and reload editor
		CurrentLanguage = LanguageKeyTranslator.Table.Keys.ElementAt(idx);
		var newTarget = FileList[CurrentLanguage];

		if (newTarget == File)
			return;

		File = newTarget;
		InitEditor();
	}
}
