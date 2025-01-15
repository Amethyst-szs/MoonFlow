using Godot;
using System;

using static Nindot.RomfsPathUtility;
using static MoonFlow.Project.Global;

using MoonFlow.Project;

namespace MoonFlow.Scene;

public partial class LangPicker : OptionButton
{
	private const string DisplayNameContext = "PROJECT_LANGUAGE_CODE";

	[Export(PropertyHint.Enum, "Default Language:0,Translation Language:1")]
	private int StartingLanguage = 0;

	[Export]
	private bool AutomaticallySetGameVersion = false;

	[Signal]
	public delegate void LangSelectedByEngineEventHandler(string lang, int idx);
	[Signal]
	public delegate void LangSelectedByUserEventHandler(string lang, int idx);

	public override void _Ready()
	{
		ItemSelected += OnItemSelected;

		foreach (var item in ProjectLanguageList)
			AddItem(Tr(item, DisplayNameContext));

		string lang = null;
		if (IsStartingLanguageDefault())
			lang = ProjectManager.GetDefaultLang();
		else if (IsStartingLanguageTranslation())
			lang = EngineSettings.GetSetting<string>("moonflow/localization/translation_language", "USen");

		if (lang != null)
			SetSelection(lang);

		if (AutomaticallySetGameVersion)
			SetGameVersion(ProjectManager.GetRomfsVersion());
	}

	private void OnItemSelected(long index)
	{
		var lang = ProjectLanguageList[(int)index];
		EmitSignal(SignalName.LangSelectedByUser, lang, (int)index);
	}

	#region Utility

	public void SetSelection(string langCode)
	{
		int idx = Array.FindIndex(ProjectLanguageList, s => s == langCode);
		if (idx == -1)
			throw new Exception("Unknown language code " + langCode);

		Selected = idx;
		EmitSignal(SignalName.LangSelectedByEngine, langCode, idx);
	}

	public void SetGameVersion(RomfsVersion ver)
	{
		if (ver == RomfsVersion.INVALID_VERSION)
			return;

		int korean = Array.FindIndex(ProjectLanguageList, s => s == "KRko");
		if (korean == -1)
			throw new Exception("Could not find index of korean language!");

		SetItemDisabled(korean, ver < RomfsVersion.v110);

		if (Selected == korean)
			SetSelection("USen");
	}

	public bool IsStartingLanguageDefault() { return StartingLanguage == 0; }
	public bool IsStartingLanguageTranslation() { return StartingLanguage == 1; }

	#endregion
}
