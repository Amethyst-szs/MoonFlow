using Godot;

using MoonFlow.Project;
using MoonFlow.Project.FTP;
using MoonFlow.Scene.Settings;
using MoonFlow.Scene.Tools;

namespace MoonFlow.Scene.Main;

public partial class ActionbarTools : ActionbarItemBase
{
	private bool IsReady = false;

	private enum MenuIds : int
	{
		ALBUM_FTP = 0
	}

	private void InitContent()
	{
		if (ProjectFtpClient.StatusIndicator == null)
			return;

		base._Ready();

		ProjectFtpClient.StatusIndicator.AttachEventConnected(OnFtpConnectionStatusConnected);
		ProjectFtpClient.StatusIndicator.AttachEventDisconnected(OnFtpConnectionStatusDisconnected);

		AssignFunction((int)MenuIds.ALBUM_FTP, OnOpenAlbumFtpApplication);
		SetItemTooltip(GetItemIndex((int)MenuIds.ALBUM_FTP), Tr("AlbumFtp", "HEADER_TOOLTIP"));

		OnFtpConnectionStatus(ProjectFtpClient.IsConnected());
		IsReady = true;
	}

	public override void _Process(double _)
	{
		if (!IsReady)
			InitContent();
	}

	private void OnFtpConnectionStatusConnected() => OnFtpConnectionStatus(true);
	private void OnFtpConnectionStatusDisconnected() => OnFtpConnectionStatus(false);
	private void OnFtpConnectionStatus(bool isCon)
	{
		SetItemDisabled(GetItemIndex((int)MenuIds.ALBUM_FTP), !isCon);
	}

	private void OnOpenAlbumFtpApplication()
	{
		AppSceneServer.CreateApp<AlbumFtp>("");
	}
}
