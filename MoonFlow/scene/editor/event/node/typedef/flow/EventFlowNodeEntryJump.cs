using Godot;
using System;
using System.Linq;

using Nindot.Al.EventFlow;
using Nindot.Al.EventFlow.Smo;

using MoonFlow.Project;

namespace MoonFlow.Scene.EditorEvent;

public partial class EventFlowNodeEntryJump : EventFlowNodeCommon
{
	protected NodeJumpEntry NodeJump;

	[Export]
	private OptionButton JumpList;

	public override void InitContent(Nindot.Al.EventFlow.Node content, Graph graph)
	{
		base.InitContent(content, graph);

		// Setup references and colors
		NodeJump = (NodeJumpEntry)content;

		var color = MetaDefaultColorLookupTable.Lookup(MetaCategoryTable.Categories.ENTRY_POINT);
		color = color.Darkened(0.1F);

		RootPanel.SelfModulate = color;
		PortIn.Modulate = color;

		// Connect to event from application
		Application.Connect(EventFlowApp.SignalName.EntryPointListModified,
			Callable.From(new Action<string, string>(OnEntryPointListModified)));
		
		// Assign default selection
		SetupSelection();
	}

	public void SetupSelection()
	{
		string n = NodeJump.JumpEntryName;
		OnEntryPointListModified(n, n);
	}

	#region Signals

	public void OnEntryPointDeleted(string name)
	{
		if (NodeJump.JumpEntryName != name)
			return;

		OnEntryPointListModified("", "");
		SetNodeModified();
	}

	private void OnEntryPointListModified(string oldName, string name)
	{
		var oldIdx = JumpList.Selected;
		var newIdx = -1;

		// Regenerate dropdown menu contents
		JumpList.Clear();
		for (var i = 0; i < Graph.EntryPoints.Count; i++)
		{
			var point = Graph.EntryPoints.ElementAt(i).Key;
			JumpList.AddItem(point);

			if (point == name)
				newIdx = i;
		}

		if (oldName == string.Empty && name == string.Empty)
		{
			JumpList.Selected = oldIdx;
			OnEntryPointJumpTargetSelected(oldIdx);
			return;
		}

		if (oldName == NodeJump.JumpEntryName && newIdx != -1)
		{
			JumpList.Selected = newIdx;
			OnEntryPointJumpTargetSelected(newIdx);
			return;
		}
	}

	private void OnEntryPointJumpTargetSelected(int idx)
	{
		if (idx >= Graph.EntryPoints.Count)
			idx = Graph.EntryPoints.Count - 1;
		
		if (idx == -1)
		{
			NodeJump.JumpEntryName = "__NULL__";
			SetNodeModified();
			return;
		}

		var name = Graph.EntryPoints.Keys.ElementAt(idx);
		NodeJump.JumpEntryName = name;
		SetNodeModified();
	}

	#endregion
}
