using Godot;

using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace MoonFlow.Scene.EditorMsbt;

public partial class FontSize : TagEditScene
{
	private MsbtTagElementSystemFontSize Tag = null;

	public override void SetupScene(MsbtTagElement tag)
	{
		base.SetupScene(tag);

		Tag = tag as MsbtTagElementSystemFontSize;

		var slider = GetNode<HSlider>("%Slider");
		slider.SetValueNoSignal(Tag.FontSize);

		var spin = GetNode<SpinBox>("%Spin_Percent");
		spin.SetValueNoSignal(Tag.FontSize);
	}

	private void OnFontSizeSliderChanged(float value)
	{
		Tag.FontSize = (ushort)value;
	}
}
