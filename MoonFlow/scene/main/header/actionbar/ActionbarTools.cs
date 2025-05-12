using Godot;

using MoonFlow.Project;
using MoonFlow.Project.FTP;
using MoonFlow.Scene.Settings;
using MoonFlow.Scene.Tools;

namespace MoonFlow.Scene.Main;

public partial class ActionbarTools : ActionbarItemBase
{
	private enum MenuIds : int
	{
		ALBUM_FTP = 0
	}

    public override void _Ready()
    {
		if (ProjectFtpClient.StatusIndicator == null) 
		{
			CallDeferred(MethodName._Ready);
			return;
		}

		base._Ready();

		ProjectFtpClient.StatusIndicator.AttachEventConnected(OnFtpConnectionStatusConnected);
		ProjectFtpClient.StatusIndicator.AttachEventDisconnected(OnFtpConnectionStatusDisconnected);

		AssignFunction((int)MenuIds.ALBUM_FTP, OnOpenAlbumFtpApplication);
		SetItemTooltip(GetItemIndex((int)MenuIds.ALBUM_FTP), Tr("AlbumFtp", "HEADER_TOOLTIP"));

		OnFtpConnectionStatus(ProjectFtpClient.IsConnected());
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
