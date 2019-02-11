using System.Collections;
using System.Collections.Generic;

namespace aspnetapp.Collections
{
    public struct BackwardArrayEnumerator<T> : IEnumerator<T>
    {
        private T[] _array;
        private uint _index;
        private T _current;
        private uint _lower;

        public BackwardArrayEnumerator(T[] array, uint lower, uint upper)
        {
            _array = array;
            _index = upper;
            _lower = lower;
            _current = default;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            var local = _array;

            if (_lower <= _index &&  _index < (uint)local.Length)
            {
                _current = local[_index];
                _index--;
                return true;
            }
            return MoveNextRare();
        }

        public void Reset() => throw new System.NotImplementedException();

        private bool MoveNextRare()
        {
            _index--;
            _current = default;
            return false;
        }

        public T Current => _current;

        void IEnumerator.Reset()
        {
            _index = 0;
            _current = default;
        }

        object IEnumerator.Current => Current;
    }
}