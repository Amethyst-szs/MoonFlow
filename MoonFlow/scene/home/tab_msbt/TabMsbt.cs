using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

using Nindot;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib.Smo;

using MoonFlow.Project;
using MoonFlow.Scene.EditorMsbt;
using MoonFlow.Project.Database;

using ByteSizeLib;

namespace MoonFlow.Scene.Home;

public partial class TabMsbt : HSplitContainer
{
	#region Properties

	// ~~~~~~~~~~~~~~~ Contents ~~~~~~~~~~~~~~ //

	public SarcMsbtFile SelectedFile { get; private set; } = null;

	public bool IsEnableTranslationFeatures { get; private set; } = false;
	public string TranslationLanguage { get; private set; } = "USen";

	// ~~~~~~~~~ Internal References ~~~~~~~~~ //

	[Export, ExportGroup("Internal References")]
	private ScrollContainer FileListScroll = null;
	[Export]
	private VBoxContainer FileListArchives = null;

	[Export]
	private Label TranslationLanguageWarning = null;

	[Export]
	private TabMsbtFileAccessor FileAccessor = null;

	// ~~~~~~~~~~~~~~~ Scripts ~~~~~~~~~~~~~~~ //

	private GDScript DropdownButton = GD.Load<GDScript>("res://scene/common/button/dropdown_checkbox.gd");
	private GDScript DoublePressButton = GD.Load<GDScript>("res://scene/common/button/double_click_button.gd");

	#endregion

	#region Init

	public override void _Ready()
	{
		var archives = ProjectManager.GetMSBTArchives(GetActiveLanguage());

		// Clear archive list
		FileListArchives.QueueFreeAllChildren();

		// Setup archive menus
		CreateArchiveDropdown(archives.SystemMessage);
		CreateStageMessageDropdown(archives.StageMessage);
		CreateArchiveDropdown(archives.LayoutMessage);

		// Set initial selection
		var first = FileListArchives.FindChildByType<Button>((node) => !node.HasMeta("dropdown_button"));
		first?.EmitSignal(Button.SignalName.Pressed);
	}

	private void CreateArchiveDropdown(SarcFile file)
	{
		// Create container
		var name = file.Name.RemoveFileExtension();
		CreateDropdown(FileListArchives, name, out VBoxContainer box, out Button dropdown);

		// Sort and add file keys as buttons
		string[] keys = [.. file.Content.Keys];
		Array.Sort(keys);

		bool isAnyFilesAdded = false;
		foreach (var key in keys)
			isAnyFilesAdded |= TryCreateButton(file, key, box);
		
		if (!isAnyFilesAdded)
		{
			dropdown.QueueFree();
			box.QueueFree();
		}
	}

	private void CreateStageMessageDropdown(SarcFile file)
	{
		// Create container
		var arcName = file.Name.RemoveFileExtension();
		CreateDropdown(FileListArchives, arcName, out VBoxContainer box, out Button dropdown);

		// Get lookup database
		var db = ProjectManager.GetProject().Database;

		// Attempt to create a dropdown container for each world
		bool isAnyWorldHaveContent = false;

		foreach (var world in db.WorldList)
			isAnyWorldHaveContent |= TryCreateWorldDropdown(file, world, box);

		// Create a container for every stage that doesn't an assigned world
		isAnyWorldHaveContent |= TryCreateNoAssignedWorldDropdown(file, box);

		// If absolutely no content was created, free container and dropdown
		if (!isAnyWorldHaveContent)
		{
			dropdown.QueueFree();
			box.QueueFree();
		}
	}

	private bool TryCreateWorldDropdown(SarcFile arc, WorldInfo world, VBoxContainer parent)
	{
		// Create container
		CreateDropdown(parent, world.Display, out VBoxContainer box, out Button dropdown);

		// Fill container with file buttons
		var prevCategory = StageInfo.CatEnum.MainStage;
		bool isAnyFilesAdded = false;

		foreach (var stage in world.StageList)
		{
			var key = stage.name + ".msbt";

			// If the archive doesn't have a file for the stage, skip
			if (!arc.Content.ContainsKey(key))
				continue;

			if (stage.CategoryType != prevCategory)
			{
				prevCategory = stage.CategoryType;

				if (!IsEnableTranslationFeatures)
					box.AddChild(new HSeparator());
			}

			isAnyFilesAdded |= TryCreateButton(arc, key, box, world);
		}

		if (!isAnyFilesAdded)
		{
			dropdown.QueueFree();
			box.QueueFree();
			return false;
		}

		return true;
	}

