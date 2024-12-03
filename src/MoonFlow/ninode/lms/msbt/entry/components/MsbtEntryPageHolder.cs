using Godot;
using System;

using Nindot.LMS.Msbp;
using Nindot.LMS.Msbt;
using System.Linq;

namespace MoonFlow.LMS.Msbt;

[ScenePath("res://ninode/lms/msbt/entry/components/msbt_entry_page_holder.tscn")]
public partial class MsbtEntryPageHolder : HBoxContainer
{
	public MsbtPageEditor PageEditor = new()
	{
		SizeFlagsHorizontal = SizeFlags.ExpandFill,
		SizeFlagsVertical = SizeFlags.ShrinkBegin,
		CustomMinimumSize = new Vector2(0, 240),
	};

	[Signal]
	public delegate void PageOrganizeEventHandler(MsbtPageEditor page, int offset);
	[Signal]
	public delegate void PageDeleteEventHandler(MsbtPageEditor page);

	public MsbtEntryPageHolder Init(SarcMsbpFile project, MsbtPage page)
	{
		// Force page holder to take up all available horizontal space
		SizeFlagsHorizontal = SizeFlags.ExpandFill;

		// Setup page editor
		PageEditor.Init(project, page);
		AddChild(PageEditor);
		MoveChild(PageEditor, 0);
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

	// ====================================================== //
	// ====================== Utilities ===================== //
	// ====================================================== //

	public void SetupButtonActiveness()
	{
		GetNode<Button>("%Button_OrganizeUp").Disabled = IsFirstPage();
		GetNode<Button>("%Button_OrganizeDown").Disabled = IsLastPage();
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
