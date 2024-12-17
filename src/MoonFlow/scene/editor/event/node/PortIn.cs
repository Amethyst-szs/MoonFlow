using Godot;
using System;

namespace MoonFlow.Scene.EditorEvent;

[GlobalClass]
[ScenePath("res://scene/editor/event/node/port_in.tscn")]
[Icon("res://asset/material/graph/port.svg")]
public partial class PortIn : TextureRect
{
	public EventFlowNode Parent { get; private set; } = null;

	public override void _Ready()
	{
		// Search upward for parent flow node
		Node nextParent = this;
		while (Parent == null)
		{
			nextParent = nextParent.GetParent();
			if (!IsInstanceValid(nextParent))
				throw new NullReferenceException("Port is not a child of an EventFlowNode!");
			
			if (nextParent.GetType() == typeof(EventFlowNode))
				Parent = nextParent as EventFlowNode;
		}
	}
}
