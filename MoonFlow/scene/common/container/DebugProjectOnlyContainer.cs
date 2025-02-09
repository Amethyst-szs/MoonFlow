using Godot;
using MoonFlow.Project;

namespace MoonFlow;

[GlobalClass]
[Icon("res://asset/material/debug/debug.svg")]
public partial class DebugProjectOnlyContainer : VBoxContainer
{
	public override void _EnterTree()
	{
		if (!ProjectManager.IsProjectDebug())
			QueueFree();
	}
}
