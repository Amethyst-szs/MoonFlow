using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoonFlow.Scene.EditorEvent;

public partial class NodeHolder : Node2D
{
	public List<EventFlowNodeCommon> Nodes = [];
	public List<EventFlowEntryPoint> EntryPoints = [];

	public override void _Ready()
	{
		// Connect to internal signals
		ChildEnteredTree += OnChildEnteredTree;
		ChildExitingTree += OnChildExitingTree;
	}

	#region Auto-Arrange

	public void ArrangeAllNodes()
	{
		foreach (var entry in EntryPoints)
			ArrangeFromNode(entry);
	}

	private void ArrangeFromNode(EventFlowEntryPoint start)
	{
		ArrangeFromNode(start.Connection);
	}
	private void ArrangeFromNode(EventFlowNodeCommon node)
	{
		while (IsInstanceValid(node))
		{
			// Check if this node is already sorted
			if (node.HasMeta("Sort"))
				return;
			
			node.SetMeta("Sort", true);

			// If it has zero outgoing edges, return
			if (node.Connections.Length == 0)
				return;
			
			// If this node only has null edges, return
			if (node.Connections.All(n => n == null))
				return;

			// If this node only has one outgoing edge, place next node right in line
			if (node.Connections.Length == 1)
			{
				var connection = node.Connections[0];

				var posOffset = node.RootPanel.Size.X + 32.0F;
				connection.SetPosition(new Vector2(node.Position.X + posOffset, node.Position.Y));

				node = (EventFlowNodeCommon)connection;
				continue;
			}

			// Otherwise, recursively call function for each connection
			var vOffset = 0.0F;

			foreach (var con in node.Connections)
			{
				var hOffset = node.RootPanel.Size.X + 32.0F;
				con.SetPosition(new Vector2(node.Position.X + hOffset, node.Position.Y + vOffset));

				vOffset += con.RootPanel.Size.Y + 32.0F;

				ArrangeFromNode(con as EventFlowNodeCommon);
			}

			return;
		}
	}

	#endregion

	#region Signals

	private void OnChildEnteredTree(Node node)
	{
		if (!IsNodeValid(node))
			throw new Exception("Invalid node added to EventFlowGraph's NodeHolder: " + node.GetType());

		if (node is EventFlowEntryPoint)
			EntryPoints.Add(node as EventFlowEntryPoint);
		else
			Nodes.Add(node as EventFlowNodeCommon);
	}

	private void OnChildExitingTree(Node node)
	{
		if (!IsNodeValid(node))
			throw new Exception("Invalid node added to EventFlowGraph's NodeHolder: " + node.GetType());

		if (node is EventFlowEntryPoint)
			EntryPoints.Remove(node as EventFlowEntryPoint);
		else
			Nodes.Remove(node as EventFlowNodeCommon);
	}

	private static bool IsNodeValid(Node node)
	{
		var t = node.GetType();
		return t.IsSubclassOf(typeof(EventFlowNodeBase));
	}

	#endregion
}
