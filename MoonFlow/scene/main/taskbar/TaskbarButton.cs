using Godot;
using System;

namespace MoonFlow.Scene.Main;

[ScenePath("res://scene/main/taskbar/taskbar_button.tscn")]
public partial class TaskbarButton : Button
{
	public AppScene App { get; private set; } = null;

	private Panel UnsavedDot = null;
	private static readonly StyleBoxFlat UnsavedDotStyle
		= GD.Load<StyleBoxFlat>("res://asset/theme/main/stylebox/taskbar_dot.tres");

	[Export]
	private Button AppCloser = null;

	public void Init(AppScene app)
	{
		// Setup button
		App = app;
		app.TaskbarButton = this;

		Text = app.AppTaskbarTitle;
		Name = app.AppTaskbarTitle;
		TooltipText = app.AppTaskbarTitle;
		Icon = app.AppIcon;

		var height = EngineSettings.GetSetting<float>("moonflow/general/taskbar_height", 40.0F);
		CustomMinimumSize = new Vector2(CustomMinimumSize.X, height);
		Size = CustomMinimumSize;

		EngineSettings.Connect("taskbar_size_modified", OnTaskbarSizeChanged);

		// Setup app closer
		if (!app.IsAppAllowUserToClose())
			AppCloser.QueueFree();

		// Setup unsaved changes dot if app supports it
		if (app.IsAppAllowUnsavedChanges())
		{
			UnsavedDot = new()
			{
				Visible = false,
				ZIndex = 1000,
			};

			UnsavedDot.AddThemeStyleboxOverride("panel", UnsavedDotStyle);
			AddChild(UnsavedDot);

			UnsavedDot.SetAnchorsPreset(LayoutPreset.BottomRight);

			app.Connect(AppScene.SignalName.ModifyStateUpdate,
				Callable.From(new Action<bool>(OnModifyStateChanged)));
		}
	}

	public override void _Pressed()
	{
		if ((Input.GetMouseButtonMask() & MouseButtonMask.Middle) != 0)
		{
			TryCloseApp();
			return;
		}

		App.AppFocus();
	}

	private void TryCloseApp()
	{
		// If the active app is exclusive (and this isn't the active app), ignore
		var activeApp = AppSceneServer.GetActiveApp();
		if (activeApp != App && activeApp.IsAppExclusive())
		{
			SetPressedNoSignal(false);
			return;
		}

		// Do not allow closing the home page application
		if (!App.IsAppAllowUserToClose())
		{
			SetPressedNoSignal(true);
			return;
		}

		App.AppClose(true);
		GetViewport().SetInputAsHandled();
	}

	private void OnModifyStateChanged(bool isModified)
	{
		if (IsInstanceValid(UnsavedDot))
			UnsavedDot.SetDeferred(PropertyName.Visible, isModified);
	}

	private void OnTaskbarSizeChanged()
	{
		var height = EngineSettings.GetSetting<float>("moonflow/general/taskbar_height", 40.0F);
		CustomMinimumSize = new Vector2(CustomMinimumSize.X, height);
		Size = new Vector2(Size.X, height);
	}
}
