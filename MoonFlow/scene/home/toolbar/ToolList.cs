using Godot;
using System;

namespace MoonFlow.Scene.Home;

public partial class ToolList : VBoxContainer
{
    public override void _Ready()
    {
        // Setup shortcut hotkey for all child entries
        int entryIdx = 0;

        foreach (var child in GetChildren())
        {
            if (child is not ToolListEntry entry)
                continue;

            var key = new InputEventKey();

            // Calculate keycode
            int keycode = entryIdx + 0x31;
            if (keycode == 0x3A)
                keycode = 0x30;
            else if (keycode > 0x3A)
                continue;

            key.Keycode = (Key)keycode;
            key.CommandOrControlAutoremap = true;

            // Create and assign shortcut to entry
            var shortcut = new Shortcut();
            shortcut.Events.Add(key);
            entry.Shortcut = shortcut;

            // Advance entry index
            entryIdx++;
        }
    }
}
