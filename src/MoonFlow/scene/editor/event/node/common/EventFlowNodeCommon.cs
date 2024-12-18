using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

using Nindot.Al.EventFlow;

namespace MoonFlow.Scene.EditorEvent;

[GlobalClass]
[ScenePath("res://scene/editor/event/node/common/event_flow_node_common.tscn")]
public partial class EventFlowNodeCommon : EventFlowNodeBase
{
	#region Properties

	// ~~~~~~~~~~~ Node References ~~~~~~~~~~~ //

	public EventFlowNodeBase[] Connections = [];

	// ~~~~~~~~~~~~~~~~~ Data ~~~~~~~~~~~~~~~~ //

	public Nindot.Al.EventFlow.Node Content { get; protected set; } = null;

	// ~~~~~~~~~ Internal References ~~~~~~~~~ //

	[Export, ExportGroup("Internal References")]
	public VBoxContainer ParamHolder { get; private set; }
	[Export]
	public VBoxContainer ParamAddDropdownHolder { get; private set; }

	public override bool IsNode() { return true; }

	#endregion

	#region Initilization
	public override void InitContent(Nindot.Al.EventFlow.Node content, Graph graph)
	{
		// Setup state
		Graph = graph;
		Content = content;
		Name = content.Id.ToString();

		// Setup name and type headers
		var labelType = GetNode<Label>("%Label_Type");
		labelType.Text = Tr(Content.TypeBase, "EVENT_GRAPH_NODE_TYPE");
		labelType.TooltipText = Content.TypeBase;

		var labelName = GetNode<Label>("%Label_Name");
		labelName.Text = Content.Name;

		// Initilize properties and connections
		InitParamEditor();

		for (var i = 0; i < content.GetNextIdCount(); i++)
			CreatePortOut();

		DrawDebugLabel();
	}

	public override void InitContent(string entryName, Graph graph, EventFlowNodeCommon target)
	{
		throw new NotImplementedException("Wrong InitContent call! Use EventFlowEntryPoint class or correct init");
	}

	public override void SetupConnections(List<EventFlowNodeCommon> list)
	{
		Connections = new EventFlowNodeBase[list.Count];

		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] == null)
				continue;

			var port = PortOutList.GetChild(i) as PortOut;
			port.Connection = list[i];
		}
	}

	protected override void InitParamEditor()
	{
		var type = Content.GetSupportedParams(out Dictionary<string, Type> pList);
		if (type == Nindot.Al.EventFlow.Node.NodeOptionType.NO_OPTIONS)
		{
			ParamAddDropdownHolder.QueueFree();
			ParamHolder.QueueFree();
			return;
		}

		// Create all param editors
		foreach (var p in pList)
			EventNodeParamFactory.CreateParamEditor(this, p.Key, p.Value);
		
		// If there are no additional properties to add, remove dropdown
		if (ParamAddDropdownHolder.GetChildCount() == 0)
			ParamAddDropdownHolder.QueueFree();
	}

	public override bool InitContentMetadata(GraphMetadata holder, NodeMetadata data)
	{
		if (!base.InitContentMetadata(holder, data))
		{
			holder.Nodes.Add(Content.Id, Metadata);
			return false;
		}

		return true;
	}

	#endregion

	#region Signals

	protected override void OnConnectionChanged(PortOut port, EventFlowNodeCommon connection)
	{
		Connections[port.Index] = connection;

		if (connection == null)
		{
			Content.RemoveNextNode(port.Index);
			DrawDebugLabel();
			return;
		}

		if (!Content.TrySetNextNode(connection.Content, port.Index))
			throw new Exception("Failed to connect " + Content.Id + " to " + connection.Content.Id);

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

		if (Content != null)
		{
			txt += AppendDebugLabel(nameof(Content.Id), Content.Id) + '\n';

			txt += "Next: ";
			foreach (var id in Content.GetNextIds()) txt += id.ToString() + ", ";
			txt += '\n';

			txt += AppendDebugLabel(nameof(Content.TypeBase), Content.TypeBase);
			txt += AppendDebugLabel(nameof(Content.Name), Content.Name);
			txt += AppendDebugLabel("C# Type", Content.GetType().Name);
		}

		DebugDataDisplay.Text = txt;
    }

    #endregion
}
