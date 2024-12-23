namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementVoice : MsbtTagElementWithTextData
{
    public string AudioName
    {
        get { return Text; }
        set { Text = value; }
    }

    public MsbtTagElementVoice(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementVoice(string audio)
        : base((ushort)TagGroup.PlaySe, 0)
    {
        AudioName = audio;
    }
    public MsbtTagElementVoice()
        : base((ushort)TagGroup.PlaySe, 0)
    {
        AudioName = "";
    }

    public override string GetTagNameStr() { return "Voice"; }
    public override string GetTextureName(int _) { return "Voice"; }
};