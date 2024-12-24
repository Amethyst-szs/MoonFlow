using Godot;
using MoonFlow.Project;
using System;

namespace MoonFlow.Scene.Main;

public partial class ActionbarFile : ActionbarItemBase
{
	private enum MenuIds : int
	{
		FILE_SAVE = 0,
		FILE_SAVE_AS = 1,
		FILE_SAVE_ALL = 2,
		FILE_CLOSE = 3,
	}

	public override void _Ready()
	{
		base._Ready();

		AssignFunction((int)MenuIds.FILE_SAVE, OnFileSave, "ui_save");
		AssignFunction((int)MenuIds.FILE_SAVE_AS, OnFileSaveAs, "ui_save_as");
		AssignFunction((int)MenuIds.FILE_SAVE_ALL, OnFileSaveAll, "ui_save_all");
		AssignFunction((int)MenuIds.FILE_CLOSE, OnFileClose);
	}

	private void OnFileSave()
	{
		Header.EmitSignal(Header.SignalName.ButtonSave, true);
	}
	private void OnFileSaveAs()
	{
		Header.EmitSignal(Header.SignalName.ButtonSaveAs);
	}
	private async void OnFileSaveAll()
	{
		var scene = GetScene();
		foreach (var app in scene.GetApps())
		{
			if (!IsInstanceValid(app))
				continue;
			
			if (!app.IsAppAllowUnsavedChanges())
				continue;
			
			await app.SaveFile(false);
		}
	}
	private void OnFileClose()
	{
        var scene = GetScene();
		scene.CloseActiveApp();
	}

	private MainSceneRoot GetScene()
	{
		var sceneBase = GetTree().CurrentScene;
        if (sceneBase.GetType() != typeof(MainSceneRoot))
            throw new Exception("Invalid scene type!");
        
        return (MainSceneRoot)sceneBase;
	}
}
