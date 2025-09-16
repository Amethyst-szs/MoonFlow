using Godot;
using System;

namespace MoonFlow.Scene.Home;

public partial class ToolListEntry : Button
{
    [Export]
    private PackedScene ToolScene = null;
    [Export]
    private Node ToolDestination = null;

    public override void _Ready()
    {
        Pressed += OnToolButtonSelected;

        if (ButtonPressed)
            OnToolButtonSelected();
    }
    public override void _ExitTree()
    {
        Pressed -= OnToolButtonSelected;
    }

    private void OnToolButtonSelected()
    {
        // If button was deactivated (pressed while already active), reset state and return
        if (!ButtonPressed)
        {
            SetPressedNoSignal(true);
            return;
        }

        // Remove selection status of all neighboring ToolListEntry buttons
        foreach (var child in GetParent().GetChildren())
        {
            if (child is not ToolListEntry toolEntry)
                continue;

            if (toolEntry != this)
                toolEntry.SetPressedNoSignal(false);
        }

        // Instantiate the tool scene in the tool destination
        ToolDestination.QueueFreeAllChildren();

        var inst = ToolScene.Instantiate();
        ToolDestination.AddChild(inst);
    }
}
