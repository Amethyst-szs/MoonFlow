using System;
using System.IO;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementPictureFont : MsbtTagElement
{
    public const TagFontIndex FontIndex = TagFontIndex.Picture;

    public TagNamePictureFont Icon
    {
        get { return (TagNamePictureFont)TagName; }
        set { TagName = (ushort)value; }
    }

    public MsbtTagElementPictureFont(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementPictureFont(TagNamePictureFont icon)
        : base((ushort)TagGroup.PictureFont, (ushort)icon)
    {
        Icon = icon;
    }

    internal override void InitTag(ref int pointer, byte[] buffer, ushort dataSize)
    {
        // Ensure that the first data field is 0x6, cause it is always equal to that
        ushort font = BitConverter.ToUInt16(buffer, pointer);
        pointer += 2;

        if (font != (ushort)FontIndex)
            Console.WriteLine("PictureFont tag has non-6 value in first data place, setting to 6");

        // Read the char byte and ensure that the IconType matches
        ushort typeChar = BitConverter.ToUInt16(buffer, pointer);
        ushort calcTypeChar = GetChar16tFromTagName();
        pointer += 2;

        if (typeChar != calcTypeChar || calcTypeChar == 0x0000)
        {
            Console.Error.WriteLine("PictureFont tag has mismatch between IconType and char data buffer, setting to default icon");
            Icon = TagNamePictureFont.COMMON_COIN;
        }
    }

    public ushort GetChar16tFromTagName()
    {
        if (!Enum.IsDefined(typeof(TagNamePictureFont), Icon))
            return 0x0;

        string enumStr = Enum.GetName(typeof(TagNamePictureFont), Icon);

        return enumStr switch
        {
            { } when enumStr.StartsWith("COMMON") => (ushort)(Icon + 0x40),
            { } when enumStr.StartsWith("COIN_COLLECT") => (ushort)(Icon + 0x40),
            { } when enumStr.StartsWith("WEDDING_TREASURE") => (ushort)(Icon + 0x43),
            { } when enumStr.StartsWith("SHINE_ICON") => (ushort)(Icon + 0x4D),
            { } when enumStr.StartsWith("ICON") => (ushort)(Icon + 0x6),
            _ => 0x0,
        };
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(FontIndex);
        value.Write(GetChar16tFromTagName());
        return value.ToArray();
    }

    public override ushort CalcDataSize() { return 0x4; }

    public override string GetTagNameStr()
    {
        if (Icon >= TagNamePictureFont.ICON_BALLOON_GAME_SMALL_STAR)
            return "Unknown Icon";
        
        return IconNameTable[(ushort)Icon];
    }

    public static readonly string[] IconNameTable =
    [
        "Coin",
        "Globe",
        "Checkpoint Flag",
        "Bowser",
        "Peach",
        "Tiara",
        "Rango",
        "Spewert",
        "Topper",
        "Hariet",
        "Odyssey Ship",
        "Frog",
        "Mario",
        "Cappy",
        "Mario (No Cappy)",
        "Pauline",

        "Purple Coin (Cap Kingdom)",
        "Purple Coin (Cascade Kingdom)",
        "Purple Coin (Sand Kingdom)",
        "Purple Coin (Wooded Kingdom)",
        "Purple Coin (Lake Kingdom)",
        "Purple Coin (Lost Kingdom)",
        "Purple Coin (Metro Kingdom)",
        "Purple Coin (Seaside Kingdom)",
        "Purple Coin (Snow Kingdom)",
        "Purple Coin (Luncheon Kingdom)",
        "Purple Coin (Bowser's Kingdom)",
        "Purple Coin (Moon Kingdom)",
        "Purple Coin (Mushroom Kingdom)",

        "Wedding Treasure (Binding Band)",
        "Wedding Treasure (Soirée Bouquet)",
        "Wedding Treasure (Lochlady Dress)",
        "Wedding Treasure (Sparkle Water)",
        "Wedding Treasure (Frost-Frosted Cake)",
        "Wedding Treasure (Stupendous Stew)",

        "Power Moon (Generic)",
        "Power Moon (Metro Kingdom)",
        "Power Moon (Wooded Kingdom)",
        "Power Moon (Bowser's Kingdom)",
        "Power Moon (Snow Kingdom)",
        "Power Moon (Sand Kingdom)",
        "Power Moon (Luncheon Kingdom)",
        "Power Moon (Lake Kingdom)",
        "Power Moon (Seaside Kingdom)",
        "Power Moon (Moon Kingdom)",
        "Power Moon",
        "Empty Power Moon",

        "Star Icon",
        "Star Icon (Empty)",
        "Life-Up Heart",
        "Cappy",
        "Luigi",

        // v1.2.0+
        "Balloon World Arrow",
        "Balloon World Star",
    ];

    public override string GetTextureName() {
        return string.Format("PictureFont_{0}", GetChar16tFromTagName().ToString("X2"));
    }
};