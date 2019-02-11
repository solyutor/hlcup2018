using System;
using System.Text;

namespace aspnetapp.Controllers
{
    public unsafe struct Prefix
    {
        private fixed byte buffer[7];
        private  byte length;
        public Prefix(string value)
        {

            fixed (byte* p = buffer)
            {
                Span<byte> span = new Span<byte>(p, 3);
                length = (byte) Encoding.UTF8.GetBytes(value, span);
            }

        }

        public Span<byte> Span
        {
            get
            {
                fixed (byte* p = buffer)
                {
                    return new Span<byte>(p, 3);
                }
            }
        }
    }
}