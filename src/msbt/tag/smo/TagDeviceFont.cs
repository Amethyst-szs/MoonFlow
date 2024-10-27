using System;
using System.IO;
using Godot;

using CommunityToolkit.HighPerformance;

namespace Nindot.MsbtTagLibrary.Smo;

public class MsbtTagElementDeviceFont : MsbtTagElement
{
    public ushort AlwaysZero = 0x0;

    public ushort IconType
    {
        get { return TagName; }
        set
        {
            if (!Enum.IsDefined(typeof(TagNamePictureFont), value))
            {
#if !UNIT_TEST
                GD.PushWarning("Attempted to set Tag IconType to invalid type, clamped to enum maximum");
#endif

                TagName = (ushort)(TagNamePictureFont.ENUM_END - 1);
            }
            else
            {
                TagName = value;
            }
        }
    }

    public ushort DEBUG_VALUE_STORE_REMOVE_THIS = 0x0;

    public MsbtTagElementDeviceFont(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
    {
        throw new NotImplementedException();
        if (!IsValid())
            return;

        // The tag name is the icon type, so make sure to assign it to itself here to run the enum clamper
        // IconType = TagName;

        // Ensure that the first data field is 0x6, cause it is always equal to that
        AlwaysZero = BitConverter.ToUInt16(buffer, pointer);
        pointer += 2;

        if (AlwaysZero != 0x0)
        {
            GD.PushWarning("SystemFont tag has non-0 value in first data place, setting to 0");
            AlwaysZero = 0x0;
        }

        // Read the char byte and ensure that the IconType matches
        ushort typeChar = BitConverter.ToUInt16(buffer, pointer);
        DEBUG_VALUE_STORE_REMOVE_THIS = typeChar;
        pointer += 2;

        return;

        // if (typeChar != GetChar16tFromTagName())
        // {
        //     GD.PushWarning("SystemFont tag has mismatch between IconType and char data buffer, setting to default icon");
        //     IconType = (ushort)(TagNamePictureFont.ENUM_END - 1);
        // }
    }

    public ushort GetChar16tFromTagName()
    {
        if (!Enum.IsDefined(typeof(TagNamePictureFont), IconType))
            return (ushort)(TagNamePictureFont.ENUM_END - 1);

        string enumStr = Enum.GetName(typeof(TagNamePictureFont), IconType);

        return enumStr switch
        {
            { } when enumStr.StartsWith("COMMON") => (ushort)(IconType + 0x40),
            { } when enumStr.StartsWith("COIN_COLLECT") => (ushort)(IconType + 0x40),
            { } when enumStr.StartsWith("WEDDING_TREASURE") => (ushort)(IconType + 0x43),
            { } when enumStr.StartsWith("SHINE_ICON") => (ushort)(IconType + 0x4D),
            { } when enumStr.Equals("LIFE_UP_HEART") => (ushort)(IconType + 0x6),
            _ => (ushort)(TagNamePictureFont.ENUM_END - 1),
        };
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(AlwaysZero);
        value.Write(DEBUG_VALUE_STORE_REMOVE_THIS);
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

    public static readonly ushort[] IconToCharTable =
    [
        0xE0AB, // PAD_BUTTON_RIGHT
        0xE0AC, // PAD_BUTTON_DOWN
        0xE0AD, // PAD_BUTTON_UP
        0xE0AE, // PAD_BUTTON_LEFT
        0xE0E0, // A
        0xE0E1, // B
        0xE0E2, // X
        0xE0E3, // Y
        0xE0E4, // L
        0xE0E5, // R
        0xE0E6, // ZL
        0xE0E7, // ZR
        0xE0E8, // SL
        0xE0E9, // SR
        0xE0EB, // PAD_ARROW_UP
        0xE0EC, // PAD_ARROW_DOWN
        0xE0ED, // PAD_ARROW_LEFT
        0xE0EE, // PAD_ARROW_RIGHT
        0xE0EF, // PLUS
        0xE0F0, // MINUS
        0x0000, // UNKNOWN_KEY
        0xE0F4, // HOME
        0xE0F5, // SCREENSHOT_CAPTURE
        0xE100, // STICK
        0xE101, // STICKL
        0xE102, // STICKR
        0xE103, // STICK_PUSH
        0xE104, // STICKL_PUSH
        0xE105, // STICKR_PUSH
        0x0000, // UNKNOWN_KEY_5
        0x0000, // UNKNOWN_KEY_6
        0x0000, // UNKNOWN_KEY_7
        0x0000, // UNKNOWN_KEY_8
        0xE0C0, // STICKR_UP_DOWN
        0xE0C1, // STICKL_UP_DOWN
        0xE0C2, // STICKR_LEFT_RIGHT
        0xE0CB, // STICKL_LEFT_RIGHT
        0xE122, // JOY_CON
        0xE124, // JOY_CON_SINGLE_VERTICAL
        0xE127, // JOY_CON_SINGLE_HORIZONTAL
        0xE12C, // PRO_CONTROLLER
        0xE146, // MODE_SELECT_GUIDE
        0xE134, // HINT_PHOTO_GUIDE
    ];
};