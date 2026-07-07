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

        // Setup swatches
        var colorResolver = ProjectManager.GetMSBPHolder().ColorResolver;
        var swatchHolder = GetNode<HBoxContainer>("%Swatch");
        int maxColorCount = colorResolver.ColorGradiationList.Count;
        
        for (int i = 0; i < maxColorCount; i++)
        {
            var gradiation = colorResolver.ColorGradiationList[i];

            var button = SceneCreator<ColorTagSceneSwatch>.Create();
            button.Init(gradiation);
            button.Connect(BaseButton.SignalName.Pressed, Callable.From(() => OnSwatchSelect(button)));

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
