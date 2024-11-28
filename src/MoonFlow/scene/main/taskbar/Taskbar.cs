using Godot;
using System;

namespace MoonFlow.Scene.Main;

public partial class Taskbar : Control
{
    public override void _Ready()
    {
        GetWindow().SizeChanged += UpdateDisplay;
        UpdateDisplay();
    }

    public void AddApplication(AppScene app)
    {
        var button = new TaskbarButton(app);

        // Setup button signal connections
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
    }

    public bool TrySelectAppByIndex(int idx)
    {
        var childCount = GetChildCount();
        if (idx < 0 || idx >= childCount)
            return false;

        GetChild<TaskbarButton>(idx).App.AppFocus();
        return true;
    }

    // ====================================================== //
    // =================== User Interface =================== //
    // ====================================================== //

    public void UpdateDisplay()
    {
        if (GetChildCount() == 0)
            return;

        if (!IsInstanceValid(GetWindow()))
            return;

        var windowSize = GetWindow().Size;
        CustomMinimumSize = new Vector2(windowSize.X, ((Control)GetChild(0)).Size.Y);

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

    // ====================================================== //
    // ====================== Utilities ===================== //
    // ====================================================== //

    private float CalcButtonWidth()
    {
        if (GetChildCount() == 0)
            return 164;

        var windowSize = GetWindow().Size;
        return Math.Min(164, windowSize.X / GetChildCount());
    }
}
