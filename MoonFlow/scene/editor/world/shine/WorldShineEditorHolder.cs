using Godot;
using System;

using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;

using MoonFlow.Project.Database;
using MoonFlow.Project;
using MoonFlow.Scene.EditorMsbt;

namespace MoonFlow.Scene.EditorWorld;

[ScenePath("res://scene/editor/world/shine/world_shine_editor_holder.tscn")]
public partial class WorldShineEditorHolder : PanelContainer
{
	public WorldInfo World { get; private set; } = null;
	public ShineInfo Shine { get; private set; } = null;

	private ScrollContainer ParentScroll;
	private WorldShineEditor Editor;

	[Export, ExportGroup("Internal References")]
	private RichTextLabel LabelShineName;
	[Export]
	private Label LabelStageName;
	
	[Export, ExportSubgroup("Icon Displays")]
	private TextureRect IconType;
	[Export]
	private TextureRect IconGrand;

	[Export, ExportSubgroup("Display Name Buttons")]
	private Button ButtonCreateDisplayName;
	[Export]
	private Button ButtonOpenDisplayName;

	[Export, ExportSubgroup("Index Modification")]
	private SpinBox SpinIndex;

	[Export, ExportSubgroup("Content Editor")]
	private Button ButtonDropdown;
	[Export]
	private VBoxContainer ContentContainer;
	[Export]
	private PackedScene DropdownContentScene;

	[Export, ExportSubgroup("Popups")]
	private ConfirmationDialog DialogConfirmDelete;

	[Signal]
	public delegate void ContentModifiedEventHandler();

	private static readonly Texture2D TextureIconAchievement = GD.Load<Texture2D>(
		"res://asset/nindot/lms/icon/PictureFont_31.png"
	);
	private static readonly Texture2D TextureIconMoonRock = GD.Load<Texture2D>(
		"res://asset/nindot/lms/icon/PictureFont_30.png"
	);

	public override void _Ready()
	{
		ParentScroll = this.FindParentByType<ScrollContainer>();
	}

	public void SetupShineEditor(WorldInfo world, ShineInfo shine, MsbtEntry displayName, int idx)
	{
		// Copy references
		World = world;
		Shine = shine;

		// Setup visual display
		UpdateDisplayName(displayName?.GetRawText());
		LabelStageName.Text = shine.StageName;

		// Setup type icons
		if (shine.IsAchievement) IconType.Texture = TextureIconAchievement;
		else if (shine.IsMoonRock) IconType.Texture = TextureIconMoonRock;
		else IconType.Texture = null;

		IconGrand.Visible = shine.IsGrand;

		// Setup display name buttons
		var msbtHolder = ProjectManager.GetMSBTArchives()?.StageMessage
		?? throw new NullReferenceException("Could not access msbt archives!");

		if (displayName != null)
		{
			ButtonCreateDisplayName.Hide();
			ButtonOpenDisplayName.Show();
		}
		else if (msbtHolder.Content.ContainsKey(shine.StageName + ".msbt"))
		{
			ButtonCreateDisplayName.Show();
			ButtonOpenDisplayName.Hide();
		}
		else
		{
			ButtonCreateDisplayName.Hide();
			ButtonOpenDisplayName.Hide();
		}

		// Setup index spinner
		SpinIndex.MinValue = 0;
		SpinIndex.MaxValue = world.ShineList.Count - 1;
		SpinIndex.SetValueNoSignal(idx);
	}

	#region Signals

	private void OnSpinIndexValueSet(float idxF)
	{
		var idx = (int)MathF.Floor(idxF);

		World.ShineList.Remove(Shine);
		World.ShineList.Insert(idx, Shine);

		GetParent().MoveChild(this, idx);
		ParentScroll.CallDeferred(ScrollContainer.MethodName.EnsureControlVisible, this);

		EmitSignal(SignalName.ContentModified);
	}

