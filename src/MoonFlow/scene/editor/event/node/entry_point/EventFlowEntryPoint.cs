using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

using Nindot.Al.EventFlow;

namespace MoonFlow.Scene.EditorEvent;

[GlobalClass]
[ScenePath("res://scene/editor/event/node/entry_point/event_flow_entry_point.tscn")]
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

	#endregion

	#region Signals

	protected override void OnConnectionChanged(PortOut port, PortIn connection)
	{
		// Clear self from current connection's incoming list
		Connection?.PortIn.RemoveIncoming(port);
		
		// Set connection
		Connection = connection?.Parent;

		// Add self to the new connection incoming list
		Connection?.PortIn.AddIncoming(port);

        Graph.EntryPoints[Name] = connection?.Parent.Content;
        DrawDebugLabel();
	}

	private void OnEntryPointNameChanged(string txt)
	{
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