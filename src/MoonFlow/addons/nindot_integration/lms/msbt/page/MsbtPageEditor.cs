using Godot;
using Godot.Collections;
using System;

using Nindot.LMS.Msbp;
using Nindot.LMS.Msbt;
using Nindot.LMS.Msbt.TagLib;
using Nindot.LMS.Msbt.TagLib.Smo;
using System.Text;

namespace MoonFlow.LMS.Msbt;

[GlobalClass]
public partial class MsbtPageEditor : TextEdit
{
    MsbpFile Project = null;
    MsbtPage Page = null;

    CompressedTexture2D TestTex = (CompressedTexture2D)GD.Load("res://iconS.png");

    public override void _Ready()
    {


        // TODO: DEBUG BULLSHITTERY REMOVE LATER
        Init(null, [new MsbtTextElement("abcde"), new MsbtTagElementEuiSpeed(), new MsbtTextElement("fghijklm")]);

    }

    public MsbtPageEditor Init(MsbpFile project, MsbtPage page)
    {
        Project = project;
        Page = page;

        PrepareTextEdit();
        return this;
    }

    private void PrepareTextEdit()
    {
        // Setup default properties
        AutowrapMode = TextServer.AutowrapMode.WordSmart;
        DragAndDropSelectionEnabled = false;
        MiddleMousePasteEnabled = false;
        WrapMode = LineWrappingMode.Boundary;

        // Setup text string to match page elements
        BeginComplexOperation();
        Text = "";
        foreach (var item in Page)
        {
            if (item.IsText())
                Text += item.GetText();
            else
                Text += '\u2E3A';
        }
        EndComplexOperation();
    }

    public override void _GuiInput(InputEvent @event)
    {
        return;
    }
}
