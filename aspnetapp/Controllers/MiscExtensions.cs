using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using aspnetapp.Collections;
using Microsoft.AspNetCore.Http;

namespace aspnetapp.Controllers
{
    public static class MiscExtensions
    {
        public static IEnumerable<uint> IntersectWith(this HList<uint> self, HList<uint> other)
        {
            var iterated = self.Count < other.Count ? self : other;
            var searchable = self.Count < other.Count ? other : self;

            int currentIndex = 0;
            foreach (uint value in iterated)
            {
                currentIndex = searchable.BinarySearch(currentIndex, searchable.Count - currentIndex, value, HList<uint>.Descending);
                if (currentIndex >= 0)
                {
                    yield return value;
                    continue;
                }

                currentIndex = ~currentIndex;
                if (currentIndex == searchable.Count)
                {
                    yield break;
                }
            }
        }

        public static int IntersectWithCount(this HList<uint> self, HList<uint> other)
        {
            var iterated = self.Count < other.Count ? self : other;
            var searchable = self.Count < other.Count ? other : self;

            int currentIndex = 0;
            var result = 0;
            foreach (uint value in iterated)
            {
                currentIndex = searchable.BinarySearch(currentIndex, searchable.Count - currentIndex, value, HList<uint>.Descending);
                if (currentIndex >= 0)
                {
                    result++;
                    continue;
                }

                currentIndex = ~currentIndex;
                if (currentIndex == searchable.Count)
                {
                    break;
                }
            }

            return result;
        }





        public static void ReadBytes(this HttpContext self, Span<byte> to)
        {
            int received = 0;
            while (received < to.Length)
            {
                received += self.Request.Body.Read(to.Slice(received));
            }
        }
    }
}