using System;
using System.IO;
using Godot;

using CommunityToolkit.HighPerformance;

namespace Nindot.MsbtTagLibrary.Smo;

public class MsbtTagElementVoiceAudio : MsbtTagElement
{
    public ushort NameLength = 0;

    private string _audioName;
    public string AudioName
    {
        get { return _audioName; }
        set {
            byte[] valueBuf = value.ToUtf16Buffer();

            NameLength = (ushort)valueBuf.Length;
            DataSize = (ushort)(NameLength + 0x2);
            IsInvalid = false;

            _audioName = value;
        }
    }

    private bool IsInvalid = false;

    public MsbtTagElementVoiceAudio(ref int pointer, byte[] buffer) : base(ref pointer, buffer)
    {
        // Copy data from buffer at pointer
        NameLength = BitConverter.ToUInt16(buffer, pointer);
        pointer += 0x2;

        // Before copying the audio name, ensure that the string length property is exactly
        // equal to DataSize minus 0x2 (the size of the one proceeding property)
        if (!IsValid()) {
            // In the event the data isn't valid, wipe the proposed string length and set the data size to just
            // the consistent bytes
            DataSize = 0x2;
            NameLength = 0x0;
            IsInvalid = true;
            return;
        }

        // Now we can safely read the string out
        int endPointer = pointer + NameLength;
        AudioName = buffer[pointer..endPointer].GetStringFromUtf16();

        pointer = endPointer;
    }

    public override byte[] GetBytes()
    {
        MemoryStream value = CreateMemoryStreamWithHeaderData();
        value.Write(NameLength);

        if (AudioName != null && NameLength > 0)
            value.Write(AudioName.ToUtf16Buffer());
        
        return value.ToArray();
    }

    public override bool IsValid()
    {
        if (IsInvalid)
            return false;
        
        if (DataSize % 2 != 0 || NameLength % 2 != 0)
            return false;
        
        if (DataSize - 0x2 != NameLength)
            return false;
        
        return true;
    }

    public override bool IsFixedDataSize()
    {
        return false;
    }
};