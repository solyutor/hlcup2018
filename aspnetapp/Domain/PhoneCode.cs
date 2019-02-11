using System;
using System.Text;
using aspnetapp.Sys;

namespace aspnetapp
{
    public unsafe struct PhoneCode
    {
        public fixed byte Buffer[3];

        private PhoneCode(string value)
        {
            fixed (byte* p = Buffer)
            {
                Span<byte> span = new Span<byte>(p, 3);
                Encoding.UTF8.GetBytes(value, span);
            }
        }

        public Span<byte> GetSpan()
        {
            fixed (byte* p = Buffer)
            {
                return new Span<byte>(p, 3);
            }
        }

        public static PhoneCode From(string value)
        {
            PhoneCode result = default;
            var span = result.GetSpan();
            span[0] = (byte)value[0];
            span[1] = (byte)value[1];
            span[2] = (byte)value[2];
            return result;
        }

        public bool IsPrefixOf(in Utf8String phone)
        {
            return !phone.IsEmpty && phone.Span.Slice(3, 3).SequenceEqual(GetSpan());
        }
    }
}