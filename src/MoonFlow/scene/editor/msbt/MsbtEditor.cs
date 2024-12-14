using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

using Nindot.LMS.Msbp;
using Nindot.LMS.Msbt;
using Nindot.Al.Localize;

using MoonFlow.Project;
using MoonFlow.Scene.Main;
using MoonFlow.Async;
using System.Threading.Tasks;

namespace MoonFlow.Scene.EditorMsbt;

public partial class MsbtEditor : PanelContainer
{
	#region Properties

	public SarcMsbpFile Project { get; private set; } = null;
	public SarcMsbtFile File { get; private set; } = null;
	public Dictionary<string, SarcMsbtFile> FileList { get; private set; } = null;
	public string DefaultLanguage { get; private set; } = "USen";
	public string CurrentLanguage { get; private set; } = "USen";

	private bool _isModified = false;
	public bool IsModified
	{
		get { return _isModified; }
		private set
		{
			_isModified = value;

			if (!_isModified)
				CallDeferred(MethodName.EmitSignal, SignalName.ContentNotModified);
		}
	}

	private EntryListHolder EntryListHolder = null;
	private EntryListBase EntryList
	{
		get { return EntryListHolder?.EntryList; }
	}

	public VBoxContainer EntryContent { get; private set; } = null;
	public MsbtEntryEditor EntryContentSelection { get; private set; } = null;

	private Label FileTitleName = null;
	private Label FileEntryName = null;
	private LangPicker LanguagePicker = null;

	[Signal]
	public delegate void ContentModifiedEventHandler(string label);
	[Signal]
	public delegate void ContentNotModifiedEventHandler();

	#endregion

	#region Initilization

	public override void _Ready()
	{
		// Setup entry list holder
		EntryListHolder = GetNode<EntryListHolder>("%EntryListHolder");
		EntryListHolder.SetupList<EntryListSimple>();

		// Get entry list from holder
		if (!IsInstanceValid(EntryList))
			throw new NullReferenceException(nameof(EntryList));

		// Get pointers to additional children
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

		if (IsInstanceValid(EntryList.EntryListSelection))
			selectionName = EntryList.EntryListSelection.Name;

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

		// Create entry list
		if (EntryList is EntryListSimple && IsStageMessage())
			EntryListHolder.SetupList<EntryListStageMessage>();
		else if (EntryList is EntryListStageMessage && !IsStageMessage())
			EntryListHolder.SetupList<EntryListSimple>();

		EntryList.CreateContent(File, out string[] labels);

		// Create entry content
		for (int i = 0; i < File.GetEntryCount(); i++) { CreateEntryContentEditor(i); }

		// Select either the first label or selectionName
		if (labels.Length > 0)
		{
			var firstItem = EntryList.FindChild(labels[0], true, false);
			if (IsInstanceValid(firstItem))
				selectionName ??= firstItem.Name;

			EntryList.SetSelection(selectionName);
		}

		EntryList.UpdateEntryCount();

		// Set file title
		FileTitleName.Text = File.Name;
	}

	public MsbtEntryEditor CreateEntryContentEditor(int i)
	{
		// Get access to the metadata accessor for the current language
		var metadataAccessor = ProjectManager.GetMSBTMetaHolder(CurrentLanguage);
		if (metadataAccessor == null)
			throw new Exception("Invalid metadata accessor!");

		// Get access to the requested entry and metadata
		var entry = File.GetEntry(i);
		var metadata = metadataAccessor.GetMetadata(File, entry);

		// Initilize entry editor
		var editor = new MsbtEntryEditor(this, entry, metadata)
		{
			Name = File.GetEntryLabel(i),
			Visible = false,
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ExpandFill,
		};

		EntryContent.AddChild(editor, true);

		// Connect to signals
		editor.Connect(MsbtEntryEditor.SignalName.EntryModified,
			Callable.From(new Action<MsbtEntryEditor>(OnEntryModified)));

		return editor;
	}

	#endregion

	// ====================================================== //
	// ================ File Access Utilities =============== //
	// ====================================================== //

	#region Read and Write

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

