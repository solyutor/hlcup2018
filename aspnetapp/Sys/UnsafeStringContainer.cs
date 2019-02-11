using System;
using System.Buffers.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace aspnetapp.Sys
{
    public static unsafe class UnsafeStringContainer
    {
        private const int TotalLength = 40  * 1024 * 1024;
        private static readonly long _beginning;
        private static long _current;
        private static readonly object _latch = new object();
        private static int _left;

        static UnsafeStringContainer()
        {
            _beginning = (long)Marshal.AllocHGlobal(TotalLength);
            _current = _beginning;
            _left = TotalLength;
        }

        public static Utf8String GetString(string value, bool quote)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Utf8String.EmptyString;
            }

            Span<byte> buffer = stackalloc byte[value.Length * 2 + 10];

            int length = 0;
            if (quote)
            {
                buffer[0] = (byte)'"';
                length++;
            }

            length += Encoding.UTF8.GetBytes(value, buffer.Slice(length));

            if (quote)
            {
                buffer.Slice(length)[0] = (byte) '"';
                length++;
            }

            byte* p = GetPointer(length);
            Span<byte> destination = new Span<byte>(p, length);
            buffer.Slice(0, length).CopyTo(destination);

            var utf8String = new Utf8String(p, length);
            return utf8String;
        }

        public static Utf8String Clone(ReadOnlySpan<byte> source)
        {
            byte* p = GetPointer(source.Length);
            Span<byte> destination = new Span<byte>(p, source.Length);
            source.CopyTo(destination);

            var utf8String = new Utf8String(p, source.Length);
            return utf8String;
        }

        private static byte* GetPointer(int length)
        {
            lock (_latch)
            {
                if (_left < length)
                {
                    throw new ArgumentOutOfRangeException(nameof(length),"String container is exhausted");
                }

                var result = _current;

                _current += length;
                _left -= length;

                return (byte*) result;
            }

        }

        public static void PrintUsage()
        {
            Console.WriteLine($"Utf8 string: {(_left * 1.0 / TotalLength):P1} free");
        }
    }
}