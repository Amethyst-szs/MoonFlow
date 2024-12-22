using Godot;
using System;
using System.Linq;

using Nindot.Al.Localize;
using Nindot.Al.SMO;

namespace MoonFlow.Scene;

public partial class LangPicker : OptionButton
{
	[Signal]
    public delegate void LangSelectedEventHandler(string lang, int idx);

	public override void _Ready()
	{
		ItemSelected += OnItemSelected;

		foreach (var item in LanguageKeyTranslator.Table)
			AddItem(item.Value);
	}

	public void SetSelection(string langCode)
	{
		int idx = LanguageKeyTranslator.Table.Keys.ToList().FindIndex(s => s == langCode);
		Selected = idx;

		var lang = LanguageKeyTranslator.Table.Keys.ElementAt(idx);
		EmitSignal(SignalName.LangSelected, lang, idx);
	}

	public void SetGameVersion(RomfsValidation.RomfsVersion ver)
	{
		int korean = LanguageKeyTranslator.Table.Keys.ToList().FindIndex(s => s == "KRko");
		if (korean == -1)
			throw new Exception("Could not find index of korean language!");
		
		SetItemDisabled(korean, ver < RomfsValidation.RomfsVersion.v110);

		if (Selected == korean)
			Selected = LanguageKeyTranslator.Table.Keys.ToList().FindIndex(s => s == "USen");
	}

	private void OnItemSelected(long index)
	{
		var lang = LanguageKeyTranslator.Table.Keys.ElementAt((int)index);
		EmitSignal(SignalName.LangSelected, lang, (int)index);
	}
}
