using Godot;
using MoonFlow.Scene.Home;

namespace MoonFlow.Scene.Main;

public partial class ActionbarFile : ActionbarItemBase
{
	private enum MenuIds : int
	{
		FILE_SAVE = 0,
		FILE_SAVE_AS = 1,
		FILE_SAVE_ALL = 2,
		FILE_CLOSE = 3,

		CLOSE_MOONFLOW = 42069, // nice
	}

	[Signal]
    public delegate void MoonFlowCloseRequestEventHandler();

	public override void _Ready()
	{
		base._Ready();

		AssignFunction((int)MenuIds.FILE_SAVE, OnFileSave, "ui_save");
		AssignFunction((int)MenuIds.FILE_SAVE_AS, OnFileSaveAs, "ui_save_as");
		AssignFunction((int)MenuIds.FILE_SAVE_ALL, OnFileSaveAll, "ui_save_all");
		AssignFunction((int)MenuIds.FILE_CLOSE, OnFileClose);
		AssignFunction((int)MenuIds.CLOSE_MOONFLOW, OnMoonFlowApplicationClose);
	}

    protected async override void AppFocusChanged()
    {
        base.AppFocusChanged();

		// Save As has not been implemented fully yet
		SetItemDisabled(GetItemIndex((int)MenuIds.FILE_SAVE_AS), true);

		var scene = await GetScene();
		var app = scene.GetActiveApp() ;
		bool isAllowUserClose = app.IsAppAllowUserToClose();

		Header.ButtonAppClose.Visible = isAllowUserClose;
		Header.ButtonAppMinimize.Visible = isAllowUserClose;

		if (app is not HomeRoot)
			return;
		
		for (var i = 0; i < ItemCount; i++)
			SetItemDisabled(i, true);
		
		SetItemDisabled(GetItemIndex((int)MenuIds.CLOSE_MOONFLOW), false);
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
		var scene = await GetScene();
		foreach (var app in scene.GetApps())
		{
			if (!IsInstanceValid(app))
				continue;
			
			if (!app.IsAppAllowUnsavedChanges())
				continue;
			
			await app.SaveFile(false);
		}
	}
	private async void OnFileClose()
	{
        var scene = await GetScene();
		scene.CloseActiveApp();
	}

	private void OnMoonFlowApplicationClose()
	{
		EmitSignal(SignalName.MoonFlowCloseRequest);
	}
}
