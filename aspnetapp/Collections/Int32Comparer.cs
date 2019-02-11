using System.Collections.Generic;

namespace aspnetapp.Collections
{
    public class Int32Comparer : IEqualityComparer<int>
    {
        public static readonly Int32Comparer Instance = new Int32Comparer();
        private Int32Comparer()
        {

        }
        public bool Equals(int x, int y) => x == y;

        public int GetHashCode(int obj) => obj;
    }
    public class Uint32Comparer : IEqualityComparer<uint>
    {
        public static readonly Uint32Comparer Instance = new Uint32Comparer();
        private Uint32Comparer()
        {

        }
        public bool Equals(uint x, uint y) => x == y;

        public int GetHashCode(uint obj) => obj.GetHashCode();
    }
}