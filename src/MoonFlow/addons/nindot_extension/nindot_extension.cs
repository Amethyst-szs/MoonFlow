#if TOOLS
using Godot;
using System;

namespace MoonFlow.Addons.NindotExt;

[Tool]
public partial class nindot_extension : EditorPlugin
{
	public override void _EnterTree()
	{
		// Initialization of the plugin goes here.
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
	}
}
#endif
