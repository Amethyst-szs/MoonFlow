using Godot;
using System;

using Nindot.LMS.Msbp;
using Nindot.LMS.Msbt;

namespace MoonFlow.LMS.Msbt;

public partial class MsbtEntryPageHolder : HBoxContainer
{
	public MsbtPageEditor PageEditor = new()
	{
		SizeFlagsHorizontal = SizeFlags.ExpandFill,
		SizeFlagsVertical = SizeFlags.ShrinkBegin,
		CustomMinimumSize = new Vector2(0, 240),
	};

	public ConfirmationDialog DeleteConfirmation = new()
	{
		Title = "Delete Page",
		DialogText = "Are you sure you want to delete this page?",
		OkButtonText = "Yes",
		CancelButtonText = "No",
	};

	[Signal]
	public delegate void PageDeleteRequestEventHandler(MsbtPageEditor page);

	public MsbtEntryPageHolder(MsbpFile project, MsbtPage page)
	{
		// Force page holder to take up all available horizontal space
		SizeFlagsHorizontal = SizeFlags.ExpandFill;

		// Setup page editor
		PageEditor.Init(project, page);
		AddChild(PageEditor);

        // Setup trash button
        var trash = new Button
        {
            Text = "Delete",
			SizeFlagsVertical = SizeFlags.ShrinkCenter,
        };

		trash.Pressed += OnDeleteButtonPressed;
		AddChild(trash);

		// Setup trash confirmation
        AddChild(DeleteConfirmation);
		DeleteConfirmation.Confirmed += OnDeleteConfirmed;
	}

	public void OnDeleteButtonPressed()
	{
		DeleteConfirmation.PopupCentered();
	}

	public void OnDeleteConfirmed()
	{

	}
}
