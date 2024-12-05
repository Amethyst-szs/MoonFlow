namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementNumberDate : MsbtTagElementWithTextData
{
    public string ReplacementKey
    {
        get { return Text; }
        set { Text = value; }
    }

    public MsbtTagElementNumberDate(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementNumberDate(string replacementKey, bool isEU)
        : base((ushort)TagGroup.Number, (ushort)(isEU ? TagNameNumber.DateEU : TagNameNumber.Date))
    {
        ReplacementKey = replacementKey;
    }

    public override string GetTagNameStr() { return "Date"; }

    public override string GetTextureName(int _) { return "Number_Time"; }
};

public class MsbtTagElementNumberDateDetail : MsbtTagElementWithTextData
{
    public string ReplacementKey
    {
        get { return Text; }
        set { Text = value; }
    }

    public MsbtTagElementNumberDateDetail(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementNumberDateDetail(string replacementKey, bool isEU)
        : base((ushort)TagGroup.Number, (ushort)(isEU ? TagNameNumber.DateDetailEU : TagNameNumber.DateDetail))
    {
        ReplacementKey = replacementKey;
    }

    public override string GetTagNameStr() { return "Date (Detail)"; }

    public override string GetTextureName(int _) { return "Number_Time"; }
};

public class MsbtTagElementNumberRaceTime : MsbtTagElementWithTextData
{
    public string ReplacementKey
    {
        get { return Text; }
        set { Text = value; }
    }

    public MsbtTagElementNumberRaceTime(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementNumberRaceTime(string replacementKey)
        : base((ushort)TagGroup.Number, (ushort)TagNameNumber.RaceTime)
    {
        ReplacementKey = replacementKey;
    }

    public override string GetTagNameStr() { return "Race Time"; }

    public override string GetTextureName(int _) { return "Number_Time"; }
};