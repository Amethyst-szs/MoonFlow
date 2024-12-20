using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoonFlow.Scene.EditorEvent;

public partial class NodeHolder : Node2D
{
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
			ArrangeFromEntryPoint(entry);
		
		GD.Print("Finished automatic node arrangement");
	}

    private static void ArrangeFromEntryPoint(EventFlowEntryPoint start)
	{
		var c = start.Connection;
		if (!IsInstanceValid(c))
			return;
		
		var posOffset = start.RootPanel.Size.X + 32.0F;
		start.SetPosition(c.Position - new Vector2(posOffset, 0));

		ArrangeFromNode(c);
	}
	private static void ArrangeFromNode(EventFlowNodeCommon node)
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

				// If this node is already sorted, skip it unless it's further left than the current node
				if (connection.HasMeta("Sort"))
				{
					node = (EventFlowNodeCommon)connection;
					continue;
				}

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
	}

	private void OnChildExitingTree(Node node)
	{
		if (!IsNodeValid(node))
			throw new Exception("Invalid node added to EventFlowGraph's NodeHolder: " + node.GetType());

		if (node is EventFlowEntryPoint)
			EntryPoints.Remove(node as EventFlowEntryPoint);
	}

	private static bool IsNodeValid(Node node)
	{
		var t = node.GetType();
		return t.IsSubclassOf(typeof(EventFlowNodeBase));
	}

	#endregion
}
