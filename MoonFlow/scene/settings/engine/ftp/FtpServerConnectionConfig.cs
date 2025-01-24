using Godot;
using System;

using MoonFlow.Project.FTP;

namespace MoonFlow.Scene.Settings;

public partial class FtpServerConnectionConfig : VBoxContainer
{
	[Export, ExportGroup("Internal References")]
	private LineEdit LineDomain;
	[Export]
	private LineEdit LinePort;
	[Export]
	private LineEdit LineUser;
	[Export]
	private LineEdit LinePass;

	[Export]
	private Label LabelConnectStatus;
	[Export]
	private Button ButtonConnect;
	[Export]
	private Button ButtonResetCredentials;
	private const string StatusContext = "ENGINE_SETTINGS_FTP_CONNECTION_STATUS";

	public override void _Ready()
	{
		var cred = ProjectFtpClient.CredentialStore;
		LineDomain.Text = cred.Host;
		LinePort.Text = cred.Port.ToString();
		LineUser.Text = cred.User;
		LinePass.Text = cred.Pass;

		ButtonConnect.Disabled = cred.Host == string.Empty;

		if (ProjectFtpClient.IsConnected())
		{
			ButtonConnect.Text = Tr("ButtonUpdate", StatusContext);
			ButtonResetCredentials.Show();

			LabelConnectStatus.Text = Tr("OK", StatusContext);
			LabelConnectStatus.SelfModulate = Colors.LightGreen;
		}
		else
		{
			ButtonConnect.Text = Tr("ButtonInitial", StatusContext);
			ButtonResetCredentials.Hide();

			LabelConnectStatus.Text = Tr("Disconnected", StatusContext);
			LabelConnectStatus.SelfModulate = Colors.IndianRed;
		}
	}

	#region Signals

	private async void OnPressedConnect()
	{
		LabelConnectStatus.Text = Tr("Pending", StatusContext);
		LabelConnectStatus.SelfModulate = Colors.LightYellow;
		ButtonConnect.Disabled = true;
		ButtonResetCredentials.Disabled = true;

		// Save login credentials before attempting connection
		ProjectFtpClient.CredentialStore.Save();

		if (ProjectFtpClient.IsConnected())
			ProjectFtpClient.Disconnect();

		if (await ProjectFtpClient.TryConnect())
		{
			ButtonConnect.Text = Tr("ButtonUpdate", StatusContext);
			ButtonResetCredentials.Show();

			LabelConnectStatus.Text = Tr("OK", StatusContext);
			LabelConnectStatus.SelfModulate = Colors.LightGreen;
		}
		else
		{
			ButtonConnect.Text = Tr("ButtonRetry", StatusContext);
			ButtonResetCredentials.Show();

			LabelConnectStatus.Text = Tr("Failed", StatusContext);
			LabelConnectStatus.SelfModulate = Colors.IndianRed;
		}

		ButtonConnect.Disabled = false;
		ButtonResetCredentials.Disabled = false;
	}

	private void OnPressedClearAllCredentials()
	{
		var cred = ProjectFtpClient.CredentialStore;
		cred.Host = "";
		cred.Port = 0;
		cred.User = "";
		cred.Pass = "";
		cred.Save();

		LineDomain.Clear();
		LinePort.Clear();
		LineUser.Clear();
		LinePass.Clear();

		if (ProjectFtpClient.IsConnected())
			ProjectFtpClient.Disconnect();
		
		ProjectFtpClient.StatusIndicator.SetStatusDisabled();
		
		ButtonConnect.Text = Tr("ButtonInitial", StatusContext);
		ButtonResetCredentials.Hide();

		LabelConnectStatus.Text = Tr("Disconnected", StatusContext);
		LabelConnectStatus.SelfModulate = Colors.IndianRed;
	}

	private void OnSetDomain(string v)
	{
		ProjectFtpClient.CredentialStore.Host = v;
		ButtonConnect.Disabled = v == string.Empty;
	}
	private static void OnSetPort(string v)
	{
		if (v == string.Empty)
		{
			ProjectFtpClient.CredentialStore.Port = 0;
			return;
		}

		ProjectFtpClient.CredentialStore.Port = v.ToInt();
	}
	private static void OnSetUser(string v) { ProjectFtpClient.CredentialStore.User = v; }
	private static void OnSetPass(string v) { ProjectFtpClient.CredentialStore.Pass = v; }

	#endregion
}
