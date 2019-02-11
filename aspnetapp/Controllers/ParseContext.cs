using System;
using System.Text;

namespace aspnetapp.Controllers
{
    public unsafe ref struct ParseContext
    {
        private readonly byte* _p;
        private byte* _current;
        private int _length;

        public int Length => _length;
        public Span<byte> Span => new Span<byte>(_current, _length);
        public byte* CurrentPointer => _current;
        public Span<byte> WrittenSpan => new Span<byte>(_p, WrittenLength);

        public int WrittenLength => (int) (_current - _p);

        public string WrittenText => Encoding.UTF8.GetString(WrittenSpan);

        public ParseContext(byte* p, int length)
        {
            _p = p;
            _length = length;
            _current = _p;
        }

        public void Move(int count)
        {
            _current += count;
            _length -= count;
        }

        public static implicit operator Span<byte>(in ParseContext value) => value.Span;

        public static implicit operator ReadOnlySpan<byte>(in ParseContext value) => value.Span;

        public int IndexOf(byte value) => Span.IndexOf(value);

        public byte this[int i] => _current[i];

        public bool StartsWith(ReadOnlySpan<byte> prefix) => Span.StartsWith(prefix);

        public int IndexOfAny(byte value1, byte value2)
        {
            return Span.IndexOfAny(value1, value2);
        }

        public override string ToString() => Encoding.UTF8.GetString(Span);

        public void Write(Span<byte> buffer)
        {
            buffer.CopyTo(Span);
            Move(buffer.Length);
        }

        public void WriteByte(byte separator)
        {
            _current[0] = separator;
            _current++;
        }
    }
}