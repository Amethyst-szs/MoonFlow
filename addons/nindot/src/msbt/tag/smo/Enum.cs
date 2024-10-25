namespace Nindot.MsbtTagLibrary.Smo;

enum TagGroup : ushort
{
    SYSTEM = 0,
    PRINT_CONTROL = 1,
    FORMAT_REPLACEMENT = 2,
    SHAKE_ANIMATOR = 3,
    VOICE = 4,
    PROJECT_TAG = 6,
    TIME = 7,
    PICTURE_FONT = 8,
    DEVICE_FONT = 9,
}

enum TagNameSystem : ushort
{
    FONT_SIZE = 2,
    COLOR = 3,
    PAGE_BREAK = 4,
}

enum TagNamePrintControl : ushort
{
    PRINT_DELAY = 0,
    PRINT_SPEED = 1,
}

enum TagNameShakeAnimator : ushort
{
    LIGHT_SHAKE = 0,
    STRONG_SHAKE = 1,
    WIGGLE = 2,
    VERY_STRONG_SHAKE = 3,
    TEXT_BOX_PULSE = 4
}

enum TagNameTime : ushort
{
    YEAR = 0,
    MONTH = 1,
    DAY = 2,
    HOUR = 3,
    MINUTE = 4,
    SECOND = 5,
    MILLISECOND = 6
}