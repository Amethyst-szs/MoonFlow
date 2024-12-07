using Godot;
using System;

using Nindot.LMS.Msbt.TagLib.Smo;
using Nindot.LMS.Msbt.TagLib;

namespace MoonFlow.LMS.Msbt;

public partial class NumberFigure : TagEditSceneWithText
{
	private MsbtTagElementNumberWithFigure Tag = null;

    public override void SetupScene(MsbtTagElement tag)
    {
        base.SetupScene(tag);

		Tag = tag as MsbtTagElementNumberWithFigure;

		var fig = GetNode<SpinBox>("%Spin_Figure");
		fig.Value = Tag.Figure;

		var jp = GetNode<CheckBox>("%Check_JapaneseZenkaku");
		jp.ButtonPressed = Tag.IsJapaneseZenkaku != 0;

		var option = GetNode<OptionButton>("%Option_TagName");
		option.Selected = Tag.GetTagName();
    }

	private void OnFigureChanged(float value)
	{
		Tag.Figure = (ushort)value;
	}
	private void OnJapaneseZenkakuToggled(bool state)
	{
		Tag.IsJapaneseZenkaku = (ushort)(state ? 1 : 0);
	}
	private void OnTagNameOptionSelected(int option)
	{
		if (option < (int)TagNameNumber.Score || option > (int)TagNameNumber.CoinNum)
			throw new Exception("Invalid TagName!");

		Tag.SetTagNameDangerous((ushort)option);
	}
}
