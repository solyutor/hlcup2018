using System;
using System.Runtime.InteropServices;
using System.Text;

namespace aspnetapp.Serializer
{
    public static class UDecoder
    {
        private static readonly UTF8Encoding Encoding = new UTF8Encoding(false, false);
        private static sbyte[] tbl = {
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, -1, -1, -1, -1, -1, -1,
            -1, 10, 11, 12, 13, 14, 15, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 10,
            11, 12, 13, 14, 15, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1
        };

        public static string GetString(Span<byte> span)
        {
            var length = utf_decode(span);
            return Encoding.GetString(span.Slice(0, length));
        }


        public static int utf_decode(Span<byte> source)
        {
            var output = source;
            var length = output.Length;
            var input = MemoryMarshal.Cast<byte, sbyte>(source);

            while (input.Length != 0)
            {
                if (input[0] == '\\' && input[1] == 'u')
                {
                    if (input[2] == '0' && input[3] == '\\')
                    {
                        input = input.Slice(3);
                        continue;
                    }
                    ushort u = (ushort)(tbl[input[2]] << 12 | tbl[input[3]] << 8 | tbl[input[4]] << 4 | tbl[input[5]]);
                    // Все ASCII символы оставляем как есть
                    if (u < 255)
                    {
                        output[0] = (byte)u;
                        output = output.Slice(1);
                    }
                    else
                    {
                        ushort w;
                        // < 'р'
                        if (u >= 0x0410 && u <= 0x043f)
                        {
                            w = (ushort)(u - 0x0410 + 0xd090);
                            // >= 'р'
                        }
                        else
                        {
                            w = (ushort)(u - 0x0440 + 0xd180);
                        }

                        output[0] = (byte)(w >> 8);
                        output[1] = (byte)w;

                        output = output.Slice(2);
                    }

                    input = input.Slice(6);
                }
                else
                {
                    //Is there a way to do it without
                    output[0] =  MemoryMarshal.Cast<sbyte, byte>(input)[0];
                    input = input.Slice(1);
                    output = output.Slice(1);
                }
            }

            return length - output.Length;
        }
    }
}