using System;
using System.Runtime.InteropServices;

namespace aspnetapp.Controllers
{
    public static unsafe class Buffer
    {
        public const int Length = 8192;

        [ThreadStatic]
        private static byte* _buffer;

        public static byte* My
        {
            get
            {
                if (_buffer != null)
                {
                    return _buffer;
                }

                return _buffer = (byte*) Marshal.AllocHGlobal(Length);
            }
        }

        public static ParseContext Context => new ParseContext(My, Length);
    }
}