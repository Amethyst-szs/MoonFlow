using Godot;
using System;
using System.Linq;
using MoonFlow.Scene.Home;

namespace MoonFlow.Scene.Main;

public partial class Taskbar : Control
{
    #region Init & App Init

    private MainSceneRoot Parent;

    public override void _Ready()
    {
        Parent = this.FindParentByType<MainSceneRoot>();

        GetWindow().SizeChanged += UpdateDisplay;
        EngineSettings.Connect("taskbar_size_modified", OnTaskbarSizeChanged);

        UpdateDisplay();
    }

    public bool TryAddApplication(AppScene app)
    {
        // Ensure app isn't already open
        if (app.IsAppOnlyOneInstance())
        {
            var unique = app.AppUniqueIdentifier;
            var list = GetChildren().Cast<TaskbarButton>();

            var result = list.ToList().Find(s => s.App.AppUniqueIdentifier == unique);

            if (IsInstanceValid(result))
            {
                result.App.AppFocus();
                return false;
            }
        }

        // Create new taskbar button
        var button = SceneCreator<TaskbarButton>.Create();
        button.Init(app);

        button.TreeExited += UpdateDisplay;

        // Calculate initial button position and add to child list
        var screenWidth = GetWindow().Size.X;
        var buttonPos = CalcButtonWidth() * GetChildCount();

        AddChild(button);

        if (buttonPos >= screenWidth - 5)
            button.Position = new Vector2(screenWidth, 0);
        else
            button.Position = new Vector2(buttonPos, 0);

        UpdateDisplay();

        // Get access to the main
        var treeRoot = GetTree().CurrentScene;

        if (treeRoot.GetType() != typeof(MainSceneRoot))
            throw new Exception("Cannot initilize taskbar item outside of MainScene!");

        var scene = (MainSceneRoot)treeRoot;

        // If no app is currently selected, select this new app
        if (scene.GetActiveApp() == null)
            app.AppFocus();

        return true;
    }

    #endregion

    #region Input

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_app_nav_left", true, true))
        {
            TryNavTaskbarByOffset(-1);
            GetViewport().SetInputAsHandled();
            return;
        }

        if (@event.IsActionPressed("ui_app_nav_right", true, true))
        {
            TryNavTaskbarByOffset(1);
            GetViewport().SetInputAsHandled();
            return;
        }

        if (@event.IsActionPressed("ui_app_nav_home", true, true))
        {
            TryNavTaskbarHome();
            GetViewport().SetInputAsHandled();
            return;
        }
    }

    #endregion

    #region UI Render

    public void UpdateDisplay()
    {
        if (GetChildCount() == 0)
            return;

        if (!IsInstanceValid(GetWindow()))
            return;

        var height = EngineSettings.GetSetting<float>("moonflow/general/taskbar_height", 40.0F);
        CustomMinimumSize = new Vector2(0, height);

        float buttonWidth = CalcButtonWidth();
        for (int i = 0; i < GetChildCount(); i++)
        {
            var node = (Control)GetChild(i);
            var tween = node.CreateTween().SetTrans(Tween.TransitionType.Cubic).SetParallel();
            tween.TweenProperty(node, "position:x", buttonWidth * i, 0.25);
            tween.TweenProperty(node, "size:x", buttonWidth, 0.25);
            tween.TweenProperty(node, "custom_minimum_size:x", buttonWidth, 0.25);
        }
    }

    #endregion

    #region Utility

    public bool TrySelectAppByIndex(int idx)
    {
        var childCount = GetChildCount();
        if (idx < 0 || idx >= childCount)
            return false;

        GetChild<TaskbarButton>(idx).App.AppFocus();
        return true;
    }

    private void TryNavTaskbarByOffset(int offset)
    {
        var active = Parent.GetActiveApp();
        if (active == null || active.IsAppExclusive()) return;

        var idx = active.GetIndex() + offset;
        idx = idx.ModPosNeg(Parent.NodeApps.GetChildCount());

        TrySelectAppByIndex(idx);
    }

    private void TryNavTaskbarHome()
    {
        var active = Parent.GetActiveApp();
        if (active == null || active.IsAppExclusive()) return;

        var home = Parent.GetApp<HomeRoot>();
        if (home == null) return;

        TrySelectAppByIndex(home.GetIndex());
    }

    private float CalcButtonWidth()
    {
        if (GetChildCount() == 0)
            return 164;

        var windowSize = GetWindow().Size;
        return Math.Min(164, windowSize.X / GetChildCount());
    }

    private void OnTaskbarSizeChanged()
    {
        var height = EngineSettings.GetSetting<float>("moonflow/general/taskbar_height", 40.0F);
        CustomMinimumSize = new Vector2(CustomMinimumSize.X, height);
        Size = new Vector2(Size.X, height);
    }

    #endregion
}
