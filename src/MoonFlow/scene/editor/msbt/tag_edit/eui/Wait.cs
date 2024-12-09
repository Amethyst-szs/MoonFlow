using Godot;
using System;

using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace MoonFlow.Scene.EditorMsbt;

public partial class Wait : TagEditScene
{
	private MsbtTagElementEuiWait Tag = null;

	public override void SetupScene(MsbtTagElement tag)
	{
		base.SetupScene(tag);

		Tag = tag as MsbtTagElementEuiWait;

		var slider = GetNode<SpinBox>("%Spin_Frames");
		slider.Value = Tag.DelayFrames;
	}

	private void OnDelayFramesSliderChanged(float value)
	{
		Tag.DelayFrames = (ushort)value;
	}
}
