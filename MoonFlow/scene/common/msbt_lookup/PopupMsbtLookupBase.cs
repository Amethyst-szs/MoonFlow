using FuzzySharp;
using Godot;
using MoonFlow.Project;
using MoonFlow.Project.Cache;
using System;
using System.Collections.Generic;

namespace MoonFlow.Scene;

public abstract partial class PopupMsbtLookupBase : Window
{
	[Export]
	protected bool IsDisplayWithoutSearch = false;
	[Export]
	protected int MaxResults = 300;
	[Export]
	protected float RefreshWaitSeconds = 0.2f;

	[Export, ExportGroup("Internal References")]
	protected LineEdit LineSearch;
	[Export]
	protected ScrollContainer ResultScroll;
	[Export]
	protected VBoxContainer ResultList;

	protected static string SearchLast = "";

	private Timer InputTimer;

	[Signal]
	public delegate void ItemSelectedEventHandler(string arc, string file, string label);

	public override void _Ready()
	{
		InputTimer = new Timer
		{
			WaitTime = RefreshWaitSeconds,
			OneShot = true,
		};

		InputTimer.Timeout += UpdateListing;
		AddChild(InputTimer);

		AboutToPopup += OnPopupReady;

		Hide();
		SetupSearchBoxFromSearchLast();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel"))
			QueueFree();
	}
	public override void _Notification(int what)
	{
		if (what == NotificationWMCloseRequest)
			QueueFree();
	}

	private void SetupSearchBoxFromSearchLast()
	{
		if (SearchLast == string.Empty)
			return;
		
		LineSearch.SetDeferred(LineEdit.PropertyName.Text, SearchLast);
		LineSearch.SetDeferred(LineEdit.PropertyName.CaretColumn, SearchLast.Length);
		CallDeferred(MethodName.UpdateListing);
	}

	protected abstract void UpdateListing();
	protected abstract void HandleItemSelect(ProjectLabelCache.LabelLookupResult item);

	#region Signals

	private void OnPopupReady()
	{
		// Setup search box
		LineSearch.Text = "";
		LineSearch.GrabFocus();

		// Clear result list content
		ResultList.QueueFreeAllChildren();

		// Set window size
		var size = GetTree().CurrentScene.GetWindow().Size;
		size.X /= 2;
		size.Y -= 128;
		Size = size;

		if (IsDisplayWithoutSearch)
			UpdateListing();
	}

	private void OnLineSearchModified(string _) { InputTimer.Start(); }

	#endregion
	
	protected abstract List<ProjectLabelCache.LabelLookupResult> LookupTerm(ProjectLabelCache cache, string term);

	#region Node Builders

	protected abstract void CreateItem(ProjectLabelCache.LabelLookupResult item);
	protected VBoxContainer CreateFileContainer(string file)
	{
		var hsep = new HSeparator();
		ResultList.AddChild(hsep);

		var label = new Label
		{
			Name = file + "_Header",
			Text = file,
			SelfModulate = Colors.LightGray,
		};

		ResultList.AddChild(label);

		var box = new VBoxContainer
		{
			Name = file,
		};

		ResultList.AddChild(box);
		return box;
	}

	#endregion
}
