using Godot;

using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;
using MoonFlow.Project;
using Nindot.LMS;

namespace MoonFlow.Scene.EditorMsbt;

public partial class ColorTagScene : TagEditScene
{
    private MsbtTagElementSystemColor Tag;

    public override void SetupScene(MsbtTagElement tag)
    {
        base.SetupScene(tag);

        Tag = tag as MsbtTagElementSystemColor;

        // Get color list from project
        var msbp = ProjectManager.GetMSBP();
        var colorList = msbp.Color_GetList();
        var colorLabelList = msbp.Color_GetLabelList();

        if (colorList.Count != colorLabelList.Count)
            throw new LMSException("Color list and color label list are different lengths!");

        // Setup swatches
        var swatchHolder = GetNode<HBoxContainer>("%Swatch");
        for (int i = 0; i < colorList.Count; i++)
        {
            var e = colorList[i];
            var color = Color.Color8(e.R, e.G, e.B, e.A);
            var label = colorLabelList[i];

            var button = new Button
            {
                SelfModulate = color,
                CustomMinimumSize = new Vector2(32, 32),
                TooltipText = label,
            };

            button.Connect(Button.SignalName.Pressed, Callable.From(() => OnSwatchSelect(button)));

            swatchHolder.AddChild(button);

            if (Tag.GetColorIdx() == i)
                button.GrabFocus();
        }
    }

    private void OnSwatchSelect(Button button)
    {
        var id = button.GetIndex();

        Tag.SetColor(id);
        QueueFree();
    }

    private void OnSwatchResetColorSelect()
    {
        Tag.SetColorResetDefault();
        QueueFree();
    }
}
