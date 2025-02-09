using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.HighPerformance;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtTagElementProjectTag : MsbtTagElement
{
    public TagNameProjectIcon Icon
    {
        get { return (TagNameProjectIcon)TagName; }
        set
        {
            TagName = (ushort)value;
            UpdateIconTableByTag();
        }
    }

    private List<string> _iconTable = [];

    public MsbtTagElementProjectTag(ref int pointer, byte[] buffer) : base(ref pointer, buffer) { }
    public MsbtTagElementProjectTag(TagNameProjectIcon icon)
        : base((ushort)TagGroup.ProjectTag, (ushort)icon)
    {
        Icon = icon;
    }

    internal override void InitTag(ref int pointer, byte[] buffer, ushort dataSize)
    {
        // Assign Icon to TagName to run update function
        // The buffer contains the IconTable, however the information in that buffer can be
        // entirely inferred from the TagName. To simplfy the read/edit/write on this tag,
        // the IconTable will be generated by the update function instead of the buffer.
        Icon = (TagNameProjectIcon)TagName;

        // Push ahead the pointer with CalcDataSize
        pointer += CalcDataSize();
    }

    public void SetIcon(TagNameProjectIcon icon)
    {
        Icon = icon;
        UpdateIconTableByTag();
    }

    public override byte[] GetBytes()
    {
        // Now that data size is correct, stream can be created
        MemoryStream value = CreateMemoryStreamWithHeaderData();

        foreach (string icon in _iconTable)
        {
            byte[] buf = Encoding.Unicode.GetBytes(icon);
            ushort strLen = (ushort)buf.Length;

            value.Write(strLen);
            value.Write(buf);
        }

        return value.ToArray();
    }

    public override ushort CalcDataSize()
    {
        ushort value = 0;

        foreach (string icon in _iconTable)
        {
            int strLen = icon.Length * sizeof(ushort);
            value += (ushort)(strLen + sizeof(ushort));
        }

        return value;
    }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameProjectIcon), TagName))
            return Enum.GetName(typeof(TagNameProjectIcon), TagName);

        return "Unknown";
    }

    public string GetTagNameStrSuffix()
    {
        string str = GetTagNameStr();

        return str switch
        {
            string s when s.StartsWith("PadStyle2P") => str.Replace("PadStyle2P", ""),
            string s when s.StartsWith("PadStyle") => str.Replace("PadStyle", ""),
            string s when s.StartsWith("PadPair") => str.Replace("PadPair", ""),
            _ => str,
        };
    }

    private void UpdateIconTableByTag()
    {
        var tagName = GetTagNameStr();
        if (tagName == null || tagName == "Unknown")
            return;

        switch (tagName)
        {
            case "ShineIconCurrentWorld" or "CoinCollectIconCurrentWorld":
                _iconTable.Clear(); // These two tags do not use the icon table
                break;
            case string s when s.StartsWith("PadStyle2P"):
                UpdateIconTableForPadStyle(tagName, true);
                break;
            case string s when s.StartsWith("PadStyle"):
                UpdateIconTableForPadStyle(tagName, false);
                break;
            case string s when s.StartsWith("PadPair"):
                UpdateIconTableForPadPair(tagName);
                break;
            default:
                Console.Error.WriteLine("Cannot update IconTable for invalid TagName!");
                break;
        }
    }

    private void UpdateIconTableForPadStyle(string tagName, bool is2P)
    {
        string suffix;
        if (is2P)
            suffix = tagName.Replace("PadStyle2P", "");
        else
            suffix = tagName.Replace("PadStyle", "");

        _iconTable.Clear();
        foreach (var prefix in _padStylePrefixes)
        {
            _iconTable.Add(prefix + suffix);
        }
    }

    private void UpdateIconTableForPadPair(string tagName)
    {
        var prefix = tagName.Replace("PadPair", "");

        _iconTable.Clear();
        foreach (var suffix in _padPairSuffixes)
        {
            _iconTable.Add(prefix + suffix);
        }
    }

    private static readonly string[] _padStylePrefixes = [
        "Dual",
        "Handheld",
        "FullKey",
        "Left",
        "Right",
    ];

    private static readonly string[] _padPairSuffixes = [
        "Button",
        "PlayerL",
        "PlayerR",
    ];

    public override string GetTextureName(int tableIndex)
    {
        if (Icon == TagNameProjectIcon.ShineIconCurrentWorld)
            return "ProjectTag";

        if (Icon == TagNameProjectIcon.CoinCollectIconCurrentWorld)
            return "ProjectTag_CoinCollect";

        if (Icon >= TagNameProjectIcon.PadPairMenu)
            tableIndex = Math.Clamp(tableIndex - 2, 0, 2);
        
        if (tableIndex < 0 || tableIndex >= _iconTable.Count)
            return null;

        return _iconTable[tableIndex];
    }

    public ReadOnlyCollection<string> GetIconTable()
    {
        return new ReadOnlyCollection<string>(_iconTable);
    }
};