using Godot;
using System;

using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace MoonFlow.Scene.EditorMsbt;

public partial class Speed : TagEditScene
{
	private MsbtTagElementEuiSpeed Tag = null;
	private SpinBox Edit = null;

	public override void SetupScene(MsbtTagElement tag)
	{
		base.SetupScene(tag);

		Tag = tag as MsbtTagElementEuiSpeed;

		Edit = GetNode<SpinBox>("%Spin_Percent");
		UpdateEditBox();
	}

	private void UpdateEditBox()
	{
		Edit.Value = Tag.PrintSpeed * 100F;
	}
	private void OnPrintSpeedSliderChanged(float value)
	{
		Tag.PrintSpeed = value / 100F;
	}

	private void SetSpeedSlow()
	{
		Tag.SetPrintSpeedSlow();
		UpdateEditBox();
	}
	private void SetSpeedNormal()
	{
		Tag.SetPrintSpeedNormal();
		UpdateEditBox();
	}
	private void SetSpeedFast()
	{
		Tag.SetPrintSpeedFast();
		UpdateEditBox();
	}
	private void SetSpeedVeryFast()
	{
		Tag.SetPrintSpeedVeryFast();
		UpdateEditBox();
	}
}
