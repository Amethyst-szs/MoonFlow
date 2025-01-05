using Godot;
using MoonFlow.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoonFlow.Scene.EditorEvent;

public partial class NodeHolder : Node2D
{
	public List<EventFlowEntryPoint> EntryPoints = [];

	private bool IsStepByStepDebug = false;

	[Signal]
	public delegate void StepByStepDebuggerEventHandler();

	public override void _Ready()
	{
		// Connect to internal signals
		ChildEnteredTree += OnChildEnteredTree;
		ChildExitingTree += OnChildExitingTree;
	}

	public override void _UnhandledKeyInput(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_graph_step_by_step_debug", false, true))
			EmitSignal(SignalName.StepByStepDebugger);
	}

	#region Auto-Arrange

	public async Task ArrangeAllNodes()
	{
		// Check if step by step debugger is requested
		IsStepByStepDebug = ProjectManager.IsProjectDebug() && Input.IsActionPressed("ui_graph_step_by_step_debug");

		if (IsStepByStepDebug)
		{
			EmitSignal(SignalName.StepByStepDebugger);
			HideAllNodes();
		}

		// Arrange all entry points
		foreach (var entry in EntryPoints)
			await ArrangeFromEntryPoint(entry);

		GD.Print("Finished automatic node arrangement");
	}

	private async Task ArrangeFromEntryPoint(EventFlowEntryPoint start)
	{
		var c = start.Connection;
		if (!IsInstanceValid(c))
			return;

		// Setup entry point position
		var hOffset = start.RootPanel.Size.X + 32f;
		var vOffset = CalcSortedBounds().End.Y + 32f;
		start.SetPosition(new Vector2(c.Position.X - hOffset, vOffset));

		// Setup first connection
		if (!c.HasMeta("Sort"))
			c.SetPosition(new Vector2(c.Position.X, vOffset));

		await ArrangeFromNode(c);
	}
	private async Task ArrangeFromNode(EventFlowNodeCommon node)
	{
		float vPos = CalcSortedBounds().Position.Y;

		while (IsInstanceValid(node))
		{
			if (IsStepByStepDebug)
			{
				node.Show();
				node.SetSelected();

				GD.PrintRich(string.Format("[i]SBS:[/i] {1} [b]({0})[/b]",
					node.Content.Id,
					node.Content.Name
				));

				await ToSignal(this, SignalName.StepByStepDebugger);

				foreach (var o in node.PortOutList.GetChildren())
					(o as PortOut).Show();
			}

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
					node = connection;
					continue;
				}

				var posOffset = node.RootPanel.Size.X + 32.0F;
				connection.SetPosition(new Vector2(node.Position.X + posOffset, node.Position.Y));

				node = connection;
				continue;
			}

			// Otherwise, recursively call function for each connection
			foreach (var con in node.Connections)
			{
				if (!IsInstanceValid(con) || con.HasMeta("Sort"))
					continue;

				var hOffset = node.RootPanel.Size.X + 32.0F;
				con.SetPosition(new Vector2(node.Position.X + hOffset, vPos));

				await ArrangeFromNode(con);

				vPos = CalcSortedBounds().End.Y + 32f;
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

	#region Debug

	private void HideAllNodes()
	{
		foreach (var child in GetChildren())
		{
			if (child is not EventFlowNodeCommon node)
				continue;
			
			node.Hide();
			foreach (var o in node.PortOutList.GetChildren())
				(o as PortOut)?.Hide();
		}
	}

	private void SelectById(string id)
	{
		var result = FindChild(id, false, false);
		if (result == null || result is not EventFlowNodeCommon node)
			return;
		
		node.SetSelected();
	}

	private Rect2 CalcSortedBounds()
	{
		var rect = new Rect2();

		foreach (var child in GetChildren())
		{
			var t = child.GetType();
			if (t != typeof(EventFlowNodeCommon) && !t.IsSubclassOf(typeof(EventFlowNodeCommon)))
				continue;
			
			var node = (EventFlowNodeCommon)child;
			if (node.HasMeta("Sort"))
				rect = rect.Merge(node.GetRect());
		}

		return rect;
	}

	#endregion
}
