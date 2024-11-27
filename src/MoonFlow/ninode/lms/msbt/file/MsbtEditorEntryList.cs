using Godot;
using System;

using Nindot.LMS.Msbp;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib.Smo;
using System.Linq;
using MoonFlow.Fs.Rom;
using Nindot.Al.SMO;
using System.Collections.Generic;
using System.IO;

namespace MoonFlow.LMS.Msbt;

public partial class MsbtEditor : PanelContainer
{
    [Signal]
    public delegate void AddNewEntryValidityEventHandler(bool isValid);

    private Button CreateEntryListButton(string label, bool isSort = false)
    {
        var button = new Button
        {
            Name = label,
            Text = label,
            ToggleMode = true,
            TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis
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

    private MsbtEntryEditor CreateEntryContentEditor(int i)
    {
        var editor = new MsbtEntryEditor(Project, File.GetEntry(i))
        {
            Name = File.GetEntryLabel(i),
            Visible = false,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
        };

        EntryContent.AddChild(editor, true);
        return editor;
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

        File.AddEntry(name);

        CreateEntryListButton(name, true);
        CreateEntryContentEditor(File.GetEntryIndex(name));

        OnEntrySelected(name, true);

        // Update entry count in other components
        int entryCount = File.GetEntryCount();
        EmitSignal(SignalName.AddNewEntryValidity, false);
        EmitSignal(SignalName.EntryCountUpdated, [entryCount, entryCount]);
    }
}