	private async void OnOpenDisplayNameMsbt()
	{
		// Access msbt holder
		var msbtHolder = ProjectManager.GetMSBTArchives()?.StageMessage
		?? throw new NullReferenceException("Could not access msbt archives!");

		// If msbt holder doesn't contain msbt file, cancel
		var file = Shine.StageName + ".msbt";
		var label = "ScenarioName_" + Shine.ObjId;

		if (!msbtHolder.Content.ContainsKey(file))
			return;

		// Lookup msbt editor application
		var apps = AppSceneServer.GetApps<MsbtAppHolder>();

		MsbtAppHolder app = null;
		var targetIdentifier = MsbtAppHolder.GetUniqueIdentifier(msbtHolder.Name, file);

		foreach (var candidate in apps)
		{
			if (candidate.AppUniqueIdentifier != targetIdentifier)
				continue;

			app = candidate;
			break;
		}

		// Check if the requested shine label exists
		MsbtAppHolder editor;
		if (msbtHolder.GetFileMSBT(file, new MsbtElementFactory()).IsContainKey(label))
		{
			// If the pre-existing msbt app lookup failed, create a new editor app
			if (app == null)
			{
				editor = await MsbtAppHolder.OpenAppWithSearch(msbtHolder.Name, file, label);
				editor.Editor.SetSelection(label);
				return;
			}

			app.Editor.UpdateEntrySearch(label);
			app.Editor.SetSelection(label);
			app.AppFocus();

			return;
		}

		// If the label doesn't already exist, we'll need to create it
		app ??= await MsbtAppHolder.OpenApp(msbtHolder.Name, file);

		app.Editor.OnAddEntryNameSubmitted(label);
		app.Editor.UpdateEntrySearch(label);
		app.AppFocus();
	}

	private void OnRequestDeleteShine()
	{
		if (Input.IsKeyLabelPressed(Key.Ctrl) || Input.IsKeyLabelPressed(Key.Meta))
		{
			OnDeleteShine();
			return;
		}

		DialogConfirmDelete.Popup();
	}

	private void OnDeleteShine()
	{
		EmitSignal(SignalName.ContentModified);
		World.ShineList.Remove(Shine);

		GetParent().RemoveChild(this);
		QueueFree();
	}

	private void OnVisibilityChanged()
	{
		var stageMessage = ProjectManager.GetMSBTArchives()?.StageMessage
		?? throw new NullReferenceException("Could not get StageMessage!");

		var display = Shine.LookupDisplayName(stageMessage);
		UpdateDisplayName(display?.GetRawText());
	}

	private void OnContentDropdownToggled(bool isOpen)
	{
		if (Editor != null || !isOpen)
			return;
		
		Editor = DropdownContentScene.Instantiate<WorldShineEditor>();
		Editor.InitEditor(World, Shine);

		ContentContainer.AddChild(Editor);

		Editor.Connect(WorldShineEditor.SignalName.ContentModified, Callable.From(OnEditorModifiedContent));
		Editor.Connect(SignalName.VisibilityChanged, Callable.From(OnVisibilityChanged));

		ButtonDropdown.Set("dropdown", Editor);
	}

	#endregion

	#region Utility

	public void OnEditorModifiedContent()
	{
		var stageMessage = ProjectManager.GetMSBTArchives()?.StageMessage
		?? throw new NullReferenceException("Could not get StageMessage!");

		var display = Shine.LookupDisplayName(stageMessage);
		SetupShineEditor(World, Shine, display, World.ShineList.IndexOf(Shine));

		EmitSignal(SignalName.ContentModified);
	}

	private void UpdateDisplayName(string displayName)
	{
		LabelShineName.Text = string.Empty;

		if (displayName != null)
		{
			LabelShineName.AddText(displayName);
			LabelShineName.PushItalics();
			LabelShineName.PushColor(Colors.Gray);
		}

		LabelShineName.AddText(string.Format(" ({0})", Shine.ObjId));
	}

	public void UpdateShineIndex()
	{
		SpinIndex.SetValueNoSignal(GetIndex());
	}
	public void UpdateShineUniqueness()
	{
		Editor?.UpdateUniquenessWarnings();
	}

	#endregion
}
