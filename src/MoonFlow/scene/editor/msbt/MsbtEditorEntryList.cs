using System;
using System.Linq;
using Godot;

using MoonFlow.Project;

namespace MoonFlow.Scene.EditorMsbt;

public partial class MsbtEditor : PanelContainer
{
    [Signal]
    public delegate void AddNewEntryValidityEventHandler(bool isValid);

    public bool IsModified { get; private set; } = false;
    private static readonly Texture2D ModifiedTexture = GD.Load<Texture2D>("res://asset/material/file/modify.svg");

    private Button CreateEntryListButton(string label, bool isSort = false)
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

        button.ButtonDown += () => OnEntrySelected(label);
        button.Pressed += () => OnEntrySelected(label);
        button.MouseEntered += () => OnEntryHovered(label);
        EntryList.AddChild(button, true);

        if (!isSort)
            return button;

        int moveIndex = 0;
        for (int i = 0; i < EntryList.GetChildCount(); i++)
        {
            int result = string.Compare(label, EntryList.GetChild(i).Name);
            if (result > 0)
            {
                moveIndex += 1;
                continue;
            }

            break;
        }

        EntryList.MoveChild(button, moveIndex);
        return button;
    }

    // ====================================================== //
    // ==================== Signal Events =================== //
    // ====================================================== //

    private void OnAddEntryNameChanged(string name)
    {
        var isValid = IsAddEntryNameValid(name);
        EmitSignal(SignalName.AddNewEntryValidity, isValid);
    }

    private bool IsAddEntryNameValid(string name)
    {
        if (name == string.Empty)
            return false;

        if (name.Contains(' '))
            return false;

        if (File.GetEntryLabels().Contains(name))
            return false;

        byte[] bytes = name.ToCharArray().Select(c => (byte)c).ToArray();
        string decodedString = System.Text.Encoding.UTF8.GetString(bytes);

        return name.Equals(decodedString);
    }

    private void OnAddEntryNameSubmitted(string name)
    {
        if (!IsAddEntryNameValid(name))
            return;

        foreach (var file in FileList.Values)
            file.AddEntry(name);

        CreateEntryListButton(name, true);
        CreateEntryContentEditor(File.GetEntryIndex(name));

        OnEntrySelected(name, true);

        // Update entry count in other components
        int entryCount = File.GetEntryCount();
        EmitSignal(SignalName.AddNewEntryValidity, false);
        EmitSignal(SignalName.EntryCountUpdated, [entryCount, entryCount]);
    }

    private void OnDeleteEntryTrash()
    {
        if (!IsInstanceValid(EntryListSelection) || !IsInstanceValid(EntryContentSelection))
            return;

        string entry = EntryListSelection.Name;
        string prevEntry = File.GetEntryLabel(File.GetEntryIndex(entry) - 1);

        foreach (var file in FileList.Values)
            file.RemoveEntry(entry);

        EntryListSelection.QueueFree();
        EntryContentSelection.QueueFree();

        if (prevEntry != null && prevEntry != string.Empty)
            OnEntrySelected(prevEntry);

        // Update entry count in other components
        int entryCount = File.GetEntryCount();
        EmitSignal(SignalName.AddNewEntryValidity, false);
        EmitSignal(SignalName.EntryCountUpdated, [entryCount, entryCount]);
    }

    private void OnEntryModified(MsbtEntryEditor entryEditor)
    {
        // Set flag
        IsModified = true;

        // Update entry button to have modified icon texture
        var entry = entryEditor.Entry;
        var entryButton = EntryList.GetNode<Button>(entry.Name);

        if (entryButton.Icon != ModifiedTexture)
            entryButton.Icon = ModifiedTexture;

        // Append asterisk to file name
        if (!FileTitleName.Text.EndsWith('*'))
            FileTitleName.Text += '*';
    }

    public void ForceResetModifiedFlag() { IsModified = false; }
}
