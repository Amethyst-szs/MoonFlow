namespace MoonFlow.Ext;

public static partial class Extension
{
	public static int ModPosNeg(this int x, int m)
	{
		return (x % m + m) % m;
	}
}