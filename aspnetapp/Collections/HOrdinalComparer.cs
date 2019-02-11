using System;

namespace aspnetapp.Collections
{
    public sealed class HOrdinalComparer : StringComparer
    {
        public static readonly HOrdinalComparer Instance = new HOrdinalComparer();

        private HOrdinalComparer()
        {
        }

        public override int Compare(string x, string y) => string.CompareOrdinal(x, y);

        public override bool Equals(string x, string y) => x.Equals(y);

        public override int GetHashCode(string obj) => obj.GetHashCode();

        // Equals method for the comparer itself.
        public override bool Equals(object obj) => obj is HOrdinalComparer;

        public override int GetHashCode() => nameof(HOrdinalComparer).GetHashCode();
    }
}