		DefaultLanguage = ProjectManager.GetProject().Config.Data.DefaultLanguage;
		CurrentLanguage = defaultLang;
		LanguagePicker.SetSelection(CurrentLanguage);

		InitEditor();
	}

	public async void SaveFile()
	{
		// Ensure app is focused
		var parent = GetParent() as MsbtAppHolder;
		if (!parent.AppIsFocused())
			return;

		GD.Print("\n - Saving ", File.Name);

		var run = AsyncRunner.Run(TaskRunWriteFile, AsyncDisplay.Type.SaveMsbtArchives);

		await run.Task;
		await ToSignal(Engine.GetMainLoop(), "process_frame");

		if (run.Task.Exception == null)
			GD.Print("Saved ", File.Name);
		else
			GD.Print("Saving failed for ", File.Name);

		// Remove the modified icon from all entry buttons in editor
		foreach (var button in EntryList.GetChildren())
		{
			if (button.GetType() != typeof(Button))
				continue;

			((Button)button).Icon = null;
		}

		// Remove appended modified icon in title
		FileTitleName.Text = FileTitleName.Text.TrimSuffix("*");
	}

	public void TaskRunWriteFile(AsyncDisplay display)
	{
		// Get access to the default language's SarcMsbtFile
		var fileDL = FileList[DefaultLanguage];

		// Iterate through each language's version of this MSBT
		// This is used to perform language syncing
		bool isAnythingAnyLanguageModified = false;

		for (int i = 0; i < FileList.Count; i++)
		{
			var f = FileList.ElementAt(i);
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

			display.UpdateProgress(i, FileList.Count + 1);
		}

		// Write metadata
		display.UpdateProgress(FileList.Count, FileList.Count + 1);

		if (isAnythingAnyLanguageModified)
		{
			var metadataAccessor = ProjectManager.GetMSBTMetaHolder(CurrentLanguage);
			metadataAccessor.WriteFile();
		}

		// Reset flag
		IsModified = false;
	}

	#endregion

	// ====================================================== //
	// ==================== Signal Events =================== //
	// ====================================================== //

	#region Signals

	public void OnEntryListSelection(string label)
	{
		var content = EntryContent.GetNode<MsbtEntryEditor>(label);
		EntryContentSelection = content;

		if (IsInstanceValid(content))
			content.Show();

		// Update entry header
		FileEntryName.Text = label;
	}

	private void OnAddEntryNameSubmitted(string name)
	{
		foreach (var file in FileList.Values)
			file.AddEntry(name);

		EntryList.CreateEntryListButton(name, true);
		CreateEntryContentEditor(File.GetEntryIndex(name));

		EntryList.OnEntrySelected(name, true);
		EntryList.UpdateEntryCount();
	}

	private void OnDeleteEntryTrash()
	{
		if (!IsInstanceValid(EntryList.EntryListSelection) || !IsInstanceValid(EntryContentSelection))
			return;

		string entry = EntryList.EntryListSelection.Name;
		string prevEntry = File.GetEntryLabel(File.GetEntryIndex(entry) - 1);

		foreach (var file in FileList.Values)
			file.RemoveEntry(entry);

		EntryList.EntryListSelection.QueueFree();
		EntryContentSelection.QueueFree();

		if (prevEntry != null && prevEntry != string.Empty)
			EntryList.OnEntrySelected(prevEntry);

		EntryList.UpdateEntryCount();
	}

	private void OnEntryModified(MsbtEntryEditor entryEditor)
	{
		// Set flag
		IsModified = true;

		// Append asterisk to file name
		if (!FileTitleName.Text.EndsWith('*'))
			FileTitleName.Text += '*';

		// Alert other nodes of the content modification
		var entryName = entryEditor.Entry.Name;
		EmitSignal(SignalName.ContentModified, entryName);
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

	#endregion

	// ====================================================== //
	// ====================== Utilities ===================== //
	// ====================================================== //

	#region Utilities

	public bool IsStageMessage() { return File.Sarc.Name == "StageMessage.szs"; }

	public void ForceResetModifiedFlag() { IsModified = false; }
	public void UpdateEntrySearch(string str) { EntryList.UpdateSearch(str); }
	public void SetSelection(string str) { EntryList.SetSelection(str); }

	#endregion
}
