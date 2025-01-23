using Godot;

namespace MoonFlow.Scene.Main;

public partial class Header : PanelContainer
{
	[Export]
	public Button ButtonAppMinimize;
	[Export]
	public Button ButtonAppClose;
	
	[Export]
	public FtpStatusIndicator FtpStatusIndicator;

	[Signal]
	public delegate void AppFocusedEventHandler();

	[Signal]
	public delegate void ButtonSaveEventHandler(bool isRequireFocus);
	[Signal]
	public delegate void ButtonSaveAsEventHandler();
}
