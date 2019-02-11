using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using aspnetapp.Collections;

namespace aspnetapp.Sys
{
    public readonly unsafe struct Utf8String : IEquatable<Utf8String>
    {
        public static readonly Utf8String EmptyString = new Utf8String();

        public readonly byte* Pointer;
        public readonly int Length;


        public Utf8String(byte* pointer, int length)
        {
            Pointer = pointer;
            Length = length;
        }

        public Span<byte> Span => new Span<byte>(Pointer, Length);
        public bool IsEmpty => Length == 0;
        public static readonly ContentComparer DefaultComparer = new ContentComparer();

        public bool Equals(Utf8String other)
        {
            if (Pointer == other.Pointer)
            {
                return true;
            }

            if (Length != other.Length)
            {
                return false;
            }

            return Span.SequenceEqual(other.Span);
        }

        public override bool Equals(object obj) => obj is Utf8String other && Equals(other);

        public override int GetHashCode() => ((long)Pointer).GetHashCode();

        public static bool operator ==(Utf8String left, Utf8String right) => left.Equals(right);

        public static bool operator !=(Utf8String left, Utf8String right) => !left.Equals(right);

        public override string ToString() => Encoding.UTF8.GetString(Span);

        public bool Equals(in ReadOnlySpan<byte> value) => Span.SequenceEqual(value);

        public class ContentComparer : IEqualityComparer<Utf8String>
        {
            public bool Equals(Utf8String x, Utf8String y)
            {
                return x.Length == y.Length && x.Span.SequenceEqual(y.Span);
            }

            public int GetHashCode(Utf8String obj)
            {
                //TODO: Consider better hashcode cause names here are prone to be similar
                var lp = (long*)obj.Pointer;
                switch (obj.Length)
                {
                    case 0:
                        return 0;
                    case 1:
                        return obj.Pointer[0];
                    case 2:
                        return ((short*)obj.Pointer)[0];
                    case 3:
                        return (((short*)obj.Pointer)[0] << 16 + obj.Pointer[2]);
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        return ((int*)obj.Pointer)[0];
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                        return lp[0].GetHashCode();
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                        return lp[0].GetHashCode() * 397 ^ ((int*)obj.Pointer)[12];
                    case 16:
                    case 17:
                    case 18:
                    case 19:
                        return lp[0].GetHashCode() * 397 ^ lp[1].GetHashCode();
                    case 20:
                    case 21:
                    case 22:
                    case 23:
                    {

                        return (lp[0].GetHashCode() * 397) ^ (lp[1].GetHashCode() * 397) ^ ((int*)obj.Pointer)[16];
                    }
                    default:
                    {
                        var hash = lp[0].GetHashCode() * 397;
                        hash = hash ^ lp[1].GetHashCode() * 397;
                        var elp = (long*)(obj.Pointer + obj.Length - 8);
                        hash = hash ^ elp[0].GetHashCode() * 397;
                        return hash ^ obj.Length;
                    }

                }
            }
        }
    }
}