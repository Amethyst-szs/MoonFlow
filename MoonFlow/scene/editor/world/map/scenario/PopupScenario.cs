using Godot;
using MoonFlow.Project;
using MoonFlow.Project.Database;
using System;

namespace MoonFlow.Scene.EditorWorld;

public partial class PopupScenario : PopupPanel
{
    private TabMap Parent = null;

    [Export, ExportGroup("Internal References")]
    private Control ScenarioPicker;

    [Export]
    private Label LabelTypeDefault;
    [Export]
    private Label LabelTypeOverride;
    [Export]
    private Button ButtonMakeOverride;
    [Export]
    private Button ButtonRemoveOverride;

    public override void _Ready()
    {
        Parent = this.FindParentByType<TabMap>();
    }
    public void InitInfo(WorldInfo world, int scenario, Map2dHolder mapHolder)
    {
        ScenarioPicker.Call("set_primary_bit", scenario - 1);

        bool isOverride = mapHolder.GetMap(scenario) != mapHolder.GetMap();

        LabelTypeDefault.Visible = !isOverride;
        LabelTypeOverride.Visible = isOverride;
        ButtonMakeOverride.Visible = !isOverride;
        ButtonRemoveOverride.Visible = isOverride;
    }

    #region Signals

    [Signal]
    public delegate void ScenarioPickedEventHandler(int scenario);
    [Signal]
    public delegate void MakeScenarioOverrideEventHandler();
    [Signal]
    public delegate void RemoveScenarioOverrideEventHandler();

    private void OnScenarioPickerSelection(int scenario) => EmitSignalScenarioPicked(scenario + 1);
    private void OnPressedMakeScenarioOverride() => EmitSignalMakeScenarioOverride();
    private void OnPressedRemoveScenarioOverride() => EmitSignalRemoveScenarioOverride();

    #endregion
}
