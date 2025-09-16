using Godot;
using MoonFlow.Addons;

namespace MoonFlow.Scene.Main;

public partial class Header : PanelContainer
{
	[Export]
	public Button ButtonAppMinimize;
	[Export]
	public Button ButtonAppClose;

	[Export]
	public FtpStatusIndicator FtpStatusIndicator;
	[Export]
	public Label LabelVersion;

	[Export]
	public MenuBar ActionbarInjectable;

	[Signal]
	public delegate void AppFocusedEventHandler();

	[Signal]
	public delegate void ButtonSaveEventHandler(bool isRequireFocus);
	[Signal]
	public delegate void ButtonSaveAsEventHandler();

	public override void _Ready()
	{
		LabelVersion.Text = GitInfo.GitVersionName();
		LabelVersion.Visible = OS.IsDebugBuild();
    }
}
