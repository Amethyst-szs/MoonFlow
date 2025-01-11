using Godot;
using System;
using System.Collections.Generic;

using Nindot.Al.EventFlow;

using MoonFlow.Project;

namespace MoonFlow.Scene.EditorEvent;

[GlobalClass]
[ScenePath("res://scene/editor/event/node/event_flow_node_common.tscn")]
public partial class EventFlowNodeCommon : EventFlowNodeBase
{
	#region Properties

	// ~~~~~~~~~~~ Node References ~~~~~~~~~~~ //

	public EventFlowNodeCommon[] Connections = [];

	// ~~~~~~~~~~~~~~~~~ Data ~~~~~~~~~~~~~~~~ //

	public Nindot.Al.EventFlow.Node Content { get; protected set; } = null;

	// ~~~~~~~~~ Internal References ~~~~~~~~~ //

	[Export, ExportGroup("Internal References")]
	public VBoxContainer ParamHolder { get; private set; }
	[Export]
	public VBoxContainer ParamAddDropdownHolder { get; private set; }

	[Export]
	public MenuButton NameOptionButton { get; private set; }
	[Export]
	public LineEdit NameLineEdit { get; private set; }

	// ~~~~~~~~~~~~~~~ Signals ~~~~~~~~~~~~~~~ //

	[Signal]
	public delegate void MetadataEditRequestEventHandler(EventFlowNodeCommon self);

	#endregion

	#region Initilization

	public override void _Ready()
	{
		base._Ready();

		// Setup additional signal connections
		Connect(SignalName.MetadataEditRequest, Callable.From(
			new Action<EventFlowNodeCommon>(Application.OnMetadataEditRequest)
		));
	}

	public override void InitContent(Nindot.Al.EventFlow.Node content, Graph graph)
	{
		// Setup state
		Graph = graph;
		Content = content;
		Name = content.Id.ToString();

		// Setup name and type headers
		var labelType = GetNode<Label>("%Label_Type");
		labelType.Text = Tr(Content.TypeBase, "EVENT_GRAPH_NODE_TYPE");

		var labelName = GetNode<Label>("%Label_Name");
		labelName.Text = Content.Name;

		// Setup default colors
		var category = MetaCategoryTable.Lookup(Content.GetFactoryType());
		var color = MetaDefaultColorLookupTable.Lookup(category);
		RootPanel.SelfModulate = color;

		// Initilize properties and connections
		InitParamEditor();

		var edgeCount = Math.Max(content.GetNextIdCount(), content.GetMaxOutgoingEdges());
		for (var i = 0; i < edgeCount; i++)
			CreatePortOut();

		DrawDebugLabel();

		// Ensure connection array has enough space for ports
		Array.Resize(ref Connections, edgeCount);
	}

	public override void InitContent(string entryName, Graph graph, EventFlowNodeCommon target)
	{
		throw new NotImplementedException("Wrong InitContent call! Use EventFlowEntryPoint class or correct init");
	}

	public override void SetupConnections(List<EventFlowNodeCommon> list)
	{
		if (list.Count > Connections.Length)
			Array.Resize(ref Connections, list.Count);

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
		// Setup node name selection
		var nameType = Content.GetNodeNameOptions(out string[] options);
		switch (nameType)
		{
			case Nindot.Al.EventFlow.Node.NodeOptionType.NO_OPTIONS:
				NameOptionButton.QueueFree();
				NameLineEdit.QueueFree();
				break;
			case Nindot.Al.EventFlow.Node.NodeOptionType.PRESET_LIST:
				SetupNameOptions(options);

				if (IsInstanceValid(NameLineEdit))
					NameLineEdit.QueueFree();

				break;
			case Nindot.Al.EventFlow.Node.NodeOptionType.ANY_VALUE:
				if (IsInstanceValid(NameOptionButton))
					NameOptionButton.QueueFree();

				GetNode<Label>("%Label_Name").Hide();

				if (!NameLineEdit.HasMeta("InitComplete"))
				{
					NameLineEdit.Text = Content.Name;
					NameLineEdit.SetMeta("InitComplete", true);
				}

				break;
		}

		// Reset current param editors
		ParamHolder.Show();
		ParamAddDropdownHolder.Show();

		foreach (var child in ParamHolder.GetChildren()) child.QueueFree();
		foreach (var child in ParamAddDropdownHolder.GetChildren()) child.QueueFree();

		// Lookup param list for content
		var type = Content.GetSupportedParams(out Dictionary<string, Type> pList);
		if (type == Nindot.Al.EventFlow.Node.NodeOptionType.NO_OPTIONS)
		{
			ParamAddDropdownHolder.Hide();
			ParamHolder.Hide();
			return;
		}

		if (type == Nindot.Al.EventFlow.Node.NodeOptionType.ANY_VALUE)
		{
			ParamAddDropdownHolder.Hide();

			foreach (var p in Content.Params)
				EventNodeParamFactory.CreateParamEditor(this, (string)p.Key, p.Value.GetType());

			return;
		}

		// Create all param editors
		foreach (var p in pList)
			EventNodeParamFactory.CreateParamEditor(this, p.Key, p.Value);

		// If there are no additional properties to add, remove dropdown
		if (ParamAddDropdownHolder.GetChildCount() == 0)
			ParamAddDropdownHolder.Hide();
	}

