using System.Collections;
using System.Collections.Generic;

namespace aspnetapp.Collections
{
    public class BackwardArray<T> : IReadOnlyCollection<T>
    {
        private readonly T[] _array;
        private readonly uint _lower;
        private readonly uint _upper;

        public BackwardArray(T[] array, uint lower, uint upper)
        {
            _array = array;
            _lower = lower;
            _upper = upper;
        }

        public BackwardArrayEnumerator<T> GetEnumerator() => new BackwardArrayEnumerator<T>(_array, _lower, _upper);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => (int) (_upper - _lower);
    }
}