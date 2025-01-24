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

	public void SetStatusActive() => UpdateStatus("active", CursorShape.Busy);
	public void SetStatusConnecting() => UpdateStatus("try_connect", CursorShape.Busy);

	public void SetStatusConnected() => UpdateStatus("connect", CursorShape.Arrow);
	public void SetStatusDisconnected() => UpdateStatus("disconnect", CursorShape.Forbidden);

	public void SetStatusDisabled() => UpdateStatus("off", CursorShape.Help);

	private async void UpdateStatus(string anim, CursorShape shape)
	{
		await Extension.WaitProcessFrame(this);
		
		if (Animation.CurrentAnimation != anim)
			Animation.CallDeferred(AnimationPlayer.MethodName.Play, anim);
		
		SetDeferred(PropertyName.TooltipText, Tr(anim, TooltipContext));
		SetDeferred(PropertyName.MouseDefaultCursorShape, (int)shape);
	}

	#endregion

	#region Progress Updates

	public void OnProgressUpdate(FtpProgress data)
	{
		bool isInProgress = data.Progress != 100F;
		ProgressBar.SetDeferred(PropertyName.Visible, isInProgress);

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

	#region Signals

	[Signal]
    public delegate void FtpServerConnectedEventHandler();
	[Signal]
    public delegate void FtpServerDisconnectedEventHandler();

	public void AttachEventConnected(Action func) => Connect(SignalName.FtpServerConnected, Callable.From(func));
    public void AttachEventDisconnected(Action func) => Connect(SignalName.FtpServerDisconnected, Callable.From(func));
    public void DetachEventConnected(Action func) => Disconnect(SignalName.FtpServerConnected, Callable.From(func));
    public void DetachEventDisconnected(Action func) => Disconnect(SignalName.FtpServerDisconnected, Callable.From(func));

	public void EmitEventConnected() => EmitSignal(SignalName.FtpServerConnected);
	public void EmitEventDisconnected() => EmitSignal(SignalName.FtpServerDisconnected);

	#endregion
}
