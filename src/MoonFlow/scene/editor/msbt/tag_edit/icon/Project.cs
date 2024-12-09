using System;
using System.Numerics;
using Godot;
using Godot.Collections;
using Nindot.LMS.Msbt;

using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace MoonFlow.LMS.Msbt;

public partial class Project : TagEditIconBase
{
	public override void SetupScene(MsbtTagElement tag, MsbtPage page)
	{
		base.SetupScene(tag, page);

		var proj = tag as MsbtTagElementProjectTag;
		var shine = TagNameProjectIcon.ShineIconCurrentWorld;
		var coin = TagNameProjectIcon.CoinCollectIconCurrentWorld;

		TagSubmenuBase menu;
		if (proj.Icon == shine || proj.Icon == coin)
			menu = TagEditIconCommon.InitSubmenu<TagSubmenuPictureFont>(Tag);
		else
			menu = TagEditIconCommon.InitSubmenu<TagSubmenuDeviceFont>(Tag);
		
		SetupSubmenu(menu);
	}
}
