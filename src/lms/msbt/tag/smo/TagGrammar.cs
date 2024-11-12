using System;
using Godot;

namespace Nindot.LMS.Msbt.TagLib.Smo;
public class MsbtTagElementGrammar : MsbtTagElement
{
    public TagNameGrammar Grammar
    {
        get { return (TagNameGrammar)TagName; }
        set
        {
            if (!Enum.IsDefined(typeof(TagNameGrammar), value))
            {
                GD.PushWarning("Attempted to set Tag Grammar to invalid alignment, set to default value instead");
                TagName = (ushort)TagNameGrammar.DECAP;
            }
            else
            {
                TagName = (ushort)value;
            }
        }
    }

    public MsbtTagElementGrammar(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
    {
        Grammar = (TagNameGrammar)TagName;
    }

    public override string GetTagNameStr()
    {
        if (Enum.IsDefined(typeof(TagNameGrammar), TagName))
            return Enum.GetName(typeof(TagNameGrammar), TagName);

        return "Unknown";
    }
};