using Godot;
using System;

namespace MoonFlow.Scene.Home;

[SceneUid("uid://c6rv82608htru")]
public partial class ArchivePanel : PanelContainer
{
    [Export, ExportGroup("Custom Styleboxes")]
    private StyleBox StyleboxDefault;
    [Export]
    private StyleBox StyleboxSelected;

    [Export, ExportGroup("Internal References")]
    private Label LabelName = null;

    #region Initilization

    public void InitPanel(string arc, string path)
    {
        Deselect();

        Name = arc;
        LabelName.Set("label_text", arc);
    }

    #endregion

    #region Input

    [Signal]
    public delegate void ArchiveClickedEventHandler(ArchivePanel panel, bool isCtrl, bool isShift);

    public void Select()
    {
        SelfModulate = Colors.White;
        AddThemeStyleboxOverride("panel", StyleboxSelected);
    }
    public void Deselect()
    {
        SelfModulate = Colors.LightGray;
        AddThemeStyleboxOverride("panel", StyleboxDefault);
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouse)
            if (mouse.ButtonIndex == MouseButton.Left && mouse.Pressed)
                EmitSignalArchiveClicked(this, mouse.IsCommandOrControlPressed(), mouse.ShiftPressed);
    }

    #endregion
}
