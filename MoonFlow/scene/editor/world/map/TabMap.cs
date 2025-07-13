using Godot;
using System;
using System.Threading.Tasks;

using MoonFlow.Project;
using MoonFlow.Project.Database;
using System.Numerics;

namespace MoonFlow.Scene.EditorWorld;

public partial class TabMap : TextureRect
{
    private Map2d Map = null;
    private CheckpointFlagDbFile CheckpointInfo = null;

    private WorldEditorApp Parent = null;
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

    public async void InitMap()
    {
        Parent = this.FindParentByType<WorldEditorApp>() ?? throw new NullReferenceException();
        PreviewScenario = Parent.World.MoonRockScenario;

        RenderMapDisable();

        // Load map and other databases
        var db = ProjectManager.GetDB();
        Map = await db.TryCreateOrGetMap2d(Parent.World, PreviewScenario);
        CheckpointInfo = await CheckpointFlagDbGenerator.CreateInfoAsync(db, Parent.World, PreviewScenario);

        SliderDragSensitivity.Value = EngineSettings.GetSetting<float>(DragSensivityKey, 50.0f);

        await RenderMap();
        RenderIcons();
    }

    #region Rendering

    private void RenderMapDisable()
    {
        LabelLoading.Show();
        SelfModulate = Colors.Black;
    }
    private async Task RenderMap()
    {
        LabelLoading.Show();

        ImageTexture tex = await Map2dRenderUtility.GetMapImageTexture(Parent.World, PreviewScenario);
        Texture = tex;
        SelfModulate = Colors.White;

        LabelLoading.Hide();
    }
    private void RenderIcons()
    {
        if (Parent == null || Parent.World == null || Map == null)
            return;

        // Get map information
        Map.RecalculateViewProjMatrix();

        // Render icons
        var shineList = Parent.World.ShineList;
        Map2dRenderUtility.RenderShineIcons(Map, Size, IconHolder, shineList, HoverShine, TextureShine);
        Map2dRenderUtility.RenderCheckpointIcons(Map, Size, IconHolder, CheckpointInfo, TextureCheckpoint);

        Map2dRenderUtility.RenderOriginPoint(Map, Size, IconHolder, TextureOrigin);
    }

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
    private void OnRefreshButton() => InitMap();

    private void OnSetDragAdjustSensitivity(bool isChanged)
    {
        if (!isChanged)
            return;

        EngineSettings.SetSetting(DragSensivityKey, SliderDragSensitivity.Value);
        EngineSettings.Save();
    }
    private void OnSetRotateMapSpinbox(float value)
    {
        Parent.OnMapInfoModify();

        Map.RotateViewMatrix(value);
        RenderIcons();

        SpinMapRotate.Value = 0;
    }
    private void OnSetScaleMapSpinbox(float value)
    {
        Parent.OnMapInfoModify();

        Map.ScaleViewMatrix(value / 100.0f);
        RenderIcons();

        SpinMapScale.Value = 100;
    }
    private void OnUndoMatrixModifications()
    {
        Parent.OnMapInfoModify();

        Map.SetInternalMatrices(MatrixBackupProj, MatrixBackupView);
        RenderIcons();
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

        var map = await ProjectManager.GetDB().TryCreateOrGetMap2d(Parent.World, PreviewScenario);

        // Modify translation of ViewMatrix
        map.DragViewMatrix(offset);

        Parent.OnMapInfoModify();
        RenderIcons();
    }

    #endregion
}
