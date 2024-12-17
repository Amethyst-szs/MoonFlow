using Godot;
using System;

namespace MoonFlow.Scene.EditorEvent;

[GlobalClass]
[ScenePath("res://scene/editor/event/node/event_flow_node.tscn")]
public partial class EventFlowNode : Node2D
{
	[Export]
	public PortIn PortIn { get; private set; }
}
