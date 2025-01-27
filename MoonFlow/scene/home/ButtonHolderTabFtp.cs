using Godot;
using MoonFlow.Project.FTP;
using System;

namespace MoonFlow.Scene.Home;

public partial class ButtonHolderTabFtp : HBoxContainer
{
	public override void _Ready()
	{
		Visible = ProjectFtpClient.IsConnected();

		ProjectFtpClient.StatusIndicator.AttachEventConnected(Show);
		ProjectFtpClient.StatusIndicator.AttachEventDisconnected(Hide);
	}
}
