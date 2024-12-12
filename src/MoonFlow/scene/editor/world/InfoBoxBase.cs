using Godot;
using System;

using MoonFlow.Project.Database;

namespace MoonFlow.Scene.EditorWorld;

public partial class InfoBoxBase : VBoxContainer
{
	protected WorldInfo Info = null;

	[Signal]
    public delegate void ModifiedWorldInfoEventHandler();
	[Signal]
    public delegate void ModifiedShineListEventHandler();
	[Signal]
    public delegate void ModifiedItemInfoEventHandler();

	public virtual void OpenWorld(WorldInfo world)
	{
		Info = world;
	}
}
