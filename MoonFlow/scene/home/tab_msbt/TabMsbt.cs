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
	private ScrollContainer FileListRoot = null;

	[Export]
	private VBoxContainer SystemMessageButtons = null;
	[Export]
	private VBoxContainer StageMessageVBox = null;
	[Export]
	private VBoxContainer LayoutMessageButtons = null;

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
		StageMessageVBox.QueueFreeAllChildren();

		// Setup vbox buttons for system and layout
		ShowDropdown(SystemMessageButtons);
		ShowDropdown(LayoutMessageButtons);

		var archives = ProjectManager.GetMSBTArchives();
		SetupGenericVBox(SystemMessageButtons, archives.SystemMessage);
		SetupGenericVBox(LayoutMessageButtons, archives.LayoutMessage);

		// Setup buttons for stage messages
		ShowDropdown(StageMessageVBox);

		var db = ProjectManager.GetProject().Database;
		var stageFiles = archives.StageMessage.Content.Keys.ToList();

		foreach (var world in db.WorldList)
		{
			var content = SetupWorldVBox(world, archives.StageMessage);
			foreach (var item in content)
				stageFiles.Remove(item);
		}

		SetupWorldVBoxWithoutWorld(stageFiles, archives.StageMessage);
		HideDropdownIfEmpty(StageMessageVBox);
	}

	private void SetupGenericVBox(VBoxContainer box, SarcFile file)
	{
		box.QueueFreeAllChildren();

		string[] keys = [.. file.Content.Keys];
		Array.Sort(keys);

		foreach (var key in keys)
			CreateButton(file, key, box);
		
		// If no buttons were created, hide the dropdown
		HideDropdownIfEmpty(box);

		if (file.Name == "SystemMessage.szs")
			OnFilePressed(file, keys[0], box.GetChild(0) as Button);
	}

	private List<string> SetupWorldVBox(WorldInfo world, SarcFile arc)
	{
		// Output stage name list
		List<string> result = [];

		// Create new container
		var worldBoxMargin = new MarginContainer();
		var worldBox = new VBoxContainer();

		// Fill container with buttons
		var prevCategory = StageInfo.CatEnum.MainStage;

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
					worldBox.AddChild(new HSeparator());
			}

			CreateButton(arc, key, worldBox, world);
			result.Add(key);
		}

		// Create dropdown button
		var dropdown = DropdownButton.New().As<Button>();
		dropdown.Text = world.Display;
		dropdown.Set("dropdown", worldBoxMargin);

		// Add children
		StageMessageVBox.AddChild(dropdown);
		StageMessageVBox.AddChild(worldBoxMargin);
		worldBoxMargin.AddChild(worldBox);

		// If no buttons were created, hide the dropdown
		HideDropdownIfEmpty(worldBox);

		return result;
	}

	private void SetupWorldVBoxWithoutWorld(List<string> files, SarcFile arc)
	{
		if (files.Count == 0)
			return;

		// Create new container
		var worldBoxMargin = new MarginContainer();
		var worldBox = new VBoxContainer();

		// Fill container with buttons
		foreach (var file in files)
			CreateButton(arc, file, worldBox);

		// Create dropdown button
		var dropdown = DropdownButton.New().As<Button>();
		dropdown.Text = "Misc.";
		dropdown.Set("dropdown", worldBoxMargin);

		// Add children
		StageMessageVBox.AddChild(dropdown);
		StageMessageVBox.AddChild(worldBoxMargin);
		worldBoxMargin.AddChild(worldBox);

		// If no buttons were created, hide the dropdown
		HideDropdownIfEmpty(worldBox);
	}

	private void CreateButton(SarcFile file, string key, Container box, WorldInfo world = null)
	{
		// Check if the default language's metadata has an epoch timestamp
		var metaDefaultLang = ProjectManager.GetMSBTMetaHolder();
		bool isDefaultLangAtEpoch = metaDefaultLang.IsLastModifiedTimeAtEpoch(file, key);

		if (IsEnableTranslationFeatures && isDefaultLangAtEpoch)
			return;

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
		button.Connect("double_pressed",
			Callable.From(new Action(() => MsbtAppHolder.OpenApp(file.Name, key))));

		box.AddChild(button);
	}

	#endregion

	#region Signals

	private void OnFilePressed(SarcFile archive, string key, Button button)
	{
		// Remove old selection
		SystemMessageButtons.DeselectAllButtons();
		LayoutMessageButtons.DeselectAllButtons();
		StageMessageVBox.DeselectAllButtons();

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
		MsbtAppHolder.OpenApp(SelectedFile.Sarc.Name, SelectedFile.Name);
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
		HomeRoot.RecursiveFileSearch(FileListRoot, txt);
	}

	private void OnEnableTranslationFeatures(bool enabled)
	{
		var isReloadInterface = enabled != IsEnableTranslationFeatures;
		IsEnableTranslationFeatures = enabled;

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
			ReloadInterface(true);
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
		var oldScroll = FileListRoot.ScrollVertical;

		if (isRunReady)
			_Ready();

		FileListRoot.SetDeferred(ScrollContainer.PropertyName.ScrollVertical, oldScroll);

		if (oldSelection == null)
			return;

		var buttonName = oldSelection.Name.Replace('.', '_');
		if (FileListRoot.FindChild(buttonName, true, false) is Button button)
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

	private static void ShowDropdown(VBoxContainer box)
	{
		if (box.GetParent() is not MarginContainer parent)
			throw new Exception("Invalid parent for box!");
		
		parent.Show();
		return;
	}

	private void HideDropdownIfEmpty(VBoxContainer box)
	{
		if (box.GetParent() is not MarginContainer parent)
			throw new Exception("Invalid parent for box!");
		
		if (box.GetChildCount() == 0 || (box == StageMessageVBox && !box.IsAnyChildVisible()))
		{
			parent.Hide();
			if (parent.HasMeta("dropdown"))
				parent.GetMeta("dropdown").As<Button>().Hide();
			
			return;
		}
	}

	private bool UpdateTranslationWarning()
	{
		var lang = TranslationLanguage;
		var defaultLang = ProjectManager.GetDefaultLang();

		bool isWarn = IsEnableTranslationFeatures && lang == defaultLang;

		TranslationLanguageWarning.Visible = isWarn;
		FileListRoot.Visible = !isWarn;

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
