namespace Nindot.LMS.Msbt.TagLib.Smo;

public enum TagGroup : ushort
{
    System = 0x0,
    Eui = 0x1,
    Number = 0x2,
    TextAnim = 0x3,
    PlaySe = 0x4,
    String = 0x5,
    ProjectTag = 0x6,
    Time = 0x7,
    PictureFont = 0x8,
    DeviceFont = 0x9,
    TextAlign = 0xA,
    Grammar = 0xC9,
}

public enum TagNameSystem : ushort
{
    Ruby = 0, // Used for rendering Japanese Furigana
    Font = 1,
    FontSize = 2,
    Color = 3,
    PageBreak = 4,
    /* REFERENCE = 5 (This tag is not implemented by the game, only noted in MSBP) */
}

public enum TagNameEui : ushort
{
    Wait = 0,
    Speed = 1,
    /*
    "Flush" (ID 2) is implemented in code but unused, not functional?
    "AutoNext" (ID 3) is referenced in MSBP but with no implementation
    "Choice2" (ID 4) is referenced in MSBP but with no implementation
    "Choice3" (ID 5) is referenced in MSBP but with no implementation
    "Choice4" (ID 6) is referenced in MSBP but with no implementation
    */
}

public enum TagNameNumber : ushort
{
    Score = 0,
    // The MSBP defines a tag called Fig02, and the game's code checks for "FigLeft", "Fig02",
    // "Fig03", and more. As far as I can tell though, this is completely unused? Will get
    // reguistered as an MsbtTagElementUnknown for now.
    Fig02 = 1,
    CoinNum = 2,
    Date = 3,
    RaceTime = 4,
    DateDetail = 5,
    DateEU = 6, // v1.2.0+
    DateDetailEU = 7, // v1.2.0+
}

public enum TagNameTextAnim : ushort
{
    Tremble = 0,
    Shake = 1,
    Wave = 2,
    Scream = 3,
    Beat = 4,
};

public enum TagNameTime : ushort
{
    Year = 0,
    Month = 1,
    Day = 2,
    Hour = 3,
    Minute = 4,
    Second = 5,
    Centisecond = 6
}

public enum TagNamePictureFont : ushort
{
    COMMON_COIN = 0x00,
    COMMON_GLOBE = 0x01,
    COMMON_CHECKPOINT_FLAG = 0x02,
    COMMON_KOOPA = 0x03,
    COMMON_PEACH = 0x04,
    COMMON_TIARA = 0x05,
    COMMON_RANGO = 0x06,
    COMMON_SPEWERT = 0x07,
    COMMON_TOPPER = 0x08,
    COMMON_HARIET = 0x09,
    COMMON_HOME_SHIP = 0x0A,
    COMMON_FROG = 0x0B,
    COMMON_MARIO = 0x0C,
    COMMON_CAPPY = 0x0D,
    COMMON_MARIO_NO_CAP = 0x0E,
    COMMON_PAULINE = 0x0F,

    COIN_COLLECT_CAP = 0x10,
    COIN_COLLECT_WATERFALL = 0x11,
    COIN_COLLECT_SAND = 0x12,
    COIN_COLLECT_FOREST = 0x13,
    COIN_COLLECT_LAKE = 0x14,
    COIN_COLLECT_CLASH = 0x15,
    COIN_COLLECT_CITY = 0x16,
    COIN_COLLECT_SEA = 0x17,
    COIN_COLLECT_SNOW = 0x18,
    COIN_COLLECT_LAVA = 0x19,
    COIN_COLLECT_SKY = 0x1A,
    COIN_COLLECT_MOON = 0x1B,
    COIN_COLLECT_PEACH = 0x1C,

    WEDDING_TREASURE_RING = 0x1D,
    WEDDING_TREASURE_FLOWER = 0x1E,
    WEDDING_TREASURE_DRESS = 0x1F,
    WEDDING_TREASURE_WATER = 0x20,
    WEDDING_TREASURE_CAKE = 0x21,
    WEDDING_TREASURE_STEW = 0x22,

    SHINE_ICON_COMMON = 0x23,
    SHINE_ICON_CITY = 0x24,
    SHINE_ICON_FOREST = 0x25,
    SHINE_ICON_SKY = 0x26,
    SHINE_ICON_SNOW = 0x27,
    SHINE_ICON_SAND = 0x28,
    SHINE_ICON_LAVA = 0x29,
    SHINE_ICON_LAKE = 0x2A,
    SHINE_ICON_SEA = 0x2B,
    SHINE_ICON_MOON = 0x2C,
    SHINE_ICON_ALL = 0x2D,
    SHINE_ICON_NULL = 0x2E,

    ICON_STAR = 0x2F,
    ICON_STAR_EMPTY = 0x30,
    ICON_LIFE_UP_HEART = 0x31,
    ICON_CAP_ORIGINAL = 0x32,
    ICON_LUIGI = 0x33,

    // v1.2.0+
    ICON_BALLOON_GAME_ARROW = 0x34,
    ICON_BALLOON_GAME_SMALL_STAR = 0x35,
}

public enum TagNameProjectIcon : ushort
{
    ShineIconCurrentWorld = 0x0,
    CoinCollectIconCurrentWorld = 0x1,
    PadStyleButtonA = 0x2,
    PadStyleButtonB = 0x3,
    PadStyleButtonX = 0x4,
    PadStyleButtonY = 0x5,
    PadStyleButtonL = 0x6,
    PadStyleButtonR = 0x7,
    PadStyleButtonZL = 0x8,
    PadStyleButtonZR = 0x9,
    PadStyleButtonMinus = 0xA,
    PadStyleButtonPlus = 0xB,
    PadStyleKeyUp = 0xC,
    PadStyleKeyDown = 0xD,
    PadStyleKeyLeft = 0xE,
    PadStyleKeyRight = 0xF,
    PadStyleStickL = 0x10,
    PadStyleStickR = 0x11,
    PadStyleStickPushR = 0x12,
    PadStyleStickUD = 0x13,
    PadStyleStickLR = 0x14,
    PadStyleButtonCapture = 0x15,
    PadStyleJoyCon = 0x16,
    PadStyleJoyConR = 0x17,
    PadStyleReset = 0x18,
    PadStyleJoyConIconOnly = 0x19,
    PadStyle2PButtonY = 0x1A,
    PadStyle2PStickL = 0x1B,
    PadStyle2PStickR = 0x1C,
    PadPairMenu = 0x1D,
    PadPairMap = 0x1E,
}

public enum TagNameTextAlign : ushort
{
    Center = 0x0,
    Left = 0x1,
}

public enum TagNameGrammar : ushort
{
    Decap = 0x0,
    Cap = 0x1,
}

public enum TagFontIndex : ushort
{
    Device = 0x0,
    Head = 0x1,
    Icon16 = 0x2,
    Icon80 = 0x3,
    Message = 0x4,
    Number = 0x5,
    Picture = 0x6,
    Title = 0x7,
}