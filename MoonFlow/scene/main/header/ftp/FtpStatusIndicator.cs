using Godot;
using System;

using MoonFlow.Project;

using FluentFTP;
using MoonFlow.Project.FTP;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MoonFlow.Scene.Main;

public partial class FtpStatusIndicator : HBoxContainer, IProjectFtpStatusIndicator
{
	[Export, ExportGroup("Internal References")]
	private AnimationPlayer Animation;
	[Export]
	private ProgressBar ProgressBar;
	[Export]
	private Label ProgressLabel;

	#region Display

	private const string TooltipContext = "FTP_STATUS_INDICATOR_TOOLTIP";

	public void SetStatusActive(bool isForce = false) => UpdateStatus("active", CursorShape.Busy, isForce);
	public void SetStatusConnecting(bool isForce = false) => UpdateStatus("try_connect", CursorShape.Busy, isForce);

	public void SetStatusConnected(bool isForce = false) => UpdateStatus("connect", CursorShape.Arrow, isForce);
	public void SetStatusDisconnected(bool isForce = false) => UpdateStatus("disconnect", CursorShape.Forbidden, isForce);

	public void SetStatusDisabled(bool isForce = false) => UpdateStatus("off", CursorShape.Help, isForce);

	private async void UpdateStatus(string anim, CursorShape shape, bool isForce)
	{
		await Extension.WaitProcessFrame(this);

		ProgressBar.SetDeferred(PropertyName.Visible, anim == "active");

		if (Animation.CurrentAnimation != anim || isForce)
			Animation.CallDeferred(AnimationPlayer.MethodName.Play, anim);

		SetDeferred(PropertyName.TooltipText, Tr(anim, TooltipContext));
		SetDeferred(PropertyName.MouseDefaultCursorShape, (int)shape);
	}

	#endregion

	#region Progress Updates

	public void OnProgressUpdate(FtpProgress data)
	{
		// A value of -1 on progress indicates an indeterminate transfer
		ProgressBar.SetDeferred(ProgressBar.PropertyName.Indeterminate, data.Progress == -1F);

		// Update progress
		ProgressBar.SetDeferred(ProgressBar.PropertyName.Value, data.Progress);
		ProgressLabel.SetDeferred(Label.PropertyName.Text, data.RemotePath);

		// Generate tooltip
		var tooltip = string.Format("ETA: {0:c}\n{1}", data.ETA, data.TransferSpeedToString());
		ProgressBar.SetDeferred(PropertyName.TooltipText, tooltip);
	}

	#endregion

	#region Input

	public override async void _GuiInput(InputEvent @event)
	{
		if (@event is not InputEventMouseButton mouse || !mouse.Pressed)
			return;

		bool isCon = await ProjectFtpClient.IsConnectedStill();

		if (isCon && !ProjectFtpClient.IsTransferQueueActive())
		{
			SetStatusConnected(true);
			return;
		}

		if (!isCon && !ProjectFtpClient.IsAttemptingServerConnect())
			_ = ProjectFtpClient.TryConnect();
	}

	#endregion

	#region Signals

	[Signal]
	public delegate void FtpServerConnectedEventHandler();
	[Signal]
	public delegate void FtpServerDisconnectedEventHandler();

	public void AttachEventConnected(Action func) => Connect(SignalName.FtpServerConnected, Callable.From(func));
	public void AttachEventDisconnected(Action func) => Connect(SignalName.FtpServerDisconnected, Callable.From(func));
	public void DetachEventConnected(Action func) => Disconnect(SignalName.FtpServerConnected, Callable.From(func));
	public void DetachEventDisconnected(Action func) => Disconnect(SignalName.FtpServerDisconnected, Callable.From(func));

	public void EmitEventConnected() => CallThreadSafe(MethodName.EmitSignal, SignalName.FtpServerConnected);
	public void EmitEventDisconnected() => CallThreadSafe(MethodName.EmitSignal, SignalName.FtpServerDisconnected);

	#endregion
}
