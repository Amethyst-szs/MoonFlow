using System;
using System.Collections.Generic;
using System.IO;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementPictureFont : MsbtTagElement
{
    public const TagFontIndex FontIndex = TagFontIndex.Picture;

    private IconCodePictureFont _iconCodeInternal = 0x00;
    public IconCodePictureFont IconCode
    {
        get => _iconCodeInternal;
        set
        {
            // Update internal storage
            _iconCodeInternal = value;

            // Update tag name to match the selected icon code
            SetTagNameUnsafe(CalcTagName(IconCode));
        }
    }

    public MsbtTagElementPictureFont(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementPictureFont(IconCodePictureFont icon)
        : base((ushort)TagGroup.PictureFont, CalcTagName(icon))
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
        using MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(FontIndex);
        value.Write(IconCode);
        return value.ToArray();
    }

    public override ushort CalcDataSize() { return 0x4; }

    #region Utility

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

    private static ushort CalcTagName(IconCodePictureFont code)
    {
        return code switch
        {
            IconCodePictureFont.IconCoin => 0x00,
            IconCodePictureFont.IconEarth => 0x01,
            IconCodePictureFont.IconCheckpoint => 0x02,
            IconCodePictureFont.IconKoopa => 0x03,
            IconCodePictureFont.IconPeach => 0x04,
            IconCodePictureFont.IconTiara => 0x05,
            IconCodePictureFont.IconBroodalCapThrower => 0x06,
            IconCodePictureFont.IconBoodalFireBlower => 0x07,
            IconCodePictureFont.IconBoodalStacker => 0x08,
            IconCodePictureFont.IconBoodalBombTail => 0x09,
            IconCodePictureFont.IconHomeShip => 0x0A,
            IconCodePictureFont.IconFrog => 0x0B,
            IconCodePictureFont.IconMario => 0x0C,
            IconCodePictureFont.IconCap => 0x0D,
            IconCodePictureFont.IconMarioCapOff => 0x0E,
            IconCodePictureFont.IconPauline => 0x0F,
            IconCodePictureFont.CoinCollectCap => 0x10,
            IconCodePictureFont.CoinCollectWaterfall => 0x11,
            IconCodePictureFont.CoinCollectSand => 0x12,
            IconCodePictureFont.CoinCollectForest => 0x13,
            IconCodePictureFont.CoinCollectLake => 0x14,
            IconCodePictureFont.CoinCollectClash => 0x15,
            IconCodePictureFont.CoinCollectCity => 0x16,
            IconCodePictureFont.CoinCollectSea => 0x17,
            IconCodePictureFont.CoinCollectSnow => 0x18,
            IconCodePictureFont.CoinCollectLava => 0x19,
            IconCodePictureFont.CoinCollectSky => 0x1A,
            IconCodePictureFont.CoinCollectMoon => 0x1B,
            IconCodePictureFont.CoinCollectPeach => 0x1C,
            IconCodePictureFont.TreasureRing => 0x1D,
            IconCodePictureFont.TreasureFlower => 0x1E,
            IconCodePictureFont.TreasureDress => 0x1F,
            IconCodePictureFont.TreasureWater => 0x20,
            IconCodePictureFont.TreasureCake => 0x21,
            IconCodePictureFont.TreasureStew => 0x22,
            IconCodePictureFont.ShineCommon => 0x23,
            IconCodePictureFont.ShineCity => 0x24,
            IconCodePictureFont.ShineForest => 0x25,
            IconCodePictureFont.ShineSky => 0x26,
            IconCodePictureFont.ShineSnow => 0x27,
            IconCodePictureFont.ShineSand => 0x28,
            IconCodePictureFont.ShineLava => 0x29,
            IconCodePictureFont.ShineLake => 0x2A,
            IconCodePictureFont.ShineSea => 0x2B,
            IconCodePictureFont.ShineMoon => 0x2C,
            IconCodePictureFont.ShineRainbow => 0x2D,
            IconCodePictureFont.ShineNull => 0x2E,
            IconCodePictureFont.IconLifeMaxUpItem => 0x31,
            IconCodePictureFont.IconCapManHero => 0x32,
            IconCodePictureFont.IconLuigi => 0x33,
            _ => 0x00
        };
    }

    #endregion
};