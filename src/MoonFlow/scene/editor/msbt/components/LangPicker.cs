using Godot;
using System;

using Nindot.Al.Localize;
using System.Linq;

namespace MoonFlow.Scene.EditorMsbt;

public partial class LangPicker : OptionButton
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		foreach (var item in LanguageKeyTranslator.Table)
			AddItem(item.Value);
	}

	public void SetSelection(string langCode)
	{
		int idx = LanguageKeyTranslator.Table.Keys.ToList().FindIndex(s => s == langCode);
		Selected = idx;
	}
}
