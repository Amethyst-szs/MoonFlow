using FuzzySharp;
using Godot;
using MoonFlow.Project;
using MoonFlow.Project.Cache;
using System;
using System.Collections.Generic;

namespace MoonFlow.Scene;

[SceneUid("uid://h84sss0s2x5b")]
public partial class PopupMsbtSelectEntry : PopupMsbtLookupBase
{
	[Export]
	public bool IsSystemMessage = true;
	[Export]
	public bool IsStageMessage = false;
	[Export]
	public bool IsLayoutMessage = true;

	[Export, ExportGroup("Internal References")]
	private Label LabelInvalidRequest;
	[Export]
	private Label LabelNoResults;
	[Export]
	private Label LabelTooManyResults;

	public override void _Ready()
	{
		base._Ready();
		
		LabelNoResults.Hide();
		LabelTooManyResults.Hide();
		LabelInvalidRequest.Show();
    }

	#region Signals

	protected override void UpdateListing()
	{
		// Reset warning messages
		LabelNoResults.Hide();
		LabelTooManyResults.Hide();
		LabelInvalidRequest.Hide();

		// Reset result list
		ResultList.QueueFreeAllChildren();

		// Fetch search request
		var txt = LineSearch.Text;
		SearchLast = txt;

		if (txt == string.Empty && !IsDisplayWithoutSearch)
		{
			LabelInvalidRequest.Show();
			return;
		}

		// Lookup all labels
		var lookup = ProjectManager.GetProject().MsgLabelCache;
		List<ProjectLabelCache.LabelLookupResult> results = LookupTerm(lookup, txt);

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
	protected override void HandleItemSelect(ProjectLabelCache.LabelLookupResult item)
	{
		var arc = ProjectLabelCache.GetArchiveNameFromEnum(item);
		EmitSignal(SignalName.ItemSelected, arc, item.File, item.Label);
		QueueFree();
	}

	#endregion

	protected override List<ProjectLabelCache.LabelLookupResult> LookupTerm(ProjectLabelCache cache, string term)
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

	#region Node Builders

	protected override void CreateItem(ProjectLabelCache.LabelLookupResult item)
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

		button.Connect(Button.SignalName.Pressed, Callable.From(() => HandleItemSelect(item)));
		container.AddChild(button);
	}

    #endregion
}
