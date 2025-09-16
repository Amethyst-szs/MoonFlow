using FuzzySharp;
using Godot;
using MoonFlow.Project;
using MoonFlow.Project.Cache;
using MoonFlow.Scene.EditorMsbt;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoonFlow.Scene;

[SceneUid("uid://bgg76e446o85y")]
public partial class PopupMsbtFindSearchEntry : PopupMsbtLookupBase
{
	[Export, ExportGroup("Internal References")]
	private Label LabelInvalidRequest;
	[Export]
	private Label LabelNoResults;

	public override void _Ready()
	{
		base._Ready();
		
		LabelNoResults.Hide();
		LabelInvalidRequest.Show();
    }

	#region Signals

	protected override void UpdateListing()
	{
		// Reset warning messages
		LabelNoResults.Hide();
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
		var results = lookup.CreateSortedListByFuzz(txt);

		// If the list count is invalid, display a message and exit
		if (results.Count == 0)
		{
			LabelNoResults.Show();
			return;
		}

		// Generate list of files
		foreach (var item in results.Take(MaxResults))
			CreateItem(item);

		ResultScroll.Call("scroll_to_top");
	}
	protected override async void HandleItemSelect(ProjectLabelCache.LabelLookupResult item)
	{
		QueueFree();

		var arcName = ProjectLabelCache.GetArchiveNameFromEnum(item);
		await MsbtAppHolder.OpenAppWithSearch(arcName, item.File, item.Label);
	}

	#endregion
	
	protected override List<ProjectLabelCache.LabelLookupResult> LookupTerm(ProjectLabelCache cache, string term)
	{
		throw new NotImplementedException();
	}

	#region Node Builders

	protected override void CreateItem(ProjectLabelCache.LabelLookupResult item)
	{
		var noExt = item.File.RemoveFileExtension();

		var button = new Button()
		{
			Name = item.Label,
			Text = string.Format("{0} ({1}%)\n- {2} -", item.Label, item.FuzzValue, item.File),
			TooltipText = item.PreviewText,
			AutowrapMode = TextServer.AutowrapMode.Arbitrary,
		};

		button.Connect(Button.SignalName.Pressed, Callable.From(() => HandleItemSelect(item)));
		ResultList.AddChild(button);
	}

    #endregion
}
