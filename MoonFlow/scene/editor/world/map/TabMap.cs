using Godot;
using System;
using System.Threading.Tasks;

using MoonFlow.Project;
using MoonFlow.Project.Database;

namespace MoonFlow.Scene.EditorWorld;

public partial class TabMap : TextureRect
{
    private WorldEditorApp Parent = null;
    private int PreviewScenario = -1;
    private ShineInfo HoverShine = null;

    [Export, ExportGroup("Internal References")]
    private Control IconHolder = null;
    [Export]
    private Label LabelLoading = null;

    [Export, ExportGroup("Map Icons")]
    private Texture2D TextureShine = null;

    public async void InitMap()
    {
        Parent = this.FindParentByType<WorldEditorApp>() ?? throw new NullReferenceException();
        PreviewScenario = Parent.World.MoonRockScenario;

        await RenderMap();
        RenderIcons();
    }

    #region Rendering

    private async Task RenderMap()
    {
        LabelLoading.Show();

        ImageTexture tex = await Map2dRenderUtility.GetMapImageTexture(Parent.World, PreviewScenario);
        SetDeferred(PropertyName.Texture, tex);
        SetDeferred(PropertyName.SelfModulate, Colors.White);

        LabelLoading.Hide();
    }
    private async void RenderIcons()
    {
        if (Parent == null || Parent.World == null)
            return;

        // Get map information
        var map = await ProjectManager.GetDB().TryCreateOrGetMap2d(Parent.World, PreviewScenario);

        // Render all shines as icon on map
        foreach (var shine in Parent.World.ShineList)
        {
            var id = GetShineNodeId(shine);
            var icon = GetOrCreateIcon(id, TextureShine);

            // Write tooltip text if not already written
            if (icon.TooltipText == string.Empty)
                icon.TooltipText = shine.LookupDisplayName(ProjectManager.GetMSBTArchives()?.StageMessage)?.GetRawText();

            Map2dRenderUtility.PositionMapIcon(this, map, icon, shine.Trans);

            // Set modulation and size depending on if the shine is hovered
            if (HoverShine == null)
            {
                icon.SetStateNothingFocused();
                continue;
            }

            if (shine == HoverShine)
            {
                icon.SetStateFocus();
                icon.MoveToFront();
            }
            else
            {
                icon.SetStateOtherFocused();
            }
        }
    }

    private MapIcon GetOrCreateIcon(string id, Texture2D icon)
    {
        // Lookup node in icon holder first
        Node iconNode = IconHolder.FindChild(id, false, false);
        if (iconNode != null && iconNode is MapIcon iconNodeTex)
            return iconNodeTex;

        // Create new node if lookup failed
        var mapIcon = SceneCreator<MapIcon>.Create();
        mapIcon.Name = id;
        mapIcon.Texture = icon;

        IconHolder.AddChild(mapIcon);
        return mapIcon;
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

    private void OnMapSizeChanged()
    {
        RenderIcons();
    }

    #endregion

    #region Utility

    private string GetShineNodeId(ShineInfo shine)
    {
        return string.Format("Shine_{0}_{1}", shine.UniqueId, shine.ObjId);
    }

    #endregion
}
