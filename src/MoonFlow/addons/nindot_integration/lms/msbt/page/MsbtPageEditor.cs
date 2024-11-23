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
        // Setup default properties
        AutowrapMode = TextServer.AutowrapMode.WordSmart;
        ContextMenuEnabled = true;

        // TODO: DEBUG BULLSHITTERY REMOVE LATER
        Setup(null, [new MsbtTextElement("abcde"), new MsbtTagElementEuiSpeed(), new MsbtTextElement("fghijklm")]);
        foreach (var item in Page)
        {
            if (item.IsText())
                Text += item.GetText();
            else
                Text += ' ';
        }
    }

    public MsbtPageEditor Setup(MsbpFile project, MsbtPage page)
    {
        Project = project;
        Page = page;
        return this;
    }

    public override void _GuiInput(InputEvent @event)
    {
        return;
    }

    public override void _Draw()
    {
        // DrawTextureRect(TestTex, new Rect2(Text.Length, 0.0F, 100.0F, 100.0F), false, null, true);
    }
}
