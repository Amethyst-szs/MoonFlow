using Nindot.LMS.Msbt;

using Nindot.LMS.Msbt.TagLib;

namespace MoonFlow.Scene.EditorMsbt;

public partial class Device : TagEditIconBase
{
	public override void SetupScene(MsbtTagElement tag, MsbtPage page)
	{
		base.SetupScene(tag, page);

		var menu = TagEditIconCommon.InitSubmenu<TagSubmenuDeviceFont>(Tag);
		SetupSubmenu(menu);
	}
}
