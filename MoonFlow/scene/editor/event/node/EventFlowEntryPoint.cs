using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

using Nindot.Al.EventFlow;

namespace MoonFlow.Scene.EditorEvent;

[GlobalClass]
[ScenePath("res://scene/editor/event/node/event_flow_entry_point.tscn")]
public partial class EventFlowEntryPoint : EventFlowNodeBase
{
	public EventFlowNodeCommon Connection;

	[Export, ExportGroup("Internal References")]
	private LineEdit NameEdit;

	#region Initilization

	public override void InitContent(Nindot.Al.EventFlow.Node content, Graph graph)
	{
		throw new NotImplementedException("Wrong InitContent call! Use EventFlowNodeCommon or correct init!");
	}

	public override void InitContent(string entryName, Graph graph, EventFlowNodeCommon target)
	{
		Graph = graph;
		Name = entryName;

		// Setup name and type headers
		var labelType = GetNode<Label>("%Label_Type");
		labelType.Text = Tr("EntryPoint", "EVENT_GRAPH_NODE_TYPE");

		NameEdit.Text = entryName;

		// Setup default colors
		var color = MetaDefaultColorLookupTable.Lookup(MetaCategoryTable.Categories.ENTRY_POINT);
		RootPanel.SelfModulate = color;
		DefaultPortOutColor = color;

		// Setup ports
		PortIn.QueueFree();
		PortIn = null;

		var o = CreatePortOut();
		Connection = target;
		o.Connection = target;

		DrawDebugLabel();
	}

	public override void SetupConnections(List<EventFlowNodeCommon> list)
	{
		throw new NotImplementedException("Not compatible with EventFlowEntryPoint");
	}

	protected override void InitParamEditor()
	{
		throw new NotImplementedException("Not compatible with EventFlowEntryPoint");
	}

	public override bool InitContentMetadata(GraphMetadata holder, NodeMetadata data)
	{
		if (!base.InitContentMetadata(holder, data))
		{
			holder.EntryPoints.Add(Name, Metadata);
			return false;
		}

		return true;
	}

	#endregion

	#region Signals

	protected override void OnConnectionChanged(PortOut port, PortIn connection)
	{
		SetNodeModified();

		// Clear self from current connection's incoming list
		Connection?.PortIn.RemoveIncoming(port);

		// Set connection
		Connection = connection?.Parent;

		// Add self to the new connection incoming list
		Connection?.PortIn.AddIncoming(port);

		Graph.EntryPoints[Name] = connection?.Parent.Content;
		DrawDebugLabel();
	}

    public override void DeleteNode()
    {
		// Update connection that we are dead o7
		foreach (var node in PortOutList.GetChildren())
		{
			var port = node as PortOut;
			Connection?.PortIn.RemoveIncoming(port);
		}

		// Loop through graph, removing self from jump nodes
		foreach (var node in Application.GraphNodeHolder.GetChildren())
		{
			if (node is not EntryPointJump) continue;

			var jump = (EntryPointJump)node;
			jump.OnEntryPointDeleted(Name);
		}
		
        // Delete content and godot object
		Graph.EntryPoints.Remove(Name);
		QueueFree();
    }

    private void OnEntryPointNameChanged(string txt)
	{
		SetNodeModified();

		var oldName = Name;
		
		if (Graph.EntryPoints.ContainsKey(txt))
		{
			var caret = NameEdit.CaretColumn;
			NameEdit.Text = Name;
			NameEdit.CaretColumn = caret;
			return;
		}

		Graph.EntryPoints.Remove(Name);
		Graph.EntryPoints.Add(txt, Connection?.Content);

		Metadata = Application.Metadata.RenameEntryPoint(Name, txt);

		Name = txt;
		DrawDebugLabel();

		Application.EmitSignal(EventFlowApp.SignalName.EntryPointListModified, oldName, Name);
	}

	#endregion

	#region Debug

	protected override void DrawDebugLabel()
	{
		if (DebugDataDisplay == null)
			return;

		string txt = "";

		txt += AppendDebugLabel(nameof(Type), GetType().Name);
		txt += AppendDebugLabel(nameof(Position), Position);

		txt += AppendDebugLabel(nameof(Name), Name);

		if (Graph.EntryPoints.TryGetValue(Name, out Nindot.Al.EventFlow.Node target) && target != null)
			txt += AppendDebugLabel("Target", target.Id);

		DebugDataDisplay.Text = txt;
	}

	#endregion
}