	private bool TryCreateNoAssignedWorldDropdown(SarcFile arc, VBoxContainer parent)
	{
		// Create container
		CreateDropdown(parent, "Misc.", out VBoxContainer box, out Button dropdown);

		// Fill container with file buttons
		var db = ProjectManager.GetDB();
		bool isAnyFilesAdded = false;

		foreach (var file in arc.Content.Keys)
		{
			var world = db.GetWorldInfoByStageName(file.RemoveFileExtension());
			if (world != null)
				continue;
			
			isAnyFilesAdded |= TryCreateButton(arc, file, box);
		}

		if (!isAnyFilesAdded)
		{
			dropdown.QueueFree();
			box.QueueFree();
			return false;
		}

		return true;
	}

	private void CreateDropdown(Control parent, string name, out VBoxContainer box, out Button dropdown)
	{
		var boxMargin = new MarginContainer();
		box = new VBoxContainer();

		boxMargin.AddChild(box);

		dropdown = DropdownButton.New().As<Button>();
		dropdown.Text = name;
		dropdown.Set("dropdown", boxMargin);

		parent.AddChild(dropdown);
		parent.AddChild(boxMargin);
	}

	private bool TryCreateButton(SarcFile file, string key, Container box, WorldInfo world = null)
	{
		// Check if the default language's metadata has an epoch timestamp
		var metaDefaultLang = ProjectManager.GetMSBTMetaHolder();
		bool isDefaultLangAtEpoch = metaDefaultLang.IsLastModifiedTimeAtEpoch(file, key);

		if (IsEnableTranslationFeatures && isDefaultLangAtEpoch)
			return false;

		var button = DoublePressButton.New().As<Button>();
		button.ToggleMode = true;
		button.Name = key;
		button.Text = key;
		button.Alignment = HorizontalAlignment.Left;
		UpdateFileButtonModulation(file, key, button);

		button.TooltipText = file.Name;
		if (world != null)
			button.TooltipText += '\n' + world.Display;

		if (box.IsInsideTree())
			button.FocusNeighborLeft = box.GetPath();

		// These signals are automatically disconnected on free by DoublePressButton gdscript code
		button.Connect("pressed", Callable.From(new Action(() => OnFilePressed(file, key, button))));
		button.Connect("double_pressed", Callable.From(OnFooterOpenFilePressed));

		box.AddChild(button);
		return true;
	}

	#endregion

	#region Signals

	private void OnFilePressed(SarcFile archive, string key, Button button)
	{
		// Remove old selection
		FileListArchives.DeselectAllButtons();

		// Get last modified time
		var meta = ProjectManager.GetMSBTMetaHolder(GetActiveLanguage());
		if (meta == null)
			return;

		var t = meta.GetLastModifiedTime(archive, key);
		bool isEpoch = t.ToFileTimeUtc() == DateTime.UnixEpoch.ToFileTimeUtc();

		// Set current selection
		if (!key.EndsWith(".msbt"))
			key += ".msbt";

		var length = archive.Content[key].Count;
		SelectedFile = archive.GetFileMSBT(key, new MsbtElementFactoryProjectSmo());

		button.SetPressedNoSignal(true);
		button.GrabFocus();
		UpdateFileButtonModulation(archive, key, button);

		FileAccessor.OnFileSelected(SelectedFile);

		// Update info box
		GetNode<Label>("%Label_InfoName").Text = key;

		t = t.ToLocalTime();
		if (!isEpoch)
			GetNode<Label>("%Label_DateTime").Text = t.ToShortDateString() + "\n(" + t.ToLongTimeString() + ')';
		else
			GetNode<Label>("%Label_DateTime").Text = "N/A";

		GetNode<Label>("%Label_Size").Text = ByteSize.FromBytes(length).ToString();
		GetNode<Label>("%Label_EntryCount").Text = SelectedFile.GetEntryCount().ToString();

		var usage = archive.Name;
		if (archive.Name == "StageMessage.szs")
		{
			var world = ProjectManager.GetDB().GetWorldInfoByStageName(key);
			if (world != null)
				usage = world.WorldName;
		}

		GetNode<Label>("%Label_Usage").Text = usage;
	}

