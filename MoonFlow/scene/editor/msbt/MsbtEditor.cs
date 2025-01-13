using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

using Nindot.LMS.Msbp;
using Nindot.LMS.Msbt;

using MoonFlow.Project;
using MoonFlow.Scene.Main;
using MoonFlow.Async;
using System.Threading.Tasks;

namespace MoonFlow.Scene.EditorMsbt;

public partial class MsbtEditor : PanelContainer
{
	#region Properties

	// ~~~~~~~~~~~~~~~ Content ~~~~~~~~~~~~~~~ //

	public SarcMsbpFile Project { get; private set; } = null;
	public SarcMsbtFile File { get; private set; } = null;
	public Dictionary<string, SarcMsbtFile> FileList { get; private set; } = null;
	public string DefaultLanguage { get; private set; } = "USen";
	public string CurrentLanguage { get; private set; } = "USen";

	// ~~~~~~~~~~~~~~~~ State ~~~~~~~~~~~~~~~~ //

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

	public MsbtEntryEditor EntryContentSelection { get; private set; }

	// ~~~~~~~~~ Internal References ~~~~~~~~~ //

	private MsbtAppHolder Parent = null;

	[Export, ExportGroup("Internal References")]
	private EntryListHolder EntryListHolder = null;
	private EntryListBase EntryList
	{
		get { return EntryListHolder?.EntryList; }
	}

	[Export]
	public VBoxContainer EntryContentHolder { get; private set; }

	[Export]
	private Label FileTitleName;
	[Export]
	private Label FileEntryName;
	[Export]
	private LangPicker LanguagePicker;

	// ~~~~~~~~~~~~ Packed Scenes ~~~~~~~~~~~~ //

	[Export, ExportGroup("Packed Scenes")]
	private PackedScene UnsavedChangesLanguageChangePopup;

	// ~~~~~~~~~~~~~~~ Signals ~~~~~~~~~~~~~~~ //

	[Signal]
	public delegate void ContentModifiedEventHandler(string label);
	[Signal]
	public delegate void ContentNotModifiedEventHandler();

	#endregion

	#region Initilization

	public override void _Ready()
	{
		// Get parent
		Parent = this.FindParentByType<MsbtAppHolder>();

		// Setup entry list holder
		EntryListHolder.SetupList<EntryListSimple>();

		// Get entry list from holder
		if (!IsInstanceValid(EntryList))
			throw new NullReferenceException(nameof(EntryList));

		// Setup signals with header
		var header = ProjectManager.SceneRoot.NodeHeader;
		header.Connect(Header.SignalName.ButtonSave, Callable.From(new Action<bool>(SaveFileInternal)));
	}

	private void InitEditor()
	{
		// Ensure we have a valid pointer to the file and project
		if (File == null || Project == null)
			throw new Exception("Cannot init MsbtEditor without File and Project");

		// Point primary file to current language
		File = FileList[CurrentLanguage];

		// If there is already a selection in the entry list, copy down its name for later
		string selectionName = null;

		if (IsInstanceValid(EntryList.EntryListSelection))
			selectionName = EntryList.EntryListSelection.Name;

		// Clear out entry list and content in case the editor is being reinitilized
		EntryList.QueueFreeAllChildren();
		EntryContentHolder.QueueFreeAllChildren();

		// Create entry list
		if (EntryList is EntryListSimple && IsStageMessage())
			EntryListHolder.SetupList<EntryListStageMessage>();
		else if (EntryList is EntryListStageMessage && !IsStageMessage())
			EntryListHolder.SetupList<EntryListSimple>();

		EntryListHolder.UpdateToolButtonRestrictions();
		
		EntryList.CreateContent(File, out string[] labels);

		// Create entry content
		for (int i = 0; i < File.GetEntryCount(); i++)
		{
			var editor = CreateEntryContentEditor(i);
			EntryContentHolder.AddChild(editor, true);
		}

		// Select either the first label or selectionName
		if (labels.Length > 0)
		{
			var firstItem = EntryList.FindChild(labels[0], true, false);
			if (IsInstanceValid(firstItem))
				selectionName ??= firstItem.Name;

			EntryList.SetSelection(selectionName, false);
		}

		EntryList.UpdateEntryCount();

		// Set file title
		FileTitleName.Text = File.Name;
	}