	private void SetupNameOptions(string[] opt)
	{
		if (!IsInstanceValid(NameOptionButton))
			return;

		var popup = NameOptionButton.GetPopup();

		popup.Clear();
		foreach (var item in opt)
			popup.AddItem(item);

		var call = Callable.From(new Action<int>(OnSetName));

		if (!popup.IsConnected(PopupMenu.SignalName.IdPressed, call))
			popup.Connect(PopupMenu.SignalName.IdPressed, call);
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

	protected override PortOut CreatePortOut()
	{
		var port = base.CreatePortOut();

		if (Content.GetMaxOutgoingEdges() > 1)
			port.PortColor = PortColorList[port.Index];

		return port;
	}

	#endregion

	#region Signals

	protected override void OnConnectionChanged(PortOut port, PortIn connection)
	{
		SetNodeModified();

		// Clear self from current connection's incoming list
		Connections[port.Index]?.PortIn.RemoveIncoming(port);

		// Set connection
		Connections[port.Index] = connection?.Parent;

		// Add self to the new connection incoming list
		Connections[port.Index]?.PortIn.AddIncoming(port);

		if (connection == null)
		{
			Content.RemoveNextNode(port.Index);
			DrawDebugLabel();
			return;
		}

		if (!Content.TrySetNextNode(connection.Parent.Content, port.Index))
			throw new Exception("Failed to connect " + Content.Id + " to " + connection.Parent.Content.Id);

		DrawDebugLabel();
	}

	public override void DeleteNode()
	{
		// Remove self from outgoing connections
		foreach (var con in Connections)
			con?.PortIn?.RemoveIncoming(this);

		// Remove self from incoming connections
		foreach (var incoming in PortIn.IncomingList)
			incoming?.CallDeferred("RemoveConnection");

		SetNodeModified();

		// Delete content and godot object
		Graph.RemoveNode(Content);
		QueueFree();
	}

	public void OnSetName(int index)
	{
		Content.SetName(index);

		var labelName = GetNode<Label>("%Label_Name");
		labelName.Text = Content.Name;

		InitParamEditor();
		OnNodeNameModified();
	}

	public void OnSetName(string name)
	{
		Content.Name = name;

		var labelName = GetNode<Label>("%Label_Name");
		labelName.Text = Content.Name;

		InitParamEditor();
		OnNodeNameModified();
	}

	public void OnSetNameLineEdit(string name)
	{
		Content.Name = name;
		OnNodeNameModified();
		DrawDebugLabel();
	}
	protected virtual void OnNodeNameModified() { }

	private void OnMetadataEditRequest() { EmitSignal(SignalName.MetadataEditRequest, this); }

    #endregion

    #region Utility

    public override void SetNodeColor()
    {
        base.SetNodeColor();

		if (!Metadata.IsOverrideColor)
		{
			// Setup default colors
			var category = MetaCategoryTable.Lookup(Content.GetFactoryType());
			var color = MetaDefaultColorLookupTable.Lookup(category);
			RootPanel.SelfModulate = color;
		}
    }

    #endregion

    #region Debug

    protected override void DrawDebugLabel()
	{
		if (DebugDataDisplay == null)
			return;

		string txt = "";

		txt += AppendDebugLabel(nameof(Type), GetType().Name);
		txt += AppendDebugLabel(nameof(RawPosition), RawPosition.ToString("0"));
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
