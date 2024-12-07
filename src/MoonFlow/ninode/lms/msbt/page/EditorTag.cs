using System;
using System.Linq;

using Godot;
using Godot.Collections;
using MoonFlow.Project;

using Nindot.LMS.Msbt.TagLib;

namespace MoonFlow.LMS.Msbt;

public partial class MsbtPageEditor : TextEdit
{
    // ====================================================== //
    // =============== Tag Wheel Functionality ============== //
    // ====================================================== //

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

    // ====================================================== //
    // =============== Tag Edit Functionality =============== //
    // ====================================================== //

    private bool TryOpenTagEdit(int charIdx, Vector2I spawnPosition)
    {
        // Get the element at the current character index
        var tag = TagEditGetTargetElement(charIdx - 1);
        if (tag == null)
        {
            // If failed, try again with the previous index
            tag = TagEditGetTargetElement(charIdx);
            if (tag == null)
                return false;
        }

        // Create scene and add to root
        var scene = TagEditFactory.Create(tag);
        if (scene == null)
            return false;
        
        var root = ProjectManager.SceneRoot;
        if (!IsInstanceValid(root))
            return false;
        
        root.AddChild(scene);

        // Setup scene
        scene.Position = (Vector2I)(GlobalPosition + spawnPosition);

        Editable = false;
        scene.TreeExiting += OnTagEditSceneClose;

        scene.SetupScene(tag);

        return true;
    }
    
    private MsbtTagElement TagEditGetTargetElement(int charIdx)
    {
        if (charIdx < 0)
            return null;
        
        int elementIdx = Page.CalcElementIdxAtCharPos(charIdx);
        if (elementIdx < 0 || elementIdx >= Page.Count)
            return null;

        MsbtBaseElement element = Page[elementIdx];
        if (!element.IsTag())
            return null;

        return element as MsbtTagElement;
    }

    private void OnTagEditSceneClose()
    {
        SyntaxHighlighter.ClearHighlightingCache();
        QueueRedraw();
        
        Editable = true;
    }
}
