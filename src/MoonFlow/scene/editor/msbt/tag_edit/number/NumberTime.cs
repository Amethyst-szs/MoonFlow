using Godot;
using System;

using Nindot.LMS.Msbt.TagLib.Smo;
using Nindot.LMS.Msbt.TagLib;
using Nindot.Al.SMO;

using MoonFlow.Project;

namespace MoonFlow.LMS.Msbt;

public partial class NumberTime : TagEditSceneWithText
{
	private MsbtTagElementNumberTime Tag = null;

    public override void SetupScene(MsbtTagElement tag)
    {
        base.SetupScene(tag);

		Tag = tag as MsbtTagElementNumberTime;;

		var option = GetNode<OptionButton>("%Option_TagName");
		option.Selected = Tag.GetTagName() - (ushort)TagNameNumber.Date;

		// If project is before version 1.2.0, remove EU options in menu
		var ver = ProjectManager.GetProjectVersion();
		if (ver >= RomfsValidation.RomfsVersion.v120)
			return;
		
		int removeIdx = (ushort)TagNameNumber.DateEU - (ushort)TagNameNumber.Date;
		option.RemoveItem(removeIdx);
		option.RemoveItem(removeIdx);
    }

	private void OnTagNameOptionSelected(int option)
	{
		option += (int)TagNameNumber.Date;

		if (option > (int)TagNameNumber.DateDetailEU)
			throw new Exception("Invalid TagName!");

		Tag.SetTagNameDangerous((ushort)option);
	}
}
