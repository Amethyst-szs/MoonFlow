using Godot;
using System;

namespace MoonFlow.Scene.EditorMsbt;

public partial class MsbtEntryList(MsbtEditor parent) : VBoxContainer
{
    #region Properties & Init

    private MsbtEditor Parent = parent;

    public Button EntryListSelection { get; private set; } = null;
    public Label EntryCount { get; private set; } = null;

    private static readonly Texture2D ModifiedTexture = GD.Load<Texture2D>("res://asset/material/file/modify.svg");

    [Signal]
    public delegate void EntrySelectedEventHandler(string label);

    public override void _Ready()
    {
        if (!IsInstanceValid(Parent))
            throw new NullReferenceException(nameof(Parent));
        
        // Get references
        EntryCount = GetNode<PanelContainer>("../../Controls").Get("label_entry_count").As<Label>();

        // Connect to signals from parent
        Parent.Connect(MsbtEditor.SignalName.ContentModified,
            Callable.From(new Action<string>(OnContentModified)));
    }

    #endregion

    #region Button Builder

    public Button CreateEntryListButton(string label, bool isSort = false)
    {
        var button = new Button
        {
            Name = label,
            Text = label,
            ToggleMode = true,
            TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis,

            ExpandIcon = true,
            IconAlignment = HorizontalAlignment.Right
        };

        button.Connect(Button.SignalName.Pressed, Callable.From(() => OnEntrySelected(label)));
        button.Connect(Button.SignalName.ButtonDown, Callable.From(() => OnEntrySelected(label)));
        button.Connect(Button.SignalName.MouseEntered, Callable.From(() => OnEntryHovered(label)));

        AddChild(button);

        if (!isSort)
            return button;

        int moveIndex = 0;
        for (int i = 0; i < GetChildCount(); i++)
        {
            int result = string.Compare(label, GetChild(i).Name);
            if (result > 0)
            {
                moveIndex += 1;
                continue;
            }

            break;
        }

        MoveChild(button, moveIndex);
        return button;
    }

    #endregion

    // ====================================================== //
    // ==================== Signal Events =================== //
    // ====================================================== //

    #region Signals (Buttons)

    public void OnEntryHovered(string label)
    {
        if (Input.IsMouseButtonPressed(MouseButton.Left))
            OnEntrySelected(label);
    }

    public void OnEntrySelected(string label, bool isGrabFocus = true)
    {
        // Close old selection
        if (IsInstanceValid(EntryListSelection))
            EntryListSelection.ButtonPressed = false;

        if (IsInstanceValid(Parent.EntryContentSelection))
            Parent.EntryContentSelection.Hide();

        // Ensure string is not empty
        if (label == string.Empty)
            return;

        // Open entry
        var button = GetNode<Button>(label);
        EntryListSelection = button;

        if (button == null)
            return;

        button.ButtonPressed = true;
        if (isGrabFocus)
            button.GrabFocus();

        // Emit signal for main editor to handle editor content
        EmitSignal(SignalName.EntrySelected, label);
    }

    #endregion

    #region Signals (Content)

    private void OnContentModified(string label)
    {
        var entryButton = GetNode<Button>(label);

        if (entryButton.Icon != ModifiedTexture)
            entryButton.Icon = ModifiedTexture;
    }

    #endregion
}
