using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

using Nindot;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib.Smo;

using MoonFlow.Project;
using MoonFlow.Project.FTP;
using MoonFlow.Scene.EditorMsbt;
using MoonFlow.Project.Database;

using ByteSizeLib;

namespace MoonFlow.Scene.Home;

[SceneUid("uid://dgh2onvx6k4ft")]
public partial class TabMsbt : HSplitContainer
{
	#region Properties

	// ~~~~~~~~~~~~~~~ Contents ~~~~~~~~~~~~~~ //

	public SarcMsbtFile SelectedFile { get; private set; } = null;

	[Export]
	public bool IsEnableTranslationFeatures { get; private set; } = false;
	public string TranslationLanguage { get; private set; } = "USen";

	// ~~~~~~~ Searching and Filtering ~~~~~~~ //

	private string FileSearchString = "";
	private bool FileSearchIsModifiedOnly = false;
	private bool FileSearchIsCustomOnly = false;
	private bool FileListIsUseOrganization = true;

	// ~~~~~~~~~ Internal References ~~~~~~~~~ //

	[Export, ExportGroup("Internal References")]
	private ScrollContainer FileListScroll = null;
	[Export]
	private VBoxContainer FileListArchives = null;

	[Export]
	private Label TranslationLanguageWarning = null;

	[Export]
	private BoxContainer FooterSourceText = null;
	[Export]
	private BoxContainer FooterTranslation = null;

	[Export]
	private MenuButton ButtonFileListFilter = null;
	[Export]
	private FileListSortMenu ButtonFileListSort = null;
	[Export]
	private Button ButtonFileListOrganization = null;

	[Export]
	private TabMsbtFileAccessor FileAccessor = null;

	// ~~~~~~~~~~~~~~~ Scripts ~~~~~~~~~~~~~~~ //

	private GDScript DropdownButton = GD.Load<GDScript>("res://scene/common/button/dropdown_checkbox.gd");

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

		// Setup footer based on type
		FooterSourceText.Visible = !IsEnableTranslationFeatures;
		FooterTranslation.Visible = IsEnableTranslationFeatures;

		// Setup file list config buttons and filter/sort state
		FileSearchIsModifiedOnly = EngineSettings.GetSetting<bool>("moonflow/home_msbt/is_filter_modified", false);
		FileSearchIsCustomOnly = EngineSettings.GetSetting<bool>("moonflow/home_msbt/is_filter_custom", false);
		FileListIsUseOrganization = EngineSettings.GetSetting<bool>("moonflow/home_msbt/is_use_organize", true);
		var sortMode = EngineSettings.GetSetting<FileListSortMenu.SortMode>("moonflow/home_msbt/sort_mode", 0);
		
		ButtonFileListFilter.SetPressedNoSignal(FileSearchIsModifiedOnly || FileSearchIsCustomOnly);
		ButtonFileListFilter.GetPopup().SetItemChecked(0, FileSearchIsModifiedOnly);
		ButtonFileListFilter.GetPopup().SetItemChecked(1, FileSearchIsCustomOnly);
		ButtonFileListSort.SetCurrentSortModeInMenu((int)sortMode);
		ButtonFileListOrganization.SetPressedNoSignal(FileListIsUseOrganization);

		if (FileSearchIsModifiedOnly || FileSearchIsCustomOnly || !FileListIsUseOrganization)
			UpdateFileListSearch();
		if (sortMode != FileListSortMenu.SortMode.Default)
			OnSortModeChanged(sortMode);
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

		parent.MoveChild(box.GetParent(), 0);
		parent.MoveChild(dropdown, 0);

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

		var button = new MsbtFileListButton();
		button.InitButton(file, key, box, world, GetActiveLanguage());

		// These signals are automatically disconnected on free by DoublePressButton gdscript code
		button.Connect(BaseButton.SignalName.Pressed, Callable.From(new Action(() => OnFilePressed(file, key, button))));
		button.Connect(MsbtFileListButton.SignalName.DoublePressed, Callable.From(OnFooterOpenFilePressed));

