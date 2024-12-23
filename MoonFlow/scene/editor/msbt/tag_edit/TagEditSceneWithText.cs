using Godot;
using System;

using Nindot.LMS.Msbt.TagLib;

namespace MoonFlow.Scene.EditorMsbt;

[GlobalClass]
public partial class TagEditSceneWithText : TagEditScene
{
	private MsbtTagElementWithTextData Tag = null;

	public override void SetupScene(MsbtTagElement tag)
	{
		Tag = tag as MsbtTagElementWithTextData;

		var edit = GetNode<LineEdit>("%Line_Text");
		edit.Text = Tag.Text;
		edit.GrabFocus();
		edit.CaretColumn = edit.Text.Length;
	}

	private void OnTextDataChanged(string text)
	{
		Tag.Text = text;
	}
}
