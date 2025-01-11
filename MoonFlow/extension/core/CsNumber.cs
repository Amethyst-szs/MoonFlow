using System;

namespace MoonFlow.Ext;

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
}