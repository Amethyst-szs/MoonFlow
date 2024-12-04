using System;
using System.Linq;

using Godot;
using Godot.Collections;

namespace MoonFlow.LMS.Msbt;

public partial class MsbtPageEditor : TextEdit
{
    private void SpawnTagWheel(int line, int column, bool isMouseSpawner)
    {
        // Only allow spawning the wheel if currently editable
        if (!Editable)
            return;

        Editable = false;

        // Calculate start of mouse line position
        var caretOrigin = GetRectAtLineColumn(line, column);
        var caretPos = caretOrigin.GetCenter() + (Vector2I)GlobalPosition;
        caretPos.X += caretOrigin.Size.X / 2;

        // Create wheel and connect to various signals
        var wheel = SceneCreator<TagWheel>.Create();
        wheel.Parent = this;
        wheel.CaretPosition = caretPos;
        wheel.TreeExiting += EndTagInsertMenu;

        wheel.Connect(TagWheel.SignalName.FinishedAddTag,
            Callable.From(new Action<TagWheel, TagWheelTagResult>(CloseTagWheel)));

        wheel.Connect(TagWheel.SignalName.MigrateSubmenu,
            Callable.From(new Action<TagWheel, TagSubmenuBase>(MigrateTagSubmenu)));

        // Assign wheel's global position
        if (isMouseSpawner)
            wheel.SetGlobalPosition((Vector2I)GetGlobalMousePosition());
        else
            wheel.SetGlobalPosition(GlobalPosition + (Size / 2));

        // Add wheel as child of scene
        GetTree().CurrentScene.AddChild(wheel);
    }

    private void InsertTag(TagWheelTagResult result)
    {
        HandleTagInput(result.Tag, 0);
    }
    private void InsertTagList(Array<TagWheelTagResult> result)
    {
        foreach (var tag in result)
            HandleTagInput(tag.Tag, 0);
    }

    private void EndTagInsertMenu()
    {
        Editable = true;
        GrabFocus();
    }

    private void CloseTagWheel(TagWheel wheel, TagWheelTagResult result)
    {
        // Remove tree exit event
        wheel.TreeExiting -= EndTagInsertMenu;

        // Insert tag from result
        InsertTag(result);
        EndTagInsertMenu();
    }

    private void MigrateTagSubmenu(TagWheel wheel, TagSubmenuBase menu)
    {
        // Remove tree exit event
        wheel.TreeExiting -= EndTagInsertMenu;

        // Connect to submenu
        menu.TreeExiting += EndTagInsertMenu;
        menu.Connect(TagSubmenuBase.SignalName.AddTag,
            Callable.From(new Action<Array<TagWheelTagResult>>(InsertTagList)));
    }
}
