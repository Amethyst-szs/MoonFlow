using Godot;
using System;

namespace MoonFlow.Scene.Main;

public partial class Header : PanelContainer
{
	[Signal]
	public delegate void AppFocusedEventHandler();

	[Signal]
	public delegate void ButtonSaveEventHandler(bool isRequireFocus);
	[Signal]
	public delegate void ButtonSaveAsEventHandler();
}