using System.IO;
using Godot;

namespace Nindot
{
    namespace MsbtContent
    {
        public abstract class MsbtBaseElement
        {
            public abstract string GetText();

            public abstract byte[] GetBytes();

            public abstract void WriteBytes(ref MemoryStream stream);
        }

        public class MsbtTextElement : MsbtBaseElement
        {
            internal string _text;

            public MsbtTextElement(string txt)
            {
                _text = txt;
            }

            public MsbtTextElement(byte[] buffer)
            {
                _text = buffer.GetStringFromUtf16();
            }

            public override string GetText()
            {
                return _text;
            }

            public override byte[] GetBytes()
            {
                return _text.ToUtf16Buffer();
            }

            public override void WriteBytes(ref MemoryStream stream)
            {
                byte[] buffer = GetBytes();
                stream.Write(buffer);
            }
        }

        public class MsbtTagElementGeneric : MsbtBaseElement
        {
            internal byte[] _buffer;

            public MsbtTagElementGeneric(byte[] buf)
            {
                _buffer = buf;
            }

            public override byte[] GetBytes()
            {
                return _buffer;
            }

            public override string GetText()
            {
                return _buffer.GetStringFromUtf16();
            }

            public override void WriteBytes(ref MemoryStream stream)
            {
                stream.Write(_buffer);
            }
        }
    }
}