using Godot;
using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;
using System;

namespace MoonFlow.LMS.Msbt;

public partial class Font : TagEditScene
{
	private MsbtTagElementSystemFont Tag = null;

    public override void SetupScene(MsbtTagElement tag)
    {
        base.SetupScene(tag);

		Tag = tag as MsbtTagElementSystemFont;

		var currentLabel = GetNode<Label>("%Label_Current");
		currentLabel.Text += Tr("TAG_EDIT_SCENE_FONT_OPTION_" + Enum.GetName(Tag.Font));
    }

	private void SetFont(int id)
	{
		Tag.Font = (TagFontIndex)id;
		QueueFree();
	}
}
