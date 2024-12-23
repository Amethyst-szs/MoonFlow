using Godot;
using MoonFlow.Project;
using System;

namespace MoonFlow.Scene.Main;

public partial class ActionbarFile : ActionbarItemBase
{
	private enum MenuIds : int
	{
		FILE_SAVE = 0,
	}

	public override void _Ready()
	{
		base._Ready();

		AssignFunction((int)MenuIds.FILE_SAVE, OnFileSave, "ui_save");
	}

	private void OnFileSave()
	{
		Header.EmitSignal(Header.SignalName.ButtonSave);
	}
}