	private void SetupTranslationStateEnabled(string lang)
	{
		CurrentLanguage = lang;
		InitEditor();
	}
	private void SetupTranslationStateDisabled()
	{
		CurrentLanguage = DefaultLanguage;
		InitEditor();
	}

	public MsbtEntryEditor CreateEntryContentEditor(int i)
	{
		// Get access to the metadata accessor for the current language
		var metadataAccessor = ProjectManager.GetMSBTMetaHolder(CurrentLanguage)
		?? throw new Exception("Invalid metadata accessor!");

		// Get access to the requested entry and metadata
		var entry = File.GetEntry(i);
		var metadata = metadataAccessor.GetMetadata(File, entry);

		// Initilize entry editor
		var editor = new MsbtEntryEditor(this, entry, metadata)
		{
			Name = File.GetEntryLabel(i),
			Visible = false,
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ExpandFill
		};

		if (!IsDefaultLanguage())
			editor.SetTranslationMode();

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

	public void OpenFile(SarcMsbpFile project, Dictionary<string, SarcMsbtFile> msbtList, string lang)
	{
		// Grab the default SarcMsbtFile using lang
		if (!msbtList.TryGetValue(lang, out SarcMsbtFile defaultMsbt))
		{
			lang = "USen";
			if (!msbtList.TryGetValue("USen", out defaultMsbt))
				throw new Exception("TextFiles doesn't have default language or USen!");
		}

		Project = project;
		File = defaultMsbt;
		FileList = msbtList;

		var projConfig = ProjectManager.GetProject().Config.Data;

		DefaultLanguage = projConfig.DefaultLanguage;
		CurrentLanguage = lang;

		LanguagePicker.SetSelection(CurrentLanguage);
		LanguagePicker.SetGameVersion(projConfig.Version);

		// If any language is missing entry keys, add them
		foreach (var target in FileList.Values)
			FixMissingOrExtraEntryKeys(defaultMsbt, target);

		InitEditor();
	}

	private async void SaveFileInternal(bool isRequireFocus) { await SaveFile(isRequireFocus); }
	public async Task SaveFile(bool isRequireFocus)
	{
		// Ensure app is focused
		var parent = GetParent() as MsbtAppHolder;
		if (!parent.AppIsFocused() && isRequireFocus)
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
		EntryListBase.ClearAllModifiedIcons(EntryList);

		// Remove appended modified icon in title
		FileTitleName.Text = FileTitleName.Text.TrimSuffix("*");

		// Update label cache in background
		ProjectManager.UpdateMsbtLabelCache();
	}

	public void TaskRunWriteFile(AsyncDisplay display)
	{
		// Get access to the default language's SarcMsbtFile
		var fileDL = FileList[DefaultLanguage];

		for (int i = 0; i < FileList.Count; i++)
		{
			var f = FileList.ElementAt(i);
			var metaHolder = ProjectManager.GetMSBTMetaHolder(f.Key);

			foreach (var entryLabel in f.Value.GetEntryLabels())
			{
				// Get entry and meta for this language
				var entry = f.Value.GetEntry(entryLabel);
				var meta = metaHolder.GetMetadata(f.Value, entry);

				// If language syncing is disabled for this entry, continue
				if (meta.IsDisableSync)
				{
					entry.ResetModifiedFlag();
					continue;
				}

				// If the default language entry was modified, replace this language's entry
				var entryDL = fileDL.GetEntry(entryLabel);
				if (entryDL == null)
					continue;

				if (entryDL.IsModified)
				{
					var newEntry = entryDL.CloneDeep();
					newEntry.ResetModifiedFlag();

					f.Value.ReplaceEntry(entryLabel, newEntry);
				}
			}

			// Write updated archive to disk
			f.Value.WriteArchive();
			display.UpdateProgress(i, FileList.Count + 1);
		}

		// Write metadata
		display.UpdateProgress(FileList.Count, FileList.Count + 1);

		var metaAccess = ProjectManager.GetMSBTMetaHolder(CurrentLanguage);
		metaAccess.SetLastModifiedTime(File);

		metaAccess.WriteFile();

		// Reset flag
		ForceResetModifiedFlag();
	}

	#endregion

	// ====================================================== //
	// ==================== Signal Events =================== //
	// ====================================================== //

	#region Signals

	public void OnEntryListSelection(string label)
	{
		EntryContentSelection = EntryContentHolder.GetNodeOrNull<MsbtEntryEditor>(label);
		if (IsInstanceValid(EntryContentSelection))
			EntryContentSelection.Show();

		// Update entry header
		FileEntryName.Text = label;
	}

	public void OnAddEntryNameSubmitted(string name)
	{
		foreach (var file in FileList.Values)
			file.AddEntry(name);

		EntryList.CreateEntryListButton(name, true);

		// Create editable editor for current language
		var editor = CreateEntryContentEditor(File.GetEntryIndex(name));
		EntryContentHolder.AddChild(editor, true);

		EntryList.OnEntrySelected(name, true);
		EntryList.UpdateEntryCount();

		editor.SetModified();
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

		SetModified();
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

	private void OnLanguagePickerSelectedLang(string lang, int idx)
	{
		SetTranslationModeState(lang);
	}

	#endregion

	// ====================================================== //
	// ====================== Utilities ===================== //
	// ====================================================== //

	#region Utilities

	public bool IsStageMessage() { return File.Sarc.Name == "StageMessage.szs"; }
	public bool IsDefaultLanguage() { return CurrentLanguage == DefaultLanguage; }

	public void SetModified()
	{
		IsModified = true;

		// Append asterisk to file name
		if (!FileTitleName.Text.EndsWith('*'))
			FileTitleName.Text += '*';

		EmitSignal(SignalName.ContentModified, "");
	}

	public void SetTranslationModeState(string lang)
	{
		if (!IsInsideTree() || !GetParent().IsNodeReady())
			return;

		if (!IsModified)
		{
			SetTranslationModeStateCallback(lang, true);
			return;
		}

		Parent.AppearUnsavedChangesDialog(out _, out _, UnsavedChangesLanguageChangePopup, new Action<bool>(
			(b) => SetTranslationModeStateCallback(lang, b)
		));
	}
	private async void SetTranslationModeStateCallback(string lang, bool isAcceptUnsaved)
	{
		if (!isAcceptUnsaved)
		{
			LanguagePicker.SetSelection(CurrentLanguage);
			return;
		}
		
		if (IsModified)
			await SaveFile(false);

		if (lang == DefaultLanguage)
		{
			if (IsDefaultLanguage())
				return;

			SetupTranslationStateDisabled();
			return;
		}

		if (CurrentLanguage == lang)
			return;

		SetupTranslationStateEnabled(lang);
	}

	public void ForceResetModifiedFlag() { IsModified = false; }
	public void SetSelection(string str) { EntryList.SetSelection(str, false); }
	public void UpdateEntrySearch(string str)
	{
		EntryListHolder.SearchBoxLine.Text = str;
		EntryList.UpdateSearch(str);
	}

	private static void FixMissingOrExtraEntryKeys(SarcMsbtFile source, SarcMsbtFile target)
	{
		if (source == target)
			return;

		foreach (var label in source.GetEntryLabels())
		{
			if (target.IsContainKey(label))
				continue;

			target.AddEntry(label, source.GetEntry(label).CloneDeep());
		}

		foreach (var label in target.GetEntryLabels())
		{
			if (source.IsContainKey(label))
				continue;

			target.RemoveEntry(label);
		}
	}

	#endregion
}
