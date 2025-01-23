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

	protected override void AppFocusChanged()
	{
		base.AppFocusChanged();

		// Save As has not been implemented fully yet
		SetItemDisabled(GetItemIndex((int)MenuIds.FILE_SAVE_AS), true);

		var app = AppSceneServer.GetActiveApp();
		if (app == null)
			return;
		
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
		foreach (var app in AppSceneServer.GetApps())
		{
			if (!IsInstanceValid(app))
				continue;

			if (!app.IsAppAllowUnsavedChanges())
				continue;

			await app.AppSaveContent(false);
		}
	}
	private void OnFileClose()
	{
		AppSceneServer.CloseActiveApp();
	}

	private void OnMoonFlowApplicationClose()
	{
		EmitSignal(SignalName.MoonFlowCloseRequest);
	}
}
