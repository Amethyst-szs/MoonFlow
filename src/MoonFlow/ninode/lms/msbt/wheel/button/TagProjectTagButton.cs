using Godot;
using MoonFlow.Project;
using Nindot.LMS.Msbt.TagLib.Smo;

using System;
using System.Collections.Generic;
using System.Linq;

namespace MoonFlow.LMS.Msbt;

[GlobalClass, Tool, Icon("res://asset/nindot/lms/icon/ProjectTag.png")]
public partial class TagProjectTagButton : TagInsertButtonBase
{
    private TagNameProjectIcon _iconCode = TagNameProjectIcon.ShineIconCurrentWorld;

    [Export]
    public TagNameProjectIcon IconCode
    {
        get { return _iconCode; }
        set
        {
            _iconCode = value;
            TooltipText = "PROJECT_TAG_TOOLTIP_" + Enum.GetName(value);
        }
    }

    private int InputDevice = 0;

    private List<Texture2D> Textures = [];
    private int TextureIndex = 0;
    private double TextureTimer = 1.0F;

    public override void _Ready()
    {
        base._Ready();
        UpdateTexturesWithResolver();
    }

    public override void _Process(double delta)
    {
        if (Textures.Count <= 1)
            return;
        
        TextureTimer -= delta;
        if (TextureTimer > 0.0F)
            return;
        
        TextureTimer = 1.0F;
        TextureIndex = (TextureIndex + 1) % Textures.Count;

        Icon = Textures[TextureIndex];
    }

    public override void _Pressed()
    {
        var tag = new MsbtTagElementProjectTag(IconCode);
        EmitSignal(SignalName.SelectedTag, [new TagWheelTagResult(tag)]);
    }

    // ====================================================== //
    // ============== Resolver Lookup Utilities ============= //
    // ====================================================== //

    public void SetInputDevice(int id)
    {
        InputDevice = id;
        UpdateTexturesWithResolver();
    }

    private void UpdateTexturesWithResolver()
    {
        if (Engine.IsEditorHint())
            return;

        // Clear current texture table
        Textures.Clear();

        // Instantiate tag to find lookup key
        var resolver = ProjectManager.GetMSBTArchives().ProjectIconResolver;

        var tag = new MsbtTagElementProjectTag(IconCode);
        var lookupKey = tag.GetTextureName(InputDevice);

        if (lookupKey == null)
            return;

        // Get list of textures in resolver
        var texNames = resolver.ResolveTextureNames(lookupKey);

        foreach (var texName in texNames)
            Textures.Add(GD.Load<Texture2D>(TexturePath + texName + ".png"));

        // Assign first texture to button icon
        if (Textures.Count > 0)
        {
            Show();
            Icon = Textures[0];
        }
        else
        {
            Hide();
            Icon = null;
        }

        // Get text from resolver and assign to button text
        var txt = resolver.ResolveText(lookupKey);

        if (txt != null && txt != "-" && txt != " + ")
            Text = txt;
        else
            Text = null;
    }
}