using Godot;

using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace MoonFlow.Scene.EditorMsbt;

public partial class Ruby : TagEditSceneWithText
{
	private MsbtTagElementSystemRuby Tag = null;

	public override void SetupScene(MsbtTagElement tag)
	{
		base.SetupScene(tag);

		Tag = tag as MsbtTagElementSystemRuby;

		var code = GetNode<SpinBox>("%Spin_Code");
		code.Value = Tag.Code;
	}

	private void SetCode(ushort id)
	{
		Tag.Code = id;
	}
}
