namespace System;

public static partial class MathUtil
{
    public static bool AlmostZero(this double v)
    {
        return Math.Abs(v) <= 0.01;
    }
}