using System;
using System.Numerics;
using Godot;
using Godot.Collections;
using Nindot.LMS.Msbt;

using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace MoonFlow.Scene.EditorMsbt;

public partial class Picture : TagEditIconBase
{
	public override void SetupScene(MsbtTagElement tag, MsbtPage page)
	{
		base.SetupScene(tag, page);

		var menu = TagEditIconCommon.InitSubmenu<TagSubmenuPictureFont>(Tag);
		SetupSubmenu(menu);
	}
}
