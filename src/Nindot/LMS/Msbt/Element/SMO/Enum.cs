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
    // registered as an MsbtTagElementUnknown for now.
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

public enum IconCodePictureFont : ushort
{
    GlyphDot = 0x2E,
    GlyphSlash = 0x2F,

    IconMoonRock = 0x30,
    IconAchivementNpc = 0x31,
    IconHintNpc = 0x32,
    IconPoetter = 0x33,
    IconAmiibo = 0x34,
    IconStar = 0x35,
    IconStarDotted = 0x36,
    IconLifeMaxUpItem = 0x37,
    IconCapManHero = 0x38,
    IconLuigi = 0x39,

    GlyphColon_IconBalloonHintArrow = 0x3A, // Colon replaced with the arrow in update 1.2.0+
    IconStarSmall = 0x3B, // v1.2.0+ only

    IconCoin = 0x40,
    IconEarth = 0x41,
    IconCheckpoint = 0x42,
    IconKoopa = 0x43,
    IconPeach = 0x44,
    IconTiara = 0x45,
    IconBroodalCapThrower = 0x46,
    IconBoodalFireBlower = 0x47,
    IconBoodalStacker = 0x48,
    IconBoodalBombTail = 0x49,
    IconHomeShip = 0x4A,
    IconFrog = 0x4B,
    IconMario = 0x4C,
    IconCap = 0x4D,
    IconMarioCapOff = 0x4E,
    IconPauline = 0x4F,

    CoinCollectCap = 0x50,
    CoinCollectWaterfall = 0x51,
    CoinCollectSand = 0x52,
    CoinCollectForest = 0x53,
    CoinCollectLake = 0x54,
    CoinCollectClash = 0x55,
    CoinCollectCity = 0x56,
    CoinCollectSea = 0x57,
    CoinCollectSnow = 0x58,
    CoinCollectLava = 0x59,
    CoinCollectSky = 0x5A,
    CoinCollectMoon = 0x5B,
    CoinCollectPeach = 0x5C,

    TreasureRing = 0x60,
    TreasureFlower = 0x61,
    TreasureDress = 0x62,
    TreasureWater = 0x63,
    TreasureCake = 0x64,
    TreasureStew = 0x65,

    ShineCommon = 0x70,
    ShineCity = 0x71,
    ShineForest = 0x72,
    ShineSky = 0x73,
    ShineSnow = 0x74,
    ShineSand = 0x75,
    ShineLava = 0x76,
    ShineLake = 0x77,
    ShineSea = 0x78,
    ShineMoon = 0x79,
    ShineRainbow = 0x7A,
    ShineNull = 0x7B,
    ShineTripleDot = 0x7C,
}

public enum TagNameDeviceFont : ushort
{
    ButtonGroupRight,
    ButtonGroupBottom,
    ButtonGroupUp,
    ButtonGroupLeft,
    ButtonA,
    ButtonB,
    ButtonX,
    ButtonY,
    ButtonL,
    ButtonR,
    ButtonZL,
    ButtonZR,
    ButtonSL,
    ButtonSR,
    ButtonUp,
    ButtonDown,
    ButtonLeft,
    ButtonRight,
    ButtonPlus,
    ButtonMinus,
    ButtonPower,
    ButtonHome,
    ButtonCapture,
    Stick,
    StickL,
    StickR,
    StickPush,
    StickPushL,
    StickPushR,
    KeyUp,
    KeyDown,
    KeyLeft,
    KeyRight,
    StickUD,
    StickUDL,
    StickLRR,
    ButtonLR,
    JoyConBoth,
    JoyConRight,
    JoyConSingle,
    ProCon,
    ArrowRight,
    Album,
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