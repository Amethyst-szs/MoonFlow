using System;
using System.IO;
using Godot;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementPictureFont : MsbtTagElement
{
    public const TagFontIndex FontIndex = TagFontIndex.PICTURE_FONT;

    public TagNamePictureFont IconType
    {
        get { return (TagNamePictureFont)TagName; }
        set
        {
            if (!Enum.IsDefined(typeof(TagNamePictureFont), value))
            {
                GD.PushWarning("Attempted to set PictureFont Tag IconType to invalid type, clamped to enum maximum");
                TagName = (ushort)(TagNamePictureFont.ENUM_END - 1);
            }
            else
            {
                TagName = (ushort)value;
            }
        }
    }

    public MsbtTagElementPictureFont(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
    {
        if (!IsValid())
            return;

        // The tag name is the icon type, so make sure to assign it to itself here to run the enum clamper
        IconType = (TagNamePictureFont)TagName;

        // Ensure that the first data field is 0x6, cause it is always equal to that
        ushort font = BitConverter.ToUInt16(buffer, pointer);
        pointer += 2;

        if (font != (ushort)FontIndex)
            GD.PushWarning("PictureFont tag has non-6 value in first data place, setting to 6");

        // Read the char byte and ensure that the IconType matches
        ushort typeChar = BitConverter.ToUInt16(buffer, pointer);
        ushort calcTypeChar = GetChar16tFromTagName();
        pointer += 2;

        if (typeChar != calcTypeChar || calcTypeChar == 0x0000)
        {
            GD.PushWarning("PictureFont tag has mismatch between IconType and char data buffer, setting to default icon");
            IconType = TagNamePictureFont.ENUM_END;
        }
    }

    public ushort GetChar16tFromTagName()
    {
        if (!Enum.IsDefined(typeof(TagNamePictureFont), IconType))
            return 0x40;

        string enumStr = Enum.GetName(typeof(TagNamePictureFont), IconType);

        return enumStr switch
        {
            { } when enumStr.StartsWith("COMMON") => (ushort)(IconType + 0x40),
            { } when enumStr.StartsWith("COIN_COLLECT") => (ushort)(IconType + 0x40),
            { } when enumStr.StartsWith("WEDDING_TREASURE") => (ushort)(IconType + 0x43),
            { } when enumStr.StartsWith("SHINE_ICON") => (ushort)(IconType + 0x4D),
            { } when enumStr.StartsWith("ICON") => (ushort)(IconType + 0x6),
            _ => (ushort)(TagNamePictureFont.ENUM_END - 1),
        };
    }

    public string GetIconName()
    {
        if (IconType >= TagNamePictureFont.ENUM_END)
            return "Unknown Icon";

        return IconNameTable[(ushort)IconType];
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(FontIndex);
        value.Write(GetChar16tFromTagName());
        return value.ToArray();
    }

    public override ushort GetDataSizeBase()
    {
        return 0x4;
    }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNamePictureFont), TagName))
            return Enum.GetName(typeof(TagNamePictureFont), TagName);

        return "Unknown";
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
    ];
};