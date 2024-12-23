using System;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementNumberTime : MsbtTagElementWithTextData
{
    public string ReplacementKey
    {
        get { return Text; }
        set { Text = value; }
    }

    public MsbtTagElementNumberTime(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementNumberTime(TagNameNumber type, string replacementKey)
        : base((ushort)TagGroup.Number, (ushort)type)
    {
        ReplacementKey = replacementKey;

        if (type < TagNameNumber.Date)
            throw new Exception("Invalid TagName!");
    }

    public override string GetTagNameStr()
    {
        return TagName switch
        {
            (ushort)TagNameNumber.Date => "Date",
            (ushort)TagNameNumber.DateDetail => "DateDetail",
            (ushort)TagNameNumber.RaceTime => "RaceTime",

            (ushort)TagNameNumber.DateEU => "DateEU",
            (ushort)TagNameNumber.DateDetailEU => "DateDetailEU",

            _ => throw new Exception("Invalid TagName!"),
        };
    }

    public override string GetTextureName(int _) { return "Number_Time"; }
};