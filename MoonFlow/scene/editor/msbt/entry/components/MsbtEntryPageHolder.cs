using Godot;

using Nindot.LMS.Msbp;
using Nindot.LMS.Msbt;

namespace MoonFlow.Scene.EditorMsbt;

[ScenePath("res://scene/editor/msbt/entry/components/msbt_entry_page_holder.tscn")]
public partial class MsbtEntryPageHolder : HBoxContainer
{
	public MsbtPageEditor PageEditor { get; private set; }
	public MsbtPageEditor PageSourcePreview { get; private set; }

	[Export, ExportGroup("Internal References")]
	private VBoxContainer ContainerSidebar;

	[Signal]
	public delegate void PageOrganizeEventHandler(MsbtPageEditor page, int offset);
	[Signal]
	public delegate void PageDeleteEventHandler(MsbtPageEditor page);
	[Signal]
	public delegate void PageModifiedEventHandler(MsbtPageEditor page);
	[Signal]
	public delegate void DebugHashCopyEventHandler();

	public MsbtEntryPageHolder Init(SarcMsbpFile project, MsbtPage page, MsbtPage pageSourcePreview)
	{
		// Create page editor (and optionally source preview)
		PageEditor = CreatePageEditor();

		if (page != pageSourcePreview)
			PageSourcePreview = CreatePageEditor();

		// Force page holder to take up all available horizontal space
		SizeFlagsHorizontal = SizeFlags.ExpandFill;

		// Setup page editor
		PageEditor.Init(project, page);
		AddChild(PageEditor);
		MoveChild(PageEditor, 0);

		// Setup source preview
		if (IsInstanceValid(PageSourcePreview))
		{
			PageSourcePreview.Init(project, pageSourcePreview);
			PageSourcePreview.Editable = false;
			AddChild(PageSourcePreview);
		}

		// Connect to page editor signals
		PageEditor.Connect(TextEdit.SignalName.TextChanged, Callable.From(OnPageModified));
		PageEditor.Connect(MsbtPageEditor.SignalName.PageModified, Callable.From(OnPageModified));

		return this;
	}

	public static MsbtPageEditor CreatePageEditor()
	{
		return new MsbtPageEditor()
		{
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			SizeFlagsVertical = SizeFlags.ShrinkBegin,
			CustomMinimumSize = new Vector2(0, 72),
			ScrollFitContentHeight = true,
		};
	}

	// ====================================================== //
	// ==================== Signal Events =================== //
	// ====================================================== //

	public void OnDeleteConfirmed()
	{
		EmitSignal(SignalName.PageDelete, PageEditor);
	}

	public void OnPressOrganize(int offset)
	{
		EmitSignal(SignalName.PageOrganize, PageEditor, offset);
	}

	public void OnPageModified()
	{
		EmitSignal(SignalName.PageModified, PageEditor);
	}

	public void OnDebugHashCopyPressed()
	{
		EmitSignal(SignalName.DebugHashCopy);
	}

	// ====================================================== //
	// ====================== Utilities ===================== //
	// ====================================================== //

	public void UpdateButtonActiveness(bool isDisableSync, bool isDefaultLang)
	{
		bool isDisable = !isDisableSync && !isDefaultLang;
		PageEditor.Editable = !isDisable;

		if (!IsInstanceValid(ContainerSidebar))
			return;

		GetNode<Button>("%Button_OrganizeUp").Disabled = IsFirstPage() || isDisable;
		GetNode<Button>("%Button_OrganizeDown").Disabled = IsLastPage() || isDisable;
		GetNode<Button>("%Button_Trash").Disabled = isDisable;
	}

	public bool IsFirstPage()
	{
		foreach (var child in GetParent().GetChildren())
		{
			if (child.GetType() != typeof(MsbtEntryPageHolder))
				continue;

			if (child == this)
				return true;

			return false;
		}

		return false;
	}

	public bool IsLastPage()
	{
		var children = GetParent().GetChildren();
		children.Reverse();

		foreach (var child in children)
		{
			if (child.GetType() != typeof(MsbtEntryPageHolder))
				continue;

			if (child == this)
				return true;

			return false;
		}

		return false;
	}
}
