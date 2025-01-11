using Godot;
using System;

using static Nindot.RomfsPathUtility;

using MoonFlow.Project;

namespace MoonFlow.Scene;

public partial class LangPicker : OptionButton
{
	private static readonly string[] LangList = ["CNzh", "EUde", "EUen", "EUes", "EUfr", "EUit", "EUnl", "EUru", "JPja", "KRko", "TWzh", "USen", "USes", "USfr"];
	private const string DisplayNameContext = "PROJECT_LANGUAGE_CODE";

	[Signal]
	public delegate void LangSelectedByEngineEventHandler(string lang, int idx);
	[Signal]
	public delegate void LangSelectedByUserEventHandler(string lang, int idx);

	public override void _Ready()
	{
		ItemSelected += OnItemSelected;

		foreach (var item in LangList)
			AddItem(Tr(item, DisplayNameContext));

		var lang = ProjectManager.GetProject()?.Config?.Data?.DefaultLanguage;
		if (lang != null)
			SetSelection(lang);
	}

	public void SetSelection(string langCode)
	{
		int idx = Array.FindIndex(LangList, s => s == langCode);
		if (idx == -1)
			throw new Exception("Unknown language code " + langCode);

		Selected = idx;
		EmitSignal(SignalName.LangSelectedByEngine, langCode, idx);
	}

	public void SetGameVersion(RomfsVersion ver)
	{
		int korean = Array.FindIndex(LangList, s => s == "KRko");
		if (korean == -1)
			throw new Exception("Could not find index of korean language!");

		SetItemDisabled(korean, ver < RomfsVersion.v110);

		if (Selected == korean)
			SetSelection("USen");
	}

	private void OnItemSelected(long index)
	{
		var lang = LangList[(int)index];
		EmitSignal(SignalName.LangSelectedByUser, lang, (int)index);
	}
}
