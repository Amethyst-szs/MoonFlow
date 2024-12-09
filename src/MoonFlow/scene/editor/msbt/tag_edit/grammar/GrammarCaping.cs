using Godot;
using System;

using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace MoonFlow.LMS.Msbt;

public partial class GrammarCaping : TagEditScene
{
	private MsbtTagElementGrammar Tag = null;

    public override void SetupScene(MsbtTagElement tag)
    {
        base.SetupScene(tag);

		Tag = tag as MsbtTagElementGrammar;

		var option = GetNode<OptionButton>("%Option_TagName");
		option.Selected = (int)Tag.Grammar;
    }

	private void SetTagName(int id)
	{
		Tag.Grammar = (TagNameGrammar)id;
	}
}
