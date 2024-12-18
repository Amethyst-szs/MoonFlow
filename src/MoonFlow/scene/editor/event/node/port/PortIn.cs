using Godot;
using System;

namespace MoonFlow.Scene.EditorEvent;

[GlobalClass]
[ScenePath("res://scene/editor/event/node/port/port_in.tscn")]
[Icon("res://asset/material/graph/port.svg")]
public partial class PortIn : TextureRect
{
	public EventFlowNodeCommon Parent { get; private set; } = null;

	public override void _Ready()
	{
		// Search upward for parent flow node
		Node nextParent = this;
		while (Parent == null)
		{
			nextParent = nextParent.GetParent();
			if (!IsInstanceValid(nextParent))
				return;
			
			var t1 = nextParent.GetType();
			var t2 = typeof(EventFlowNodeCommon);
			
			if (t1 == t2 || t1.IsSubclassOf(t2))
				Parent = nextParent as EventFlowNodeCommon;
		}
	}
}
