namespace Nindot.LMS.Msbt.TagLib.Smo;

public enum TagGroup : ushort
{
    SYSTEM = 0x0,
    EUI = 0x1,
    FORMAT_NUMBER = 0x2,
    TEXT_ANIM = 0x3,
    PLAY_SE = 0x4,
    FORMAT_STRING = 0x5,
    PROJECT_TAG = 0x6,
    TIME = 0x7,
    PICTURE_FONT = 0x8,
    DEVICE_FONT = 0x9,
    TEXT_ALIGN = 0xA,
    GRAMMAR = 0xC9,
}

public enum TagNameSystem : ushort
{
    RUBY_AND_FURIGANA = 0,
    FONT = 1,
    FONT_SIZE = 2,
    COLOR = 3,
    PAGE_BREAK = 4,
    REFERENCE = 5,
}

public enum TagNameEui : ushort
{
    PRINT_DELAY = 0,
    PRINT_SPEED = 1,
    FLUSH = 2,
    AUTO_NEXT = 3,
    CHOICE_2 = 4,
    CHOICE_3 = 5,
    CHOICE_4 = 6,
}

public enum TagNameFormatNumber : ushort
{
    SCORE = 0, // MsbtTagElementFormatting
    FIG_02 = 1, // MsbtTagElementUnknown
    RETRY_COIN = 2, // MsbtTagElementFormatting

    DATE = 3, // MsbtTagElementFormattingSimple
    RACE_TIME = 4, // MsbtTagElementFormattingSimple
    DATE_DETAIL = 5, // MsbtTagElementFormattingSimple
}

public enum TagNameTextAnim : ushort
{
    TREMBLE = 0,
    SHAKE = 1,
    WAVE = 2,
    SCREAM = 3,
    BEAT = 4,
};

public enum TagNameFormatString : ushort
{
    AMIIBO_NAME = 3,
    SHOP_OUTFIT_NAME = 4,
    MOON_NAME = 5,
    MINIGAME_NAME = 6,
    ACHIVEMENT_NAME = 9,
}

public enum TagNameTime : ushort
{
    YEAR = 0,
    MONTH = 1,
    DAY = 2,
    HOUR = 3,
    MINUTE = 4,
    SECOND = 5,
    MILLISECOND = 6
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
    ENUM_END = 0x34,
}

public enum TagNameDeviceFont : ushort
{
    PAD_BUTTON_RIGHT = 0x0,
    PAD_BUTTON_DOWN = 0x1,
    PAD_BUTTON_UP = 0x2,
    PAD_BUTTON_LEFT = 0x3,
    A = 0x4,
    B = 0x5,
    X = 0x6,
    Y = 0x7,
    L = 0x8,
    R = 0x9,
    ZL = 0xA,
    ZR = 0xB,
    SL = 0xC,
    SR = 0xD,
    PAD_ARROW_UP = 0xE,
    PAD_ARROW_DOWN = 0xF,
    PAD_ARROW_LEFT = 0x10,
    PAD_ARROW_RIGHT = 0x11,
    PLUS = 0x12,
    MINUS = 0x13,
    POWER_BUTTON = 0x14,
    HOME = 0x15,
    SCREENSHOT_CAPTURE = 0x16,
    STICK = 0x17,
    STICKL = 0x18,
    STICKR = 0x19,
    STICK_PUSH = 0x1A,
    STICKL_PUSH = 0x1B,
    STICKR_PUSH = 0x1C,

    // Four keys of padding fit here, no icon is assigned to anything between 0x1C and 0x21 (exclusive)

    STICKR_UP_DOWN = 0x21,
    STICKL_UP_DOWN = 0x22,
    STICKR_LEFT_RIGHT = 0x23,
    STICKL_LEFT_RIGHT = 0x24,
    JOY_CON = 0x25,
    JOY_CON_SINGLE_VERTICAL = 0x26,
    JOY_CON_SINGLE_HORIZONTAL = 0x27,
    PRO_CONTROLLER = 0x28,
    HOME_ALBUM = 0x29,
    HINT_PHOTO_ARROW = 0x2A,
    ENUM_END = 0x2B,
}