using System;
using System.Collections.Generic;

namespace Nindot.LMS.Msbt.TagLib.Smo;

public class MsbtElementFactoryProjectSmo : MsbtElementFactory
{
    internal override List<MsbtPage> Build(byte[] buffer)
    {
        // Create array to store all msbt pages, along with a default first page
        List<MsbtPage> pageList = [];
        MsbtPage curPage = [];
        pageList.Add(curPage);

        // Create a pointer into the buffer and continue iterating on pointer until completed
        int pointer = 0;

        while (pointer < buffer.Length)
        {
            // Get the data at the current pointer
            ushort value = BitConverter.ToUInt16(buffer, pointer);

            // If the pointer rests on a Tag bytecode, build the tag element
            if (value == MsbtTagElement.BYTECODE_TAG)
            {
                MsbtTagElement tag = BuildTagElement(buffer, ref pointer);

                // If the current tag has the "IsPageBreak" flag set, create a new page and skip adding this tag
                if (tag.IsPageBreak())
                {
                    curPage = [];
                    pageList.Add(curPage);
                    continue;
                }

                curPage.Add(tag);
                continue;
            }

            // If the pointer rests on a Tag End bytecode, insert an end element
            if (value == MsbtTagCloseElement.BYTECODE_TAG_CLOSE)
            {
                var element = new MsbtTagCloseElement(ref pointer, buffer);
                curPage.Add(element);
                continue;
            }

            // By this point we know that the next segment isn't a tag or tag close
            // Create a text element up to the next tag, or null terminator
            MsbtTextElement text = null;

            // Search for next tag byte or tag close byte
            int nextTagIdx = Array.FindIndex(buffer, pointer, c => c == MsbtTagElement.BYTECODE_TAG
                || c == MsbtTagCloseElement.BYTECODE_TAG_CLOSE);

            // If nextTag is -1, there are no more tags and the entire remaining data can be turned to a text element
            if (nextTagIdx == -1)
            {
                text = new MsbtTextElement(buffer[pointer..]);
                if (!text.IsEmpty())
                    curPage.Add(text);
                
                break;
            }

            // Otherwise, turn the range of pointer -> nextTag into a text element
            text = new MsbtTextElement(buffer[pointer..nextTagIdx]);
            if (text.IsEmpty())
                break;
            
            curPage.Add(text);
            pointer += text.Text.Length * sizeof(ushort);
        }

        return pageList;
    }

    public override string GetFactoryName() { return "Super Mario Odyssey"; }

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