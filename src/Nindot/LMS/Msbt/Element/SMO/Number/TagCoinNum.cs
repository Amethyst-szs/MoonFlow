namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementNumberCoinNum : MsbtTagElementNumberScore
{
    public MsbtTagElementNumberCoinNum(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementNumberCoinNum(string replacementKey)
        : base(replacementKey, TagNameNumber.CoinNum)
    {
        ReplacementKey = replacementKey;
    }

    public override string GetTagNameStr() { return "Coin Number"; }

    public override string GetTextureName(int _) { return "Number_Coin"; }
};