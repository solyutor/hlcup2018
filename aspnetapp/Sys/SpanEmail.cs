using System;
using System.Runtime.InteropServices;

namespace aspnetapp.Sys
{
    public readonly unsafe struct SpanEmail : IEquatable<SpanEmail>
    {
        private readonly byte* _email;
        private readonly byte _length;
        private readonly ushort _domain;

        public SpanEmail(byte* email, byte length, ushort domain)
        {
            _email = email;
            _length = length;
            _domain = domain;
        }

        public ReadOnlySpan<byte> Span => new ReadOnlySpan<byte>(_email, _length);

        public bool Equals(SpanEmail other) =>
            _domain == other._domain
                                                 && _length == other._length
            && Span.SequenceEqual(other.Span);

        public override bool Equals(object obj) => obj is SpanEmail other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                switch (_length)
                {
                    case 0:
                        return 0;
                    case 1:
                        return (Span[0] * 397) ^ _domain;
                    case 2:
                    case 3:
                        return ((Span[0] << 8) + Span[1] * 397) ^ _domain;
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        return (MemoryMarshal.Cast<byte, int>(Span)[0] * 397) ^ _domain;
                    default:
                        return (MemoryMarshal.Cast<byte, long>(Span)[0].GetHashCode() * 397) ^ _domain;
                }
            }
        }

        public static bool operator ==(SpanEmail left, SpanEmail right) => left.Equals(right);

        public static bool operator !=(SpanEmail left, SpanEmail right) => !left.Equals(right);
    }
}