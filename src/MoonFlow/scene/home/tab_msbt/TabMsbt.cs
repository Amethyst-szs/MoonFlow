using Godot;
using System;

using Nindot;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib.Smo;

using MoonFlow.Project;
using MoonFlow.Scene.EditorMsbt;

namespace MoonFlow.Scene.Home;

public partial class TabMsbt : HSplitContainer
{
	private SarcMsbtFile SelectedFile = null;

	private VBoxContainer SystemMessageButtons = null;
	private VBoxContainer StageMessageButtons = null;
	private VBoxContainer LayoutMessageButtons = null;

	private GDScript DoublePressButton = GD.Load<GDScript>("res://addons/ui_node_ext/double_click_button.gd");

	public override void _Ready()
	{
		SystemMessageButtons = GetNode<VBoxContainer>("%SystemMessage_VBox");
		StageMessageButtons = GetNode<VBoxContainer>("%StageMessage_VBox");
		LayoutMessageButtons = GetNode<VBoxContainer>("%LayoutMessage_VBox");

		// Setup vbox buttons
		var archives = ProjectManager.GetMSBTArchives();
		SetupVBox(SystemMessageButtons, archives.SystemMessage);
		SetupVBox(StageMessageButtons, archives.StageMessage);
		SetupVBox(LayoutMessageButtons, archives.LayoutMessage);
	}

	private void SetupVBox(VBoxContainer box, SarcFile file)
	{
		string[] keys = [.. file.Content.Keys];
		Array.Sort(keys);

		var pathToBox = box.GetPath();

		foreach (var key in keys)
		{
			var button = DoublePressButton.New().As<Button>();
			button.Text = key;
			button.Alignment = HorizontalAlignment.Left;
			button.FocusNeighborLeft = pathToBox;

			// These signals are automatically disconnected on free by DoublePressButton gdscript code
			button.Connect("pressed", Callable.From(new Action(() => OnFilePressed(file, key))));
			button.Connect("double_pressed", Callable.From(new Action(() => OnFileOpened(file.Name, key))));

			box.AddChild(button);
		}
	}

	private void OnFilePressed(SarcFile archive, string key)
	{
		SelectedFile = archive.GetFileMSBT(key, new MsbtElementFactoryProjectSmo());
	}

	private static void OnFileOpened(string archiveName, string key)
	{
		var editor = SceneCreator<MsbtAppHolder>.Create();
		editor.SetUniqueIdentifier(archiveName + key);
		ProjectManager.SceneRoot.NodeApps.AddChild(editor);

		var msbp = ProjectManager.GetMSBP();
		var defaultLang = ProjectManager.GetProject().Config.Data.DefaultLanguage;
		editor.SetupEditor(msbp, defaultLang, archiveName, key);
	}
}
