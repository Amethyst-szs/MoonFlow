using Godot;
using System;

using Nindot.Al.EventFlow;
using Nindot.Al.EventFlow.Smo;
using System.Linq;

namespace MoonFlow.Scene.EditorEvent;

public partial class EntryPointJump : EventFlowNodeCommon
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
    }

	#region Signals

	private void OnEntryPointListModified(string oldName, string name)
	{
		JumpList.Clear();

		var newIdx = -1;
		
		for (var i = 0; i < Graph.EntryPoints.Count; i++)
		{
			var point = Graph.EntryPoints.ElementAt(i).Key;
			JumpList.AddItem(point);

			if (point == name)
				newIdx = i;
		}
		
		if (oldName == NodeJump.JumpEntryName && newIdx != -1)
			JumpList.Select(newIdx);
	}

	private void OnEntryPointJumpTargetSelected(int idx)
	{
		var name = Graph.EntryPoints.Keys.ElementAt(idx);
		NodeJump.JumpEntryName = name;
	}

	#endregion
}
