using System;
using System.Collections;
using System.Collections.Generic;

namespace aspnetapp.Collections
{
    public class IntersectList<T> : IEnumerable<T>, IEnumerator<T>
        where T : IComparable<T>, IEquatable<T>
    {
        private readonly IEnumerable<T> _first;
        private readonly IEnumerable<T> _second;
        private IEnumerator<T> _firstEnum;
        private bool _firstNext;
        private IEnumerator<T> _secondEnum;
        private bool _secondNext;


        public IntersectList(IEnumerable<T> first, IEnumerable<T> second)
        {
            _first = first;
            _second = second;
        }

        public int Count()
        {
            var result = 0;
            foreach (var _ in this)
            {
                result++;
            }

            return result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            _firstEnum = _first.GetEnumerator();
            _secondEnum = _second.GetEnumerator();

            _firstEnum.MoveNext();
            _secondEnum.MoveNext();

            _firstNext = true;
            _secondNext = true;

            return this;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        public bool MoveNext()
        {
            if (!_firstNext || !_secondNext)
            {
                return false;
            }

            var compared = 0;
            while ((compared = _firstEnum.Current.CompareTo(_secondEnum.Current)) != 0)
            {
                if (compared > 0)
                {
                    if (_firstNext = _firstEnum.MoveNext())
                    {
                        continue;
                    }

                    return false;
                }

                if (_secondNext = _secondEnum.MoveNext())
                {
                    continue;
                }

                return false;
            }

            Current = _firstEnum.Current;
            _firstNext = _firstEnum.MoveNext();
            _secondNext = _secondEnum.MoveNext();
            return true;
        }

        public void Reset() => Current = default;

        public T Current { get; private set; }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }
}