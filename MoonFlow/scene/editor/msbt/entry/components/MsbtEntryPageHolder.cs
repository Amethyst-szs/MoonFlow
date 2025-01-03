using Godot;
using System;

using Nindot.LMS.Msbp;
using Nindot.LMS.Msbt;
using System.Linq;

namespace MoonFlow.Scene.EditorMsbt;

[ScenePath("res://scene/editor/msbt/entry/components/msbt_entry_page_holder.tscn")]
public partial class MsbtEntryPageHolder : HBoxContainer
{
	public MsbtPageEditor PageEditor = new()
	{
		SizeFlagsHorizontal = SizeFlags.ExpandFill,
		SizeFlagsVertical = SizeFlags.ShrinkBegin,
		CustomMinimumSize = new Vector2(0, 72),
		ScrollFitContentHeight = true,
	};

	[Signal]
	public delegate void PageOrganizeEventHandler(MsbtPageEditor page, int offset);
	[Signal]
	public delegate void PageDeleteEventHandler(MsbtPageEditor page);
	[Signal]
	public delegate void PageModifiedEventHandler(MsbtPageEditor page);
	[Signal]
	public delegate void DebugHashCopyEventHandler();

	public MsbtEntryPageHolder Init(SarcMsbpFile project, MsbtPage page)
	{
		// Force page holder to take up all available horizontal space
		SizeFlagsHorizontal = SizeFlags.ExpandFill;

		// Setup page editor
		PageEditor.Init(project, page);
		AddChild(PageEditor);
		MoveChild(PageEditor, 0);

		// Connect to page editor signals
		PageEditor.Connect(TextEdit.SignalName.TextChanged, Callable.From(OnPageModified));
		PageEditor.Connect(MsbtPageEditor.SignalName.PageModified, Callable.From(OnPageModified));

		return this;
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

	public void HandleSyncToggled(bool isDisableSync)
	{
		PageEditor.Editable = isDisableSync;
		SetupButtonActiveness(!isDisableSync);
	}

	// ====================================================== //
	// ====================== Utilities ===================== //
	// ====================================================== //

	public void SetupButtonActiveness(bool isForceOff = false)
	{
		GetNode<Button>("%Button_OrganizeUp").Disabled = IsFirstPage() || isForceOff;
		GetNode<Button>("%Button_OrganizeDown").Disabled = IsLastPage() || isForceOff;
		GetNode<Button>("%Button_Trash").Disabled = isForceOff;
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
