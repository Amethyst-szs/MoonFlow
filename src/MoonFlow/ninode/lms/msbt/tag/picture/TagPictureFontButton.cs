using Godot;
using Nindot.LMS.Msbt.TagLib.Smo;

namespace MoonFlow.LMS.Msbt;

public partial class TagPictureFontButton : Button
{
    private readonly MsbtTagElementPictureFont Tag;

    private const string TexturePath = "res://asset/nindot/lms/icon/";

    public TagPictureFontButton(TagNamePictureFont type)
    {
        // Convert enum value into tag instance
        Tag = new(type);

        // Setup button
        Icon = GD.Load<Texture2D>(TexturePath + Tag.GetTextureName() + ".png");
    }
}