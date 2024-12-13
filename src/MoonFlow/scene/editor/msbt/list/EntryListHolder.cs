using Godot;
using System;

namespace MoonFlow.Scene.EditorMsbt;

public partial class EntryListHolder : VBoxContainer
{
	public MsbtEditor Editor { get; private set; } = null;
	public EntryListBase EntryList { get; private set; } = null;

	private static readonly GDScript SmoothScroll
		= GD.Load<GDScript>("res://addons/SmoothScroll/SmoothScrollContainer.gd");

	[Signal]
	public delegate void CreateEntryEventHandler(string label);
	[Signal]
	public delegate void DeleteEntryEventHandler();
	[Signal]
	public delegate void OpenHelpPageEventHandler();

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

	private void UpdateSearch(string str)
	{
		EntryList.UpdateSearch(str);
	}

	private void RequestDeleteEntry()
	{
		var selection = EntryList.EntryListSelection;
		if (!IsInstanceValid(selection))
			return;

		EmitSignal(SignalName.DeleteEntry);
	}
}
