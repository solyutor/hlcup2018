using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using aspnetapp.Serializer;
using FluentAssertions;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class UDecoderTests
    {
        [TestCase(@"\u0420\u043e\u0441\u043e\u0440\u0438\u0436")]
        [TestCase(@"\u0432\u0441\u0451 \u0441\u043b\u043e\u0436\u043d\u043e")]
        [TestCase(@"\u0\u0420\u043e\u0441\u043e\u0440\u0438\u0436")]
        public void Should_decode_string(string origin)
        {
            var originString = Regex.Unescape(origin.Replace(@"\u0\", "\\"));
            Span<byte> originBytes = Encoding.UTF8.GetBytes(origin);


            var length = UDecoder.utf_decode(originBytes);


            var decodedString = Encoding.UTF8.GetString(originBytes.Slice(0, length));
            decodedString.Should().BeEquivalentTo(originString);
        }
    }
}