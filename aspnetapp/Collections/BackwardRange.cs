using System.Collections;
using System.Collections.Generic;

namespace aspnetapp.Collections
{
    public class BackwardRange : IReadOnlyCollection<uint>
    {
        private readonly uint _upper;

        public BackwardRange(uint upper)
        {
            _upper = upper;
        }

        public BackwardRangeEnumerator GetEnumerator() => new BackwardRangeEnumerator(_upper);

        IEnumerator<uint> IEnumerable<uint>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => (int) (_upper);
    }

    public struct BackwardRangeEnumerator : IEnumerator<uint>
    {
        private uint _current;

        public BackwardRangeEnumerator(uint upper)
        {
            _current = upper;
            _current++;
        }

        public bool MoveNext()
        {
            _current--;
            return _current > 0;
        }

        public void Reset()
        {

        }

        public uint Current => _current;

        object IEnumerator.Current => Current;

        public void Dispose() {

        }
    }
}