	private void OnFooterOpenFilePressed()
	{
		if (SelectedFile == null) return;
		MsbtAppHolder.OpenApp(SelectedFile.Sarc.Name, SelectedFile.Name, GetActiveLanguage());
	}

	private void OnOpenPopupMenuRequested(string nodeName)
	{
		if (SelectedFile == null) return;

		var popup = GetNode<Popup>(nodeName);
		popup.PopupCentered();
		popup.Call("init_data", SelectedFile.Sarc.Name, SelectedFile.Name);
	}

	private void OnLineSearchTextChanged(string txt)
	{
		HomeRoot.RecursiveFileSearch(FileListScroll, txt);
	}

	private void OnEnableTranslationFeatures(bool enabled)
	{
		var isReloadInterface = enabled != IsEnableTranslationFeatures;
		IsEnableTranslationFeatures = enabled;

		EngineSettings.SetSetting("moonflow/localization/translation_features_tab", enabled);

		UpdateTranslationWarning();

		if (isReloadInterface)
			ReloadInterface(true);
	}
	private void OnTranslationLanguageSelected(string lang)
	{
		var isReloadInterface = lang != TranslationLanguage && IsEnableTranslationFeatures;
		TranslationLanguage = lang;

		UpdateTranslationWarning();

		if (isReloadInterface)
		{
			EngineSettings.SetSetting("moonflow/localization/translation_language", lang);
			ReloadInterface(true);
		}
	}

	private static void OnOpenMsbpColorEditor()
	{
		var app = SceneCreator<MsbpColorEditor>.Create();
		app.SetUniqueIdentifier();
		ProjectManager.SceneRoot.NodeApps.AddChild(app);
	}

	private void OnCopyFileHashPressed()
	{
		if (SelectedFile == null)
			return;

		var hash = ProjectLanguageMetaHolder.CalcHash(SelectedFile.Sarc.Name, SelectedFile.Name);
		DisplayServer.ClipboardSet(hash);

		GD.Print(hash + " added to system clipboard!");
	}

	#endregion

	#region Utility

	public void ReloadInterface(bool isRunReady)
	{
		var oldSelection = SelectedFile;
		var oldScroll = FileListScroll.ScrollVertical;

		if (isRunReady)
			_Ready();

		FileListScroll.SetDeferred(ScrollContainer.PropertyName.ScrollVertical, oldScroll);

		if (oldSelection == null)
			return;

		var buttonName = oldSelection.Name.Replace('.', '_');
		if (FileListScroll.FindChild(buttonName, true, false) is Button button)
			OnFilePressed(oldSelection.Sarc, oldSelection.Name, button);
	}

	private void UpdateFileButtonModulation(SarcFile file, string key, Button button)
	{
		var meta = ProjectManager.GetMSBTMetaHolder(GetActiveLanguage());
		if (meta == null)
			return;

		bool isEpoch = meta.IsLastModifiedTimeAtEpoch(file, key);

		if (isEpoch) button.SelfModulate = Colors.Gray;
		else button.SelfModulate = Colors.White;
	}

	private async void HideDropdownIfEmpty(VBoxContainer box)
	{
		if (box.GetParent() is not MarginContainer parent)
			throw new Exception("Invalid parent for box!");

		if (box.GetChildCount() == 0)
		{
			if (!parent.HasMeta("dropdown"))
				return;
			
			await ToSignal(Engine.GetMainLoop(), "process_frame");
			
			var dropdown = parent.GetMeta("dropdown").As<Button>();
			dropdown.Hide();
		}
	}

	private bool UpdateTranslationWarning()
	{
		var lang = TranslationLanguage;
		var defaultLang = ProjectManager.GetDefaultLang();

		bool isWarn = IsEnableTranslationFeatures && lang == defaultLang;

		TranslationLanguageWarning.Visible = isWarn;
		FileListScroll.Visible = !isWarn;

		return !isWarn;
	}

	public static string GetFileName(string name)
	{
		if (name.EndsWith(".msbt"))
			return name;

		return name + ".msbt";
	}

	public string GetActiveLanguage()
	{
		var lang = ProjectManager.GetDefaultLang();
		if (IsEnableTranslationFeatures)
			lang = TranslationLanguage;

		return lang;
	}

	#endregion
}
