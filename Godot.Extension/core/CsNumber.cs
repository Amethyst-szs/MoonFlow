using System;

namespace Godot.Extension;

public static partial class Extension
{
    public static int ModPosNeg(this int x, int m)
    {
        return (x % m + m) % m;
    }

    public static bool AlmostZero(this float v)
    {
        return Math.Abs(v) <= 0.01;
    }
    public static bool AlmostZero(this double v)
    {
        return Math.Abs(v) <= 0.01;
    }

    public static DateTime UnixToDateTime(this long time)
    {
        var dtBase = new DateTime(time * TimeSpan.TicksPerSecond, DateTimeKind.Utc);
        var span = TimeSpan.FromTicks(DateTime.UnixEpoch.Ticks);

        return dtBase + span;
    }
}