using System;
using System.Collections.Generic;
using System.IO;
using System.Util;

using CommunityToolkit.HighPerformance;
using Nindot.Al.SMO;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementPictureFont : MsbtTagElement
{
    public const TagFontIndex FontIndex = TagFontIndex.Picture;
    public IconCodePictureFont IconCode = 0x00;

    public MsbtTagElementPictureFont(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementPictureFont(IconCodePictureFont icon)
        : base((ushort)TagGroup.PictureFont, 0)
    {
        IconCode = icon;
    }

    internal override void InitTag(ref int pointer, byte[] buffer, ushort dataSize)
    {
        // Ensure that the first data field is 0x6, cause it is always equal to that
        ushort font = BitConverter.ToUInt16(buffer, pointer);
        pointer += 2;

        if (font != (ushort)FontIndex)
            Console.WriteLine("PictureFont tag has non-6 value in first data place, setting to 6");

        // Read the icon code
        IconCode = (IconCodePictureFont)BitConverter.ToUInt16(buffer, pointer);
        if (!Enum.IsDefined(IconCode))
        {
            Console.WriteLine("PictureFont tag has invalid icon code, setting to default value");
            IconCode = IconCodePictureFont.IconCoin;
        }

        pointer += 2;
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(FontIndex);
        value.Write(IconCode);
        return value.ToArray();
    }

    public override ushort CalcDataSize() { return 0x4; }

    public override string GetTagNameStr()
    {
        return "PictureFont " + ((ushort)IconCode).ToString("X2");
    }

    public override string GetTextureName(int romfsVersion)
    {
        if (IconCode == IconCodePictureFont.GlyphColon_IconBalloonHintArrow)
        {
            if (romfsVersion < 120)
                return "PictureFont_" + ((ushort)IconCode).ToString("X2") + "_OLD";
        }

        return "PictureFont_" + ((ushort)IconCode).ToString("X2");
    }
};