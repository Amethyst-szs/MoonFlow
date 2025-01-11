using Godot;

using Nindot;

namespace MoonFlow.Scene.Home;

public abstract partial class TabFileAccessorBase : Node
{
	protected bool IsCut = false;

	[Export, ExportGroup("Internal References")]
	protected Button CopyButton;
	[Export]
	protected Button PasteButton;
	[Export]
	protected Button CutButton;
	[Export]
	protected Button DeleteButton;

	protected virtual void OnCopyFile()
	{
		IsCut = false;
	}
	protected virtual void OnCutFile()
	{
		IsCut = true;
	}

	protected bool IsFileNameValid(string name, SarcFile sourceArc)
	{
		if (sourceArc == null || sourceArc.Content.ContainsKey(name))
		{
			ThrowDuplicateNameDialog();
			return false;
		}

		return true;
	}

	protected void ThrowDuplicateNameDialog()
	{
		GetNode<AcceptDialog>("Dialog_CreateError_DuplicateName").Popup();
	}
}
