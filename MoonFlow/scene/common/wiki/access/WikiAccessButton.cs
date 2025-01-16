using Godot;
using Godot.Collections;
using MoonFlow.Scene.Main;
using System;
using System.Linq;

namespace MoonFlow.Scene;

[GlobalClass]
public partial class WikiAccessButton : Button
{
	[Export]
	private WikiAccessorResource WikiTarget;

	private string TooltipTextBase = "";

	public override void _Ready()
	{
		if (Engine.IsEditorHint())
			return;

		TooltipTextBase = TooltipText;

		SetupTooltipText();
		VisibilityChanged += SetupTooltipText;
	}

	public override void _Pressed() { WikiTarget.OpenWiki(); }

	#region Utility

	private void SetupTooltipText()
	{
		const string context = "WIKI_BUTTON_TOOLTIP";
		var pathBase = EngineSettings.GetWiki();

		string notice = "";
		string pathPrefix;

		if (pathBase.StartsWith("https://"))
			pathPrefix = Tr("UrlNotice", context);
		else if (pathBase.StartsWith("res://"))
			pathPrefix = Tr("LocalNotice", context);
		else
			pathPrefix = "";

		if (EngineSettings.GetSetting<bool>("moonflow/wiki/is_display_toggle_notice", true))
			notice = Tr("SettingNotice", context);

		TooltipText = string.Format("{0}\n{1}: {2}\n{3}",
			Tr(TooltipTextBase),
			pathPrefix,
			WikiTarget.LocalPath.TrimSuffix(".md").ToPascalCase(),
			notice
		);
	}

	#endregion
}
