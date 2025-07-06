using Godot;
using System;
using System.Collections.Generic;

using Nindot.Al.EventFlow;

using MoonFlow.Project;
using System.Linq;

namespace MoonFlow.Scene.EditorEvent;

[GlobalClass, SceneUid("uid://cdcam0c6j5qr2")]
public partial class EventFlowEntryPoint : EventFlowNodeBase
{
	// ~~~~~~~~~~~ Node References ~~~~~~~~~~~ //

	public EventFlowNodeCommon Connection;

	// ~~~~~~~~~~~~~~~~~ Data ~~~~~~~~~~~~~~~~ //

	public GraphMetaBucketEntryPoint MetadataEntryPoint { get; protected set; } = null;

	// ~~~~~~~~~ Internal References ~~~~~~~~~ //

	[Export, ExportGroup("Internal References")]
	private LineEdit NameEdit;
	[Export]
	private TextureRect RectDuplicateKeyWarning;
	[Export]
	private TextureRect RectEmptyKeyWarning;

	#region Initilization

	public override void InitContent(Nindot.Al.EventFlow.Node content, Graph graph)
	{
		throw new NotImplementedException("Wrong InitContent call! Use EventFlowNodeCommon or correct init!");
	}

	public override void InitContent(string entryName, Graph graph, EventFlowNodeCommon target)
	{
		// Ensure we have a metadata pointer before continuing
		if (MetadataEntryPoint == null)
			throw new NullReferenceException("InitContent called before SetupEntryPointMetadata, no reference to MetadataEntryPoint!");

		Graph = graph;
		Name = MetadataEntryPoint.Uid;

		// Setup name and type headers
		var labelType = GetNode<Label>("%Label_Type");
		labelType.Text = Tr("EntryPoint", "EVENT_GRAPH_NODE_TYPE");

		NameEdit.Text = entryName;

		// Hide warnings
		RectDuplicateKeyWarning.Hide();
		RectEmptyKeyWarning.Hide();

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

	public void SetupEntryPointMetadata(GraphMetaBucketCommon holder, GraphMetaBucketEntryPoint data)
	{
		InitContentMetadata(holder, data);
		MetadataEntryPoint = data;
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

		Graph.EntryPoints[MetadataEntryPoint.Name] = connection?.Parent.Content;
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

		// Delete content and godot object
		Graph.EntryPoints.Remove(MetadataEntryPoint.Name);
		Application.Metadata.EntryPoints.Remove(MetadataEntryPoint.Uid);

		// Notify other nodes that the entry list has been modified
		Application.EmitSignal(EventFlowApp.SignalName.EntryPointListModified);

		SetNodeModified();
		QueueFree();
	}

	private void OnEntryPointNameChanged(string txt)
	{
		SetNodeModified();

		bool isNewKeyDuplicate = Graph.EntryPoints.ContainsKey(txt);
		bool isNewKeyEmpty = txt == string.Empty || txt == null;

		RectDuplicateKeyWarning.Visible = isNewKeyDuplicate;
		RectEmptyKeyWarning.Visible = isNewKeyEmpty;

		if (isNewKeyDuplicate || isNewKeyEmpty)
			return;
		
		// Update backend graph
		var oldName = MetadataEntryPoint.Name;
		Graph.EntryPoints.Remove(oldName);
		Graph.EntryPoints.Add(txt, Connection?.Content);

		// Update metadata and other nodes
		MetadataEntryPoint.Name = txt;
		Application.EmitSignal(EventFlowApp.SignalName.EntryPointListModified);

		DrawDebugLabel();
	}

	#endregion

	#region Debug

	protected override void DrawDebugLabel()
	{
		if (DebugDataDisplay == null || MetadataEntryPoint == null)
			return;

		string txt = "";

		txt += AppendDebugLabel(nameof(Type), GetType().Name);
		txt += AppendDebugLabel(nameof(Position), Position);

		txt += AppendDebugLabel(nameof(MetadataEntryPoint.Uid), MetadataEntryPoint.Uid);
		txt += AppendDebugLabel("GNN: ", Name);
		txt += AppendDebugLabel(nameof(MetadataEntryPoint.Name), MetadataEntryPoint.Name);

		if (Graph.EntryPoints.TryGetValue(MetadataEntryPoint.Name, out Nindot.Al.EventFlow.Node target) && target != null)
			txt += AppendDebugLabel("Target", target.Id);

		DebugDataDisplay.Text = txt;
	}

	#endregion
}