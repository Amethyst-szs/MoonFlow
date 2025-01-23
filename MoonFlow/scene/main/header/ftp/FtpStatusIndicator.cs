using Godot;
using MoonFlow.Project;
using System;

namespace MoonFlow.Scene.Main;

public partial class FtpStatusIndicator : HBoxContainer, IProjectFtpStatusIndicator
{
	private const string AnimPlayerName = "Animation";
	private const string TooltipContext = "FTP_STATUS_INDICATOR_TOOLTIP";

	public void SetStatusActive() => UpdateStatus("active", CursorShape.Busy);
	public void SetStatusConnecting() => UpdateStatus("try_connect", CursorShape.Busy);

	public void SetStatusConnected() => UpdateStatus("connect", CursorShape.Arrow);
	public void SetStatusDisconnected() => UpdateStatus("disconnect", CursorShape.Forbidden);

	public void SetStatusDisabled() => UpdateStatus("off", CursorShape.Help);

	private void UpdateStatus(string anim, CursorShape shape)
	{
		GetNode<AnimationPlayer>(AnimPlayerName).Play(anim);
		TooltipText = Tr(anim, TooltipContext);

		MouseDefaultCursorShape = shape;
	}
}
