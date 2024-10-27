using System;
using System.Collections.Generic;
using Godot;

namespace Nindot.MsbtTagLibrary.Smo;

public static class Builder
{
    public const ushort ByteCode_Tag = 0x0E;
    public const ushort ByteCode_TagClose = 0x0F;

    public static List<MsbtBaseElement> Build(byte[] buffer)
    {
        // Establish list to store all created elements
        List<MsbtBaseElement> list = new();

        // This variable will hold a copy of the current MsbtElement class
        MsbtBaseElement curElement = null;

        // Create a pointer into the buffer and continue iterating on pointer until completed
        int pointer = 0;

        while (pointer < buffer.Length)
        {
            // Get the data at the current pointer
            ushort value = BitConverter.ToUInt16(buffer, pointer);

            // If the pointer rests on a Tag bytecode, jump to the tag builder
            if (value == ByteCode_Tag || value == ByteCode_TagClose)
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
            (ushort)TagGroup.SYSTEM => BuildTagElement_GroupNameSystem(buffer, ref pointer),
            (ushort)TagGroup.PRINT_CONTROL => BuildTagElement_GroupNamePrintControl(buffer, ref pointer),
            (ushort)TagGroup.FORMAT_REPLACEMENT => BuildTagElement_GroupNameFormatting(buffer, ref pointer),
            (ushort)TagGroup.SHAKE_ANIMATOR => new MsbtTagElementShake(ref pointer, buffer),
            (ushort)TagGroup.OBJECTIVE_NAME => new MsbtTagElementObjectiveName(ref pointer, buffer),
            (ushort)TagGroup.VOICE => new MsbtTagElementVoiceAudio(ref pointer, buffer),
            (ushort)TagGroup.PROJECT_TAG => new MsbtTagElementProjectIcon(ref pointer, buffer),
            (ushort)TagGroup.TIME => new MsbtTagElementTime(ref pointer, buffer),
            (ushort)TagGroup.PICTURE_FONT => new MsbtTagElementPictureFont(ref pointer, buffer),
            (ushort)TagGroup.DEVICE_FONT => new MsbtTagElementDeviceFont(ref pointer, buffer),
            (ushort)TagGroup.LANGUAGE_SPECIAL => new MsbtTagElementLanguageSpecial(ref pointer, buffer),
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
            (ushort)TagNameSystem.FURIGANA => new MsbtTagElementSystemFurigana(ref pointer, buffer),
            (ushort)TagNameSystem.FONT_SIZE => new MsbtTagElementDeviceFontSize(ref pointer, buffer),
            (ushort)TagNameSystem.COLOR => new MsbtTagElementSystemColor(ref pointer, buffer),
            (ushort)TagNameSystem.PAGE_BREAK => new MsbtTagElementSystemPageBreak(ref pointer, buffer),
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
            (ushort)TagNamePrintControl.PRINT_DELAY => new MsbtTagElementPrintDelay(ref pointer, buffer),
            (ushort)TagNamePrintControl.PRINT_SPEED => new MsbtTagElementPrintSpeed(ref pointer, buffer),
            _ => new MsbtTagElementUnknown(ref pointer, buffer),
        };
    }
    private static MsbtTagElement BuildTagElement_GroupNameFormatting(byte[] buffer, ref int pointer)
    {
        // Grab ushort of tag name
        ushort tag = BitConverter.ToUInt16(buffer, pointer + 2);

        // Determine which class to create based on tag name
        return tag switch
        {
            (ushort)TagNameFormatting.NORMAL => new MsbtTagElementFormatting(ref pointer, buffer),
            (ushort)TagNameFormatting.RETRY_COIN => new MsbtTagElementFormatting(ref pointer, buffer),
            (ushort)TagNameFormatting.DATE => new MsbtTagElementFormattingSimple(ref pointer, buffer),
            (ushort)TagNameFormatting.RACE_TIME => new MsbtTagElementFormattingSimple(ref pointer, buffer),
            (ushort)TagNameFormatting.DATE_DETAIL => new MsbtTagElementFormattingSimple(ref pointer, buffer),
            _ => new MsbtTagElementUnknown(ref pointer, buffer),
        };
    }
}