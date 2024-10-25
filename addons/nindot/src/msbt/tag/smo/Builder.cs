using System.Collections.Generic;
using CommunityToolkit.HighPerformance;
using Godot;
using Nindot.MsbtContent;

namespace Nindot.MsbtTagLibrary.Smo;

public static class Builder
{
    internal const ushort ByteCode_Tag = 0x0E;
    internal const ushort ByteCode_TagClose = 0x0F;

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
            ushort value = System.BitConverter.ToUInt16(buffer, pointer);

            // If the pointer rests on a Tag bytecode, jump to the tag builder
            if (value == ByteCode_Tag || value == ByteCode_TagClose)
            {
                // If the current element is a text element, run the finalizer
                if (curElement.GetType() == typeof(MsbtTextElement))
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
        if (curElement.GetType() == typeof(MsbtTextElement))
            ((MsbtTextElement)curElement).FinalizeAppending();

        return list;
    }

    internal static MsbtTagElement BuildTagElement(byte[] buffer, ref int pointer)
    {
        // Jump pointer ahead by two to read tag group type byte
        pointer += 2;

        // Create a tag element depending on tag group type
        return buffer[pointer] switch
        {
            // (byte)TagGroup.SYSTEM => new MsbtTagElementUnknown(ref pointer, buffer),
            _ => new MsbtTagElementUnknown(ref pointer, buffer),
        };
    }
}