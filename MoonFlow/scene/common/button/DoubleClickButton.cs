using Godot;
using System;

namespace MoonFlow;

[GlobalClass]
public partial class DoubleClickButton : Button
{
    private Timer DoubleTimer = null;

    [Signal]
    public delegate void DoublePressedEventHandler();

    public override void _Ready()
    {
        ActionMode = ActionModeEnum.Press;
    }

    public override void _Pressed()
    {
        if (DoubleTimer != null)
        {
            DoubleTimer.QueueFree();
            DoubleTimer = null;
            EmitSignalDoublePressed();
            return;
        }

        DoubleTimer = new Timer
        {
            WaitTime = 0.22,
            OneShot = true,
            Autostart = true
        };
        DoubleTimer.Connect(Timer.SignalName.Timeout, Callable.From(OnTimerTimeout));
        AddChild(DoubleTimer);
    }

    private void OnTimerTimeout()
    {
        DoubleTimer?.QueueFree();
        DoubleTimer = null;
    }
}
