using Godot;
using System;

using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace MoonFlow.Scene.EditorMsbt;

public partial class Text : TagEditScene
{
	private MsbtTagElementTextAnim Tag = null;

	public override void SetupScene(MsbtTagElement tag)
	{
		base.SetupScene(tag);

		Tag = tag as MsbtTagElementTextAnim;
	}

	private void SetAnimation(int id)
	{
		Tag.Anim = (TagNameTextAnim)id;
		QueueFree();
	}
}
