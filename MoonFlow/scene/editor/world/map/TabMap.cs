using Godot;
using System;
using System.Threading.Tasks;

using MoonFlow.Project;
using MoonFlow.Project.Database;
using System.Numerics;

namespace MoonFlow.Scene.EditorWorld;

public partial class TabMap : TextureRect
{
    private Map2dHolder MapHolder = null;
    private Map2d Map = null;
    private CheckpointFlagDbFile CheckpointInfo = null;

    private WorldEditorApp Parent = null;
    private WorldInfo World = null;
    private int PreviewScenario = -1;
    private ShineInfo HoverShine = null;

    private bool IsAdjustingMap = false;
    private bool IsAdjustDragMap = false;
    private const string DragSensivityKey = "moonflow/map2d/drag_sensitivity";

    private Matrix4x4 MatrixBackupProj;
    private Matrix4x4 MatrixBackupView;

    [Export, ExportGroup("Internal References")]
    private Control IconHolder = null;
    [Export]
    private Label LabelLoading = null;
    [Export]
    private VBoxContainer MapControls = null;
    [Export]
    private PopupScenario PopupScenario = null;
    [Export]
    private HSlider SliderDragSensitivity = null;
    [Export]
    private SpinBox SpinMapRotate = null;
    [Export]
    private SpinBox SpinMapScale = null;

    [Export, ExportGroup("Map Icons")]
    private Texture2D TextureShine = null;
    [Export]
    private Texture2D TextureCheckpoint = null;
    [Export]
    private Texture2D TextureOrigin = null;

    public async Task InitMap()
    {
        Parent = this.FindParentByType<WorldEditorApp>() ?? throw new NullReferenceException();
        World = Parent.World;

        if (PreviewScenario == -1)
            PreviewScenario = Parent.World.MoonRockScenario;

        await InitMapInternal();
    }
    public async Task InitMap(WorldInfo world, int scenario)
    {
        World = world;
        PreviewScenario = scenario;

        await InitMapInternal();
    }
    private async Task InitMapInternal()
    {
        // Load map and other databases
        var db = ProjectManager.GetDB();

        MapHolder = await db.TryCreateOrGetMap2dHolder(World);
        Map = MapHolder.GetMap(PreviewScenario);

        CheckpointInfo = await CheckpointFlagDbGenerator.CreateInfoAsync(db, World, PreviewScenario);

        // Destroy children of IconHolder, will be re-created by the RenderIcons function
        IconHolder.QueueFreeAllChildren();

        // Setup drag sensitivity slider in adjustment menu
        SliderDragSensitivity.Value = EngineSettings.GetSetting<float>(DragSensivityKey, 50.0f);

        await RenderMap();
        RenderIcons();

        PopupScenario.InitInfo(World, PreviewScenario, MapHolder);
    }

    #region Rendering

    private async Task RenderMap()
    {
        ImageTexture tex = await Map2dRenderUtility.GetMapImageTexture(World, PreviewScenario);
        Texture = tex;
        SelfModulate = Colors.White;

        LabelLoading.Hide();
    }
    private void RenderIcons()
    {
        if (World == null || Map == null)
            return;

        // Get map information
        Map.RecalculateViewProjMatrix();

        // Render icons
        var shineList = World.ShineList;
        Map2dRenderUtility.RenderShineIcons(Map, Size, IconHolder, shineList, HoverShine, TextureShine);
        Map2dRenderUtility.RenderCheckpointIcons(Map, Size, IconHolder, CheckpointInfo, TextureCheckpoint);

        Map2dRenderUtility.RenderOriginPoint(Map, Size, IconHolder, TextureOrigin);
    }

    public void DisableMapControls() => MapControls.Hide();

    #endregion

    #region Signals

    public void OnShineHovered(WorldShineEditorHolder shine)
    {
        HoverShine = shine.Shine;
        RenderIcons();
    }
    public void OnShineUnhovered()
    {
        HoverShine = null;
        RenderIcons();
    }
    private void OnMapSizeChanged() => RenderIcons();
    private async void OnRefreshButton() => await InitMap();
    private async void OnExportReference() => await ReferenceImageExporter.CreateAndSaveImage(World, PreviewScenario);

    private void OnSetDragAdjustSensitivity(bool isChanged)
    {
        if (!isChanged)
            return;

        EngineSettings.SetSetting(DragSensivityKey, SliderDragSensitivity.Value);
        EngineSettings.Save();
    }
    private void OnSetRotateMapSpinbox(float value)
    {
        Parent?.OnMapInfoModify();

        Map.RotateViewMatrix(value);
        RenderIcons();

        SpinMapRotate.Value = 0;
    }
    private void OnSetScaleMapSpinbox(float value)
    {
        Parent?.OnMapInfoModify();

        Map.ScaleViewMatrix(value / 100.0f);
        RenderIcons();

        SpinMapScale.Value = 100;
    }
    private void OnUndoMatrixModifications()
    {
        Parent?.OnMapInfoModify();

        Map.SetInternalMatrices(MatrixBackupProj, MatrixBackupView);
        RenderIcons();
    }

    private void OnScenarioSelectionChanged(int scenario)
    {
        if (scenario < 1)
        {
            PopupScenario.InitInfo(World, PreviewScenario, MapHolder);
            return;
        }

        PreviewScenario = scenario;
        _ = InitMap();
    }
    private void OnScenarioMakeUnique()
    {
        Parent?.OnMapInfoModify();
        
        MapHolder.MakeScenarioUnique(PreviewScenario);
        _ = InitMap();
    }
    private void OnScenarioRemoveUnique()
    {
        Parent?.OnMapInfoModify();

        MapHolder.MakeScenarioNotUnique(PreviewScenario);
        _ = InitMap();
    }

    #endregion

    #region States & Input

    public void SetAdjustState(bool isAdjusting)
    {
        if (isAdjusting)
            SetStateAdjustment();
        else
            SetStatePassive();
    }
    public void SetStatePassive()
    {
        IsAdjustingMap = false;
        MouseDefaultCursorShape = CursorShape.Arrow;
    }
    public void SetStateAdjustment()
    {
        // Create backups of matrix transformations for undo button
        Map.GetInternalMatrices(out MatrixBackupProj, out MatrixBackupView);

        IsAdjustingMap = true;
        MouseDefaultCursorShape = CursorShape.Move;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouse && mouse.ButtonIndex == MouseButton.Left && mouse.IsReleased())
            IsAdjustDragMap = false;
    }
    public override async void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouse && mouse.ButtonIndex == MouseButton.Left && mouse.IsPressed())
        {
            IsAdjustDragMap = true;
            GetViewport().SetInputAsHandled();
            return;
        }
        
        if (!IsAdjustingMap || !IsAdjustDragMap)
            return;

        if (@event is not InputEventMouseMotion motion)
            return;

        // Fetch distance and map
        float sensitivity = EngineSettings.GetSetting<float>(DragSensivityKey, 50.0f);
        var offset = motion.ScreenRelative * sensitivity;

        var map = await ProjectManager.GetDB().TryCreateOrGetMap2d(World, PreviewScenario);

        // Modify translation of ViewMatrix
        map.DragViewMatrix(offset);

        Parent?.OnMapInfoModify();
        RenderIcons();
    }

    #endregion
}
