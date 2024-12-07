using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Nindot.LMS.Msbt.TagLib;

namespace MoonFlow.LMS.Msbt;

public abstract partial class TagSubmenuBase : PanelContainer
{
    [Export]
    bool IsCenterWindow = false;

    [Signal]
    public delegate void AddTagEventHandler(Array<TagWheelTagResult> tag);

    public abstract void InitSubmenu();
    public void SetupPosition(Vector2 gPos)
    {
        if (IsCenterWindow)
            gPos = GetWindow().Size / 2;

        // Setup pivot and position
        PivotOffset = Size / 2F;
        GlobalPosition = gPos - (Size / 2F);

        // Play small appear animation
        Modulate = Color.Color8(255, 255, 255, 0);
        Scale = Vector2.One * 0.7F;

        var tween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quint).SetParallel();
        tween.TweenProperty(this, "scale", Vector2.One, 0.22);
        tween.TweenProperty(this, "modulate", Color.Color8(255, 255, 255), 0.22);
    }

    // ====================================================== //
    // ==================== Menu Closing ==================== //
    // ====================================================== //

    public override void _Process(double delta)
    {
        // Recursively check if any child node has focus
        // If not, close the menu
        if (!AnyChildHasFocus(this))
            CloseMenu();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.GetType() != typeof(InputEventKey))
            return;

        var k = @event as InputEventKey;
        if (k.IsActionPressed("ui_add_tag", false, true) || k.IsActionPressed("ui_cancel", false, true))
        {
            CloseMenu();
            GetViewport().SetInputAsHandled();
        }
    }

    public void CloseMenu()
    {
        QueueFree();
    }
    public void CloseMenu(MsbtTagElement tag)
    {
        Array<TagWheelTagResult> result = [];
        result.Add(new TagWheelTagResult(tag));

        EmitSignal(SignalName.AddTag, [result]);
        CloseMenu();
    }
    public void CloseMenu(List<MsbtTagElement> tags)
    {
        Array<TagWheelTagResult> result = [];
        foreach (var tag in tags)
            result.Add(new TagWheelTagResult(tag));

        EmitSignal(SignalName.AddTag, [result]);
        CloseMenu();
    }

    public static bool AnyChildHasFocus(Control node)
    {
        if (node.HasFocus())
            return true;

        foreach (var child in node.GetChildren())
        {
            if (child is not Control && !child.GetType().IsSubclassOf(typeof(Control)))
                continue;

            if (AnyChildHasFocus(child as Control))
                return true;
        }

        return false;
    }
}