		box.AddChild(button);
		return true;
	}

	#endregion

	#region Signals

	private void OnFilePressed(SarcFile archive, string key, Button button)
	{
		// Remove old selection
		FileListArchives.DeselectAllButtonsOfClass<MsbtFileListButton>();

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

	private async void OnFooterOpenFilePressed()
	{
		if (SelectedFile == null) return;
		await MsbtAppHolder.OpenApp(SelectedFile.Sarc.Name, SelectedFile.Name, GetActiveLanguage());
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
		FileSearchString = txt;
		UpdateFileListSearch();
	}
	private void OnFileFilterPropertyIsModifiedOnlyChanged(bool state)
	{
		FileSearchIsModifiedOnly = state;
		EngineSettings.SetSetting("moonflow/home_msbt/is_filter_modified", state);
		UpdateFileListSearch();
	}
	private void OnFileFilterPropertyIsCustomOnlyChanged(bool state)
	{
		FileSearchIsCustomOnly = state;
		EngineSettings.SetSetting("moonflow/home_msbt/is_filter_custom", state);
		UpdateFileListSearch();
	}
	private void OnSortModeChanged(FileListSortMenu.SortMode mode)
	{
		EngineSettings.SetSetting("moonflow/home_msbt/sort_mode", (int)mode);

		if (mode == FileListSortMenu.SortMode.Default)
		{
			ReloadInterface(true);
			return;
		}

		UpdateFileListSort(FileListArchives, mode);
	}
	private void OnCheckUseOrganizationToggled(bool state)
	{
		FileListIsUseOrganization = state;
		EngineSettings.SetSetting("moonflow/home_msbt/is_use_organize", state);
		UpdateFileListSearch();
	}

	private void OnTranslationLanguageSelected(string lang)
	{
		// Update configuration
		var isReloadInterface = lang != TranslationLanguage && IsEnableTranslationFeatures;
		TranslationLanguage = lang;

		EngineSettings.SetSetting("moonflow/localization/translation_language", lang);

		bool isTransferAll = ProjectFtpClient.CredentialStore.IsTransferAllLanguages;
		ProjectFtpClient.UpdateLanguageConfiguration(ProjectManager.GetDefaultLang(), lang, isTransferAll);

		// Update interface
		UpdateTranslationWarning();

		if (isReloadInterface)
			ReloadInterface(true);
	}

	private void OnCopyFileHashPressed()
	{
		if (SelectedFile == null)
			return;

		var hash = ProjectLanguageMetaFile.CalcHash(SelectedFile.Sarc.Name, SelectedFile.Name);
		DisplayServer.ClipboardSet(hash);

		GD.Print(hash + " added to system clipboard!");
	}

	#endregion

	#region Search & Filter

	public void UpdateFileListSearch()
	{
		UpdateFileListSearchLayer(FileListArchives);
	}
	private void UpdateFileListSearchLayer(Control root)
	{
		if (root is MsbtFileListButton listButton)
		{
			listButton.Visible = IsNodeAllowedBySearch(listButton);
			return;
		}

		if (root is Button button && root.GetScript().As<Script>() == DropdownButton)
		{
			var dropdownChild = button.Get("dropdown").As<Control>();
			if (dropdownChild == null)
				return;

			UpdateFileListSearchLayer(dropdownChild);

			if (!FileListIsUseOrganization)
			{
				button.Visible = false;
				button.SetPressedNoSignal(false);
				dropdownChild.Visible = HomeRoot.IsAnyChildVisible<MsbtFileListButton>(dropdownChild);
				return;
			}

			if (IsFileListUsingSearchOrFilter())
			{
				button.Visible = HomeRoot.IsAnyChildVisible<MsbtFileListButton>(dropdownChild);
				button.SetPressedNoSignal(button.Visible);
				dropdownChild.Visible = button.Visible;
			}
			else
			{
				button.Visible = true;
				button.SetPressedNoSignal(false);
				dropdownChild.Visible = false;
			}

			return;
		}

		if (root is Container)
		{
			// if (IsFileListUsingSearchOrFilter())
			// 	HomeRoot.SetVisibleIfAnyChildVisible<MsbtFileListButton>(root);

			foreach (var child in root.GetChildren())
				if (child.GetType().IsSubclassOf(typeof(Control)))
					UpdateFileListSearchLayer(child as Control);

			return;
		}

		if (root is HSeparator)
		{
			root.Visible = !IsFileListUsingSearchOrFilter() && FileListIsUseOrganization;
			return;
		}
	}

	private void UpdateFileListSort(Node root, FileListSortMenu.SortMode mode)
	{
		if (root is HSeparator hsep)
		{
			hsep.Hide();
			return;
		}

		if (root.GetChildCount() == 0)
			return;

		switch (mode)
		{
			case FileListSortMenu.SortMode.Alphabet:
				SortFileListItemsByAlphabetical(root, false);
				break;
			case FileListSortMenu.SortMode.AlphabetReverse:
				SortFileListItemsByAlphabetical(root, true);
				break;
			case FileListSortMenu.SortMode.LastModified:
				SortFileListItemsByLastModified(root, GetActiveLanguage(), false);
				break;
			case FileListSortMenu.SortMode.LastModifiedReverse:
				SortFileListItemsByLastModified(root, GetActiveLanguage(), true);
				break;
			case FileListSortMenu.SortMode.Size:
				SortFileListItemsBySize(root, false);
				break;
			case FileListSortMenu.SortMode.SizeReverse:
				SortFileListItemsBySize(root, true);
				break;
		}

		foreach (var child in root.GetChildren())
			if (child.GetType().IsSubclassOf(typeof(Control)))
				UpdateFileListSort(child as Control, mode);
	}

	private static void SortFileListItemsByAlphabetical(Node root, bool isReverse)
	{
		var children = root.GetChildren().Where((node) => node is MsbtFileListButton).Cast<MsbtFileListButton>().ToList();
		children.Sort((a, b) => string.Compare(a.Name, b.Name) * (isReverse ? -1 : 1));

		for (int i = 0; i < children.Count; i++)
			root.MoveChild(children[i], i);
	}
	private static void SortFileListItemsByLastModified(Node root, string activeLang, bool isReverse)
	{
		var children = root.GetChildren().Where((node) => node is MsbtFileListButton).Cast<MsbtFileListButton>().ToList();
		children.Sort((a, b) =>
		{
			var aDate = a.GetLastModifiedTime(activeLang);
			var bDate = b.GetLastModifiedTime(activeLang);
			var result = bDate.CompareTo(aDate) * (isReverse ? -1 : 1);
			if (result == 0)
				return string.Compare(a.Name, b.Name);

			return result;
		});

		for (int i = 0; i < children.Count; i++)
			root.MoveChild(children[i], i);
	}
	private static void SortFileListItemsBySize(Node root, bool isReverse)
	{
		var children = root.GetChildren().Where((node) => node is MsbtFileListButton).Cast<MsbtFileListButton>().ToList();
		children.Sort((a, b) =>
		{
			var result = b.GetFileSize().CompareTo(a.GetFileSize()) * (isReverse ? -1 : 1);
			if (result == 0)
				return string.Compare(a.Name, b.Name);

			return result;
		});

		for (int i = 0; i < children.Count; i++)
			root.MoveChild(children[i], i);
	}

	private bool IsNodeAllowedBySearch(MsbtFileListButton node)
	{
		if (FileSearchIsModifiedOnly && node.IsDateAtUnixEpoch(GetActiveLanguage()))
			return false;

		if (FileSearchIsCustomOnly)
		{
			var arcs = ProjectManager.GetMSBTArchives(GetActiveLanguage());
			if (arcs.IsMsbtFileInBaseRomfs(node.FileKey))
				return false;
		}

		return node.FileKey.Contains(FileSearchString, StringComparison.OrdinalIgnoreCase);
	}
	private bool IsFileListUsingSearchOrFilter()
	{
		return FileSearchString != string.Empty || FileSearchIsModifiedOnly || FileSearchIsCustomOnly;
	}

	#endregion

	#region Utility

	public void ReloadInterface(bool isRunReady)
	{
		var oldSelection = SelectedFile;

		if (isRunReady)
			_Ready();

		if (oldSelection == null)
			return;

		var buttonName = oldSelection.Name.ToNodeName();
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

			await Extension.WaitProcessFrame(this);

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
