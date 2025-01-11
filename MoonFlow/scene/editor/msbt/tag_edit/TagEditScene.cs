using Godot;

using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;

namespace MoonFlow.Scene.EditorMsbt;

[GlobalClass]
public partial class TagEditScene : Window
{
	// ====================================================== //
	// ==================== Virtual Setup =================== //
	// ====================================================== //

	public virtual bool IsRequirePageReference() { return false; }

	public virtual void SetupScene(MsbtTagElement tag) { }
	public virtual void SetupScene(MsbtTagElement tag, MsbtPage page) { }

	// ====================================================== //
	// ==================== Window Setup ==================== //
	// ====================================================== //

	public override void _Ready()
	{
		// Set window size to match root node
		if (GetChildCount() == 0)
			return;

		Size = (Vector2I)(GetChild(0) as Control).Size;

		// Move window down by the size of the border
		var pos = Position;
		pos.Y += GetThemeConstant("title_height");
		Position = pos;
	}

	public override void _UnhandledKeyInput(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel") || @event.IsActionPressed("ui_edit_tag"))
		{
			GetWindow().SetInputAsHandled();
			QueueFree();
		}
	}
}
