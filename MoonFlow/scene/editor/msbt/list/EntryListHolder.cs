using Godot;
using Godot.Collections;
using MoonFlow.Project;

namespace MoonFlow.Scene.EditorMsbt;

public partial class EntryListHolder : VBoxContainer
{
	public MsbtEditor Editor { get; private set; } = null;
	public EntryListBase EntryList { get; private set; } = null;

	private static readonly GDScript SmoothScroll
		= GD.Load<GDScript>("res://addons/SmoothScroll/SmoothScrollContainer.gd");

	[Export]
	public LineEdit SearchBoxLine { get; private set; }
	[Export]
	public Button ButtonResetEntry { get; private set; }
	[Export]
	public Array<Button> ButtonListBlockedInTranslateMode { get; private set; } = [];

	[Signal]
	public delegate void CreateEntryEventHandler(string label);
	[Signal]
	public delegate void DeleteEntryEventHandler();
	[Signal]
	public delegate void ResetEntryEventHandler();

	public void SetupList<TList>() where TList : EntryListBase, new()
	{
		// Get access to editor
		Editor = GetNode<MsbtEditor>("%MsbtEditor");

		// Destroy scrollbox children
		foreach (var child in GetChildren())
		{
			if (child is not ScrollContainer)
				continue;

			RemoveChild(child);
			child.QueueFree();
		}

		// Create entry list scrollbox
		var scroll = new ScrollContainer()
		{
			FollowFocus = true,
			HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,

			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ExpandFill,
		};

		scroll.SetScript(SmoothScroll);
		scroll.Set("drag_with_mouse", false);
		scroll.Set("follow_focus_margin", 140);

		scroll.Set("allow_horizontal_scroll", false);
		scroll.Set("force_vertical_scrolling", true);

		// Create entry list
		EntryList = new TList()
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ExpandFill,
			Editor = Editor
		};

		// Attach to tree
		AddChild(scroll);
		MoveChild(scroll, 0);
		scroll.AddChild(EntryList);
	}

	public void UpdateToolButtonRestrictions()
	{
		foreach (var button in ButtonListBlockedInTranslateMode)
			button.Disabled = Editor.CurrentLanguage != Editor.DefaultLanguage;
	}

	#region Signals

	public void OnEntrySelectedOrModified(string label)
	{
		// Fetch label metadata for current lang
		var metaH = ProjectManager.GetMSBTMetaHolder(Editor.CurrentLanguage);
		var meta = metaH.GetMetadata(Editor.File, label);

		// Toggle button activeness based on data
		ButtonResetEntry.Disabled = !meta.Mod || meta.Custom;
	}

	private void OnUpdateSearch(string str)
	{
		EntryList.UpdateSearch(str);
	}

	private void OnRequestDeleteEntry()
	{
		var selection = EntryList.EntryListSelection;
		if (!IsInstanceValid(selection))
			return;

		EmitSignal(SignalName.DeleteEntry);
	}
	private void OnRequestResetEntry()
	{
		var selection = EntryList.EntryListSelection;
		if (!IsInstanceValid(selection))
			return;

		EmitSignal(SignalName.ResetEntry);
	}

	#endregion
}
