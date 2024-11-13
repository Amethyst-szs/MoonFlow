using System;
using System.Collections.Generic;
using Godot;

namespace Nindot.LMS.Msbt.TagLib.Smo;

internal static class Builder
{
    internal static List<MsbtBaseElement> Build(byte[] buffer)
    {
        // Establish list to store all created elements
        List<MsbtBaseElement> list = [];

        // This variable will hold a copy of the current MsbtElement class
        MsbtBaseElement curElement = null;

        // Create a pointer into the buffer and continue iterating on pointer until completed
        int pointer = 0;

        while (pointer < buffer.Length)
        {
            // Get the data at the current pointer
            ushort value = BitConverter.ToUInt16(buffer, pointer);

            // If the pointer rests on a Tag bytecode, jump to the tag builder
            if (value == MsbtTagElement.BYTECODE_TAG || value == MsbtTagElement.BYTECODE_TAG_CLOSE)
            {
                // If the current element is a text element, run the finalizer
                if (curElement != null && curElement.GetType() == typeof(MsbtTextElement))
                    ((MsbtTextElement)curElement).FinalizeAppending();

                // Wipe the current element and move to the tag element builder
                curElement = null;

                MsbtTagElement tag = BuildTagElement(buffer, ref pointer);
                list.Add(tag);

                continue;
            }

            // If the current bytecode isn't a bytecode and we don't have a current element to work with, make a text element
            if (curElement == null)
            {
                curElement = new MsbtTextElement();
                list.Add(curElement);
            }

            // If the current element type isn't text, throw an error and break
            if (curElement.GetType() != typeof(MsbtTextElement))
            {
                GD.PushWarning("Parse error in MSBT : Cannot append to text element due to invalid type");
                break;
            }

            // Append text to the current text element
            ((MsbtTextElement)curElement).AppendChar16(value);
            pointer += 2;
        }

        // If the current element is a text element, run a finalizer on it to ensure _initial_text is valid
        if (curElement != null && curElement.GetType() == typeof(MsbtTextElement))
            ((MsbtTextElement)curElement).FinalizeAppending();

        return list;
    }

    private static MsbtTagElement BuildTagElement(byte[] buffer, ref int pointer)
    {
        // Jump pointer ahead by two to read tag group type byte
        pointer += 2;

        // Create a tag element depending on tag group type
        return BitConverter.ToUInt16(buffer, pointer) switch
        {
            (ushort)TagGroup.System => BuildTagElement_GroupNameSystem(buffer, ref pointer),
            (ushort)TagGroup.Eui => BuildTagElement_GroupNamePrintControl(buffer, ref pointer),
            (ushort)TagGroup.Number => BuildTagElement_GroupNameNumber(buffer, ref pointer),
            (ushort)TagGroup.TextAnim => new MsbtTagElementTextAnim(ref pointer, buffer),
            (ushort)TagGroup.String => new MsbtTagElementString(ref pointer, buffer),
            (ushort)TagGroup.PlaySe => new MsbtTagElementVoice(ref pointer, buffer),
            (ushort)TagGroup.ProjectTag => new MsbtTagElementProjectTag(ref pointer, buffer),
            (ushort)TagGroup.Time => new MsbtTagElementTimeComponent(ref pointer, buffer),
            (ushort)TagGroup.PictureFont => new MsbtTagElementPictureFont(ref pointer, buffer),
            (ushort)TagGroup.DeviceFont => new MsbtTagElementDeviceFont(ref pointer, buffer),
            (ushort)TagGroup.TextAlign => new MsbtTagElementTextAlign(ref pointer, buffer),
            (ushort)TagGroup.Grammar => new MsbtTagElementGrammar(ref pointer, buffer),
            _ => new MsbtTagElementUnknown(ref pointer, buffer),
        };
    }

    private static MsbtTagElement BuildTagElement_GroupNameSystem(byte[] buffer, ref int pointer)
    {
        // Grab ushort of tag name
        ushort tag = BitConverter.ToUInt16(buffer, pointer + 2);

        // Determine which class to create based on tag name
        return tag switch
        {
            (ushort)TagNameSystem.Ruby => new MsbtTagElementSystemRuby(ref pointer, buffer),
            (ushort)TagNameSystem.Font => new MsbtTagElementDeviceFont(ref pointer, buffer),
            (ushort)TagNameSystem.FontSize => new MsbtTagElementSystemFontSize(ref pointer, buffer),
            (ushort)TagNameSystem.Color => new MsbtTagElementSystemColor(ref pointer, buffer),
            (ushort)TagNameSystem.PageBreak => new MsbtTagElementSystemPageBreak(ref pointer, buffer),
            _ => new MsbtTagElementUnknown(ref pointer, buffer),
        };
    }
    private static MsbtTagElement BuildTagElement_GroupNamePrintControl(byte[] buffer, ref int pointer)
    {
        // Grab ushort of tag name
        ushort tag = BitConverter.ToUInt16(buffer, pointer + 2);

        // Determine which class to create based on tag name
        return tag switch
        {
            (ushort)TagNameEui.Wait => new MsbtTagElementEuiWait(ref pointer, buffer),
            (ushort)TagNameEui.Speed => new MsbtTagElementEuiSpeed(ref pointer, buffer),
            _ => new MsbtTagElementUnknown(ref pointer, buffer),
        };
    }
    private static MsbtTagElement BuildTagElement_GroupNameNumber(byte[] buffer, ref int pointer)
    {
        // Grab ushort of tag name
        ushort tag = BitConverter.ToUInt16(buffer, pointer + 2);

        // Determine which class to create based on tag name
        return tag switch
        {
            (ushort)TagNameNumber.Score => new MsbtTagElementNumberScore(ref pointer, buffer),
            (ushort)TagNameNumber.CoinNum => new MsbtTagElementNumberCoinNum(ref pointer, buffer),
            (ushort)TagNameNumber.Date => new MsbtTagElementNumberDate(ref pointer, buffer),
            (ushort)TagNameNumber.RaceTime => new MsbtTagElementNumberRaceTime(ref pointer, buffer),
            (ushort)TagNameNumber.DateDetail => new MsbtTagElementNumberDateDetail(ref pointer, buffer),
            
            //// v1.2.0+
            (ushort)TagNameNumber.DateEU => new MsbtTagElementNumberDate(ref pointer, buffer),
            (ushort)TagNameNumber.DateDetailEU => new MsbtTagElementNumberDateDetail(ref pointer, buffer),
            ////
            
            _ => new MsbtTagElementUnknown(ref pointer, buffer),
        };
    }
}