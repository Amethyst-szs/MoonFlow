using Godot;

using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace MoonFlow.Scene.EditorMsbt;

public partial class TimeComponent : TagEditScene
{
	private MsbtTagElementTimeComponent Tag = null;

	public override void SetupScene(MsbtTagElement tag)
	{
		base.SetupScene(tag);

		Tag = tag as MsbtTagElementTimeComponent;

		var option = GetNode<OptionButton>("%Option_TagName");
		option.Selected = (int)Tag.TimeComponent;
	}

	private void SetTagName(int id)
	{
		Tag.TimeComponent = (TagNameTime)id;
	}
}
