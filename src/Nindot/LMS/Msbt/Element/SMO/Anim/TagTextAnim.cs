using System;
using System.IO;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementTextAnim : MsbtTagElement
{
    public TagNameTextAnim Anim
    {
        get { return (TagNameTextAnim)TagName; }
        set { TagName = (ushort)value; }
    }

    public MsbtTagElementTextAnim(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementTextAnim(TagNameTextAnim anim)
        : base((ushort)TagGroup.TextAnim, (ushort)anim)
    {
        Anim = anim;
    }

    internal override void InitTag(ref int pointer, byte[] buffer, ushort dataSize)
    {
        Anim = (TagNameTextAnim)TagName;
    }

    public override ushort CalcDataSize() { return 0x0; }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameTextAnim), TagName))
            return Enum.GetName(typeof(TagNameTextAnim), TagName);

        return "Unknown";
    }

    public override string GetTextureName()
    {
        return Anim switch
        {
            TagNameTextAnim.Tremble => "TextAnim_Tremble",
            TagNameTextAnim.Shake => "TextAnim_Shake",
            TagNameTextAnim.Wave => "TextAnim_Wave",
            TagNameTextAnim.Scream => "TextAnim_Scream",
            TagNameTextAnim.Beat => "TextAnim_Beat",
            _ => null,
        };
    }
};