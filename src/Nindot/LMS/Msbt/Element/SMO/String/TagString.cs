using System;
using System.IO;

using CommunityToolkit.HighPerformance;

using Nindot.LMS.Msbp;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementString : MsbtTagElementWithTextData
{
    public string ReplacementKey
    {
        get { return Text; }
        set { Text = value; }
    }

    public bool IsTextless { get; private set; } = false;

    public MsbtTagElementString(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementString(MsbpFile project, string tagName, string replacementKey)
        : base((ushort)TagGroup.String, 0)
    {
        // Get tagName's index
        TagGroupInfo group = project.TagGroup_Get((int)TagGroup.String);
        int tag = project.Tag_GetIndex(tagName, group);

        if (tag == -1)
            throw new MsbtException(string.Format("Failed to lookup tag index from project: {0}", tagName));

        TagName = (ushort)tag;

        // Set text value
        if (replacementKey.Length != 0)
            ReplacementKey = replacementKey;
    }
    public MsbtTagElementString(MsbpFile project, string tagName)
        : base((ushort)TagGroup.String, 0)
    {
        // Get tagName's index
        TagGroupInfo group = project.TagGroup_Get((int)TagGroup.String);
        int tag = project.Tag_GetIndex(tagName, group);

        if (tag == -1)
            throw new MsbtException(string.Format("Failed to lookup tag index from project: {0}", tagName));

        TagName = (ushort)tag;
        IsTextless = true;
    }

    internal override void InitTag(ref int pointer, byte[] buffer, ushort dataSize)
    {
        // Attempt to read text data
        int oldPointer = pointer;
        ReadTextData(ref pointer, buffer);

        // If the text read failed, setup the textless version of the string replace tag
        if (pointer - 2 == oldPointer && Text.Length == 0x0)
        {
            IsTextless = true;
            pointer += 2;
            return;
        }
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        if (IsTextless)
        {
            const uint pad = 0x0;
            value.Write(pad);
            return value.ToArray();
        }

        WriteTextData(value);
        return value.ToArray();
    }

    public override ushort CalcDataSize()
    {
        if (!IsTextless)
            return base.CalcDataSize();

        return 0x4;
    }

    public override string GetTagNameStr()
    {
        return string.Format("String Tag: {0}", TagName);
    }

    public override string GetTextureName() { return "String"; }
};