using System;
using System.IO;

using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib.Smo;

// Used for rendering Japanese Furigana
public class MsbtTagElementSystemRuby : MsbtTagElementWithTextData
{
    public ushort Code = 0;

    public MsbtTagElementSystemRuby(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementSystemRuby(ushort code, string content)
        : base((ushort)TagGroup.System, (ushort)TagNameSystem.Ruby)
    {
        Code = code;
        Text = content;
    }

    internal override void InitTag(ref int pointer, byte[] buffer, ushort dataSize)
    {
        Code = BitConverter.ToUInt16(buffer, pointer);
        pointer += sizeof(ushort);

        ReadTextData(ref pointer, buffer);
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(Code);
        WriteTextData(value);

        return value.ToArray();
    }

    public override ushort CalcDataSize() { return (ushort)(base.CalcDataSize() + sizeof(ushort)); }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameSystem), TagName))
            return Enum.GetName(typeof(TagNameSystem), TagName);

        return "Unknown";
    }

    public override string GetTextureName() { return "System_Ruby"; }
};