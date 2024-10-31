using System;
using System.Collections.Generic;
using Godot;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public static class Builder
{
    public static List<MsbtBaseElement> Build(byte[] buffer, MsbtFile parent)
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

                MsbtTagElement tag = BuildTagElement(buffer, ref pointer, parent);
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

    private static MsbtTagElement BuildTagElement(byte[] buffer, ref int pointer, MsbtFile parent)
    {
        // Jump pointer ahead by two to read tag group type byte
        pointer += 2;

        // Create a tag element depending on tag group type
        return BitConverter.ToUInt16(buffer, pointer) switch
        {
            (ushort)TagGroup.SYSTEM => BuildTagElement_GroupNameSystem(buffer, ref pointer, parent),
            (ushort)TagGroup.EUI => BuildTagElement_GroupNamePrintControl(buffer, ref pointer, parent),
            (ushort)TagGroup.FORMAT_NUMBER => BuildTagElement_GroupNameFormatting(buffer, ref pointer, parent),
            (ushort)TagGroup.TEXT_ANIM => new MsbtTagElementShake(ref pointer, buffer, parent),
            (ushort)TagGroup.FORMAT_STRING => new MsbtTagElementObjectiveName(ref pointer, buffer, parent),
            (ushort)TagGroup.PLAY_SE => new MsbtTagElementVoiceAudio(ref pointer, buffer, parent),
            (ushort)TagGroup.PROJECT_TAG => new MsbtTagElementProjectIcon(ref pointer, buffer, parent),
            (ushort)TagGroup.TIME => new MsbtTagElementTime(ref pointer, buffer, parent),
            (ushort)TagGroup.PICTURE_FONT => new MsbtTagElementPictureFont(ref pointer, buffer, parent),
            (ushort)TagGroup.DEVICE_FONT => new MsbtTagElementDeviceFont(ref pointer, buffer, parent),
            (ushort)TagGroup.TEXT_ALIGN => new MsbtTagElementLanguageSpecial(ref pointer, buffer, parent),
            _ => new MsbtTagElementUnknown(ref pointer, buffer, parent),
        };
    }

    private static MsbtTagElement BuildTagElement_GroupNameSystem(byte[] buffer, ref int pointer, MsbtFile parent)
    {
        // Grab ushort of tag name
        ushort tag = BitConverter.ToUInt16(buffer, pointer + 2);

        // Determine which class to create based on tag name
        return tag switch
        {
            (ushort)TagNameSystem.RUBY_AND_FURIGANA => new MsbtTagElementSystemFurigana(ref pointer, buffer, parent),
            (ushort)TagNameSystem.FONT_SIZE => new MsbtTagElementDeviceFontSize(ref pointer, buffer, parent),
            (ushort)TagNameSystem.COLOR => new MsbtTagElementSystemColor(ref pointer, buffer, parent),
            (ushort)TagNameSystem.PAGE_BREAK => new MsbtTagElementSystemPageBreak(ref pointer, buffer, parent),
            _ => new MsbtTagElementUnknown(ref pointer, buffer, parent),
        };
    }
    private static MsbtTagElement BuildTagElement_GroupNamePrintControl(byte[] buffer, ref int pointer, MsbtFile parent)
    {
        // Grab ushort of tag name
        ushort tag = BitConverter.ToUInt16(buffer, pointer + 2);

        // Determine which class to create based on tag name
        return tag switch
        {
            (ushort)TagNameEui.PRINT_DELAY => new MsbtTagElementPrintDelay(ref pointer, buffer, parent),
            (ushort)TagNameEui.PRINT_SPEED => new MsbtTagElementPrintSpeed(ref pointer, buffer, parent),
            _ => new MsbtTagElementUnknown(ref pointer, buffer, parent),
        };
    }
    private static MsbtTagElement BuildTagElement_GroupNameFormatting(byte[] buffer, ref int pointer, MsbtFile parent)
    {
        // Grab ushort of tag name
        ushort tag = BitConverter.ToUInt16(buffer, pointer + 2);

        // Determine which class to create based on tag name
        return tag switch
        {
            (ushort)TagNameFormatNumber.SCORE => new MsbtTagElementFormatting(ref pointer, buffer, parent),
            (ushort)TagNameFormatNumber.RETRY_COIN => new MsbtTagElementFormatting(ref pointer, buffer, parent),
            (ushort)TagNameFormatNumber.DATE => new MsbtTagElementFormattingSimple(ref pointer, buffer, parent),
            (ushort)TagNameFormatNumber.RACE_TIME => new MsbtTagElementFormattingSimple(ref pointer, buffer, parent),
            (ushort)TagNameFormatNumber.DATE_DETAIL => new MsbtTagElementFormattingSimple(ref pointer, buffer, parent),
            _ => new MsbtTagElementUnknown(ref pointer, buffer, parent),
        };
    }
}