using Godot;
using System;
using System.Linq;

using Nindot.Al.EventFlow;
using Nindot.Al.EventFlow.Smo;

using MoonFlow.Project;

namespace MoonFlow.Scene.EditorEvent;

public partial class EventFlowNodeEntryJump : EventFlowNodeCommon
{
	protected GraphMetaBucketEntryPoint Target = null;
	protected NodeJumpEntry NodeJump = null;

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

		// Get target uid from metadata
		Target = Application.Metadata.GetEntryPointByName(NodeJump.JumpEntryName);

		// Connect to event from application
		Application.Connect(EventFlowApp.SignalName.EntryPointListModified, Callable.From(OnEntryPointListModified));

		// Assign default selection
		OnEntryPointListModified();
	}

	#region Signals

	private void OnEntryPointListModified()
	{
		// Regenerate dropdown menu contents
		JumpList.Clear();
		foreach (var point in Application.Metadata.EntryPoints.Values)
			JumpList.AddItem(point.Name);

		// Set dropdown menu selection
		var targetIdx = Application.Metadata.EntryPoints.Values.ToList().IndexOf(Target);
		JumpList.Select(targetIdx);

		// Update internal selection
		OnEntryPointJumpTargetSelected(targetIdx);

		DrawDebugLabel();
	}

	private void OnEntryPointJumpTargetSelected(int idx)
	{
		if (idx == -1)
		{
			Target = null;
			NodeJump.JumpEntryName = "__NULL__";

			SetNodeModified();
			return;
		}

		Target = Application.Metadata.EntryPoints.Values.ElementAt(idx);
		if (Target != null)
			NodeJump.JumpEntryName = Target.Name;

		SetNodeModified();
		DrawDebugLabel();
	}

	#endregion

	#region Debug

	protected override void DrawDebugLabel()
	{
		if (DebugDataDisplay == null)
			return;

		string txt = "";

		txt += AppendDebugLabel(nameof(Type), GetType().Name);

		if (Content != null)
		{
			txt += AppendDebugLabel(nameof(Content.Id), Content.Id) + '\n';
			txt += AppendDebugLabel(nameof(Content.TypeBase), Content.TypeBase);
			txt += AppendDebugLabel(nameof(Content.Name), Content.Name);
			txt += AppendDebugLabel("C# Type", Content.GetType().Name);
		}

		txt += "\n";
		txt += AppendDebugLabel("Target Uid", Target?.Uid);
		txt += AppendDebugLabel("Target Name", Target?.Name);
		txt += AppendDebugLabel("Internal Target Name", NodeJump?.JumpEntryName);

		DebugDataDisplay.Text = txt;
	}

	#endregion
}
