using Godot;
using MoonFlow.Ext;
using MoonFlow.Project;
using MoonFlow.Project.Cache;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoonFlow.Scene;

[ScenePath("res://scene/common/popup/popup_msbt_select_entry.tscn")]
public partial class PopupMsbtSelectEntry : Window
{
	[Export]
	public bool IsSystemMessage = true;
	[Export]
	public bool IsStageMessage = false;
	[Export]
	public bool IsLayoutMessage = true;
	[Export]
	public bool IsDisplayWithoutSearch = false;
	[Export]
	public int MaxResults = 300;

	[Export, ExportGroup("Internal References")]
	private LineEdit LineSearch;
	[Export]
	private VBoxContainer ResultList;
	[Export]
	private Label LabelInvalidRequest;
	[Export]
	private Label LabelNoResults;
	[Export]
	private Label LabelTooManyResults;
	
	private Timer InputTimer;

	[Signal]
	public delegate void ItemSelectedEventHandler(string arc, string file, string label);

    public override void _Ready()
    {
        InputTimer = new Timer
        {
            WaitTime = 0.2,
			OneShot = true,
        };

        InputTimer.Timeout += OnInputTimerTimeout;
		AddChild(InputTimer);

        AboutToPopup += OnPopupReady;

		Hide();
		LabelNoResults.Hide();
		LabelTooManyResults.Hide();
		LabelInvalidRequest.Show();
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

    #region Signals

    private void OnPopupReady()
	{
		// Setup search box
		LineSearch.Text = "";
		LineSearch.GrabFocus();
		
		// Clear result list content
		foreach (var child in ResultList.GetChildren())
		{
			ResultList.RemoveChild(child);
			child.QueueFree();
		}

		// Set window size
		var size = GetTree().CurrentScene.GetWindow().Size;
		size.X /= 2;
		size.Y -= 128;
		Size = size;

		if (IsDisplayWithoutSearch)
			OnInputTimerTimeout();
	}

	private void OnLineSearchModified(string _) { InputTimer.Start(); }
	private void OnInputTimerTimeout()
	{
		// Reset warning messages
		LabelNoResults.Hide();
		LabelTooManyResults.Hide();
		LabelInvalidRequest.Hide();

		// Reset result list
		foreach (var child in ResultList.GetChildren())
		{
			ResultList.RemoveChild(child);
			child.QueueFree();
		}

		// Fetch search request
		var txt = LineSearch.Text;

		if (txt == string.Empty && !IsDisplayWithoutSearch)
		{
			LabelInvalidRequest.Show();
			return;
		}

		// Convert string into list of search terms separated by spaces
		var txtList = txt.Split([' ', '/'], StringSplitOptions.RemoveEmptyEntries);

		// Lookup all labels
		var lookup = ProjectManager.GetProject().MsgLabelCache;
		List<ProjectLabelCache.LabelLookupResult> results = [];

		switch (txtList.Length)
		{
			case 0:
				results = LookupTerm(lookup, "");
				break;
			case 1:
				results = LookupTerm(lookup, txtList[0]);
				break;
			case 2:
				results = LookupTermInFile(lookup, txtList[0], txtList[1]);
				break;
			default:
				LabelInvalidRequest.Show();
				return;
		}

		// If the list count is invalid, display a message and exit
		if (results.Count == 0)
		{
			LabelNoResults.Show();
			return;
		}

		if (results.Count >= MaxResults)
		{
			LabelTooManyResults.Show();
			return;
		}

		// Generate list of files
		foreach (var item in results)
			CreateItem(item);
	}

	private void OnItemSelected(ProjectLabelCache.LabelLookupResult item)
	{
		var arc = ProjectLabelCache.GetArchiveNameFromEnum(item);
		EmitSignal(SignalName.ItemSelected, arc, item.File, item.Label);
		QueueFree();
	}

	#endregion

	#region Label Lookup

	protected virtual List<ProjectLabelCache.LabelLookupResult> LookupTerm(ProjectLabelCache cache, string term)
	{
		List<ProjectLabelCache.LabelLookupResult> results = [];

		if (IsSystemMessage)
			results.AddRange(cache.LookupLabel(ProjectLabelCache.ArchiveType.SYSTEM, term));
		if (IsStageMessage)
			results.AddRange(cache.LookupLabel(ProjectLabelCache.ArchiveType.STAGE, term));
		if (IsLayoutMessage)
			results.AddRange(cache.LookupLabel(ProjectLabelCache.ArchiveType.LAYOUT, term));
		
		return results;
	}

	protected virtual List<ProjectLabelCache.LabelLookupResult> LookupTermInFile(ProjectLabelCache cache, string term, string file)
	{
		List<ProjectLabelCache.LabelLookupResult> results = [];

		if (IsSystemMessage)
			results.AddRange(cache.LookupLabelInFile(ProjectLabelCache.ArchiveType.SYSTEM, file, term));
		if (IsStageMessage)
			results.AddRange(cache.LookupLabelInFile(ProjectLabelCache.ArchiveType.STAGE, file, term));
		if (IsLayoutMessage)
			results.AddRange(cache.LookupLabelInFile(ProjectLabelCache.ArchiveType.LAYOUT, file, term));
		
		return results;
	}

	#endregion
	
	#region Node Builders

	private void CreateItem(ProjectLabelCache.LabelLookupResult item)
	{
		VBoxContainer container;

		var noExt = item.File.RemoveFileExtension();

		if (ResultList.HasNode(noExt))
			container = ResultList.GetNode<VBoxContainer>(noExt);
		else
			container = CreateFileContainer(noExt);
		
		var button = new Button()
		{
			Name = item.Label,
			Text = item.Label,
			TooltipText = item.PreviewText,
			AutowrapMode = TextServer.AutowrapMode.Arbitrary,
		};

		button.Connect(Button.SignalName.Pressed, Callable.From(() => OnItemSelected(item)));
		container.AddChild(button);
	}

	private VBoxContainer CreateFileContainer(string file)
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
