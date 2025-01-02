using Godot;
using System;

using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;

using MoonFlow.Project.Database;
using MoonFlow.Project;
using MoonFlow.Ext;
using MoonFlow.Scene.EditorMsbt;

namespace MoonFlow.Scene.EditorWorld;

[ScenePath("res://scene/editor/world/shine/world_shine_editor_holder.tscn")]
public partial class WorldShineEditorHolder : PanelContainer
{
	public WorldInfo World { get; private set; } = null;
	public ShineInfo Shine { get; private set; } = null;

	private ScrollContainer ParentScroll;

	[Export, ExportGroup("Internal References")]
	private WorldShineEditor Editor;
	[Export]
	private RichTextLabel LabelShineName;
	[Export]
	private Label LabelStageName;

	[Export]
	private TextureRect IconType;
	[Export]
	private TextureRect IconGrand;

	[Export]
	private Button ButtonCreateDisplayName;
	[Export]
	private Button ButtonOpenDisplayName;

	[Export]
	private SpinBox SpinIndex;

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

		Editor.Connect(WorldShineEditor.SignalName.ContentModified, Callable.From(OnEditorModifiedContent));
		Editor.Connect(SignalName.VisibilityChanged, Callable.From(OnVisibilityChanged));
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

		// Setup editor
		Editor.InitEditor(world, shine);
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
		
		// Check if the requested shine label exists
		MsbtAppHolder editor;
		if (msbtHolder.GetFileMSBT(file, new MsbtElementFactory()).IsContainKey(label))
		{
			editor = MsbtAppHolder.OpenAppWithSearch("StageMessage.szs", file, label);
			editor.Editor.SetSelection(label);
			return;
		}

		editor = MsbtAppHolder.OpenApp("StageMessage.szs", file);
		await ToSignal(Engine.GetMainLoop(), "process_frame");
		
		editor.Editor.OnAddEntryNameSubmitted(label);
		editor.Editor.UpdateEntrySearch(label);
	}

	private void OnVisibilityChanged()
	{
		var display = Shine.LookupDisplayName();
		UpdateDisplayName(display?.GetRawText());
	}

	#endregion

	#region Utility

	public void OnEditorModifiedContent()
	{
		var display = Shine.LookupDisplayName();
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
		Editor.UpdateUniquenessWarnings();
	}

	#endregion
}
