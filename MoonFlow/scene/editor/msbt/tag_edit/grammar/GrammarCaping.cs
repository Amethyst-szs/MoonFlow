using Godot;
using System;

using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace MoonFlow.Scene.EditorMsbt;

public partial class GrammarCaping : TagEditScene
{
	private MsbtTagElementGrammar Tag = null;

	public override void SetupScene(MsbtTagElement tag)
	{
		base.SetupScene(tag);

		Tag = tag as MsbtTagElementGrammar;

		var option = GetNode<OptionButton>("%Option_TagName");
		if (Enum.IsDefined(Tag.Grammar))
			option.Selected = (int)Tag.Grammar;
		else
			option.Disabled = true;
	}

	private void SetTagName(int id)
	{
		Tag.Grammar = (TagNameGrammar)id;
	}
}
