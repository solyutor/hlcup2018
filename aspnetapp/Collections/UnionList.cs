using System;
using System.Collections;
using System.Collections.Generic;

namespace aspnetapp.Collections
{
    public class UnionList<T> : IEnumerable<T>, IEnumerator<T>
        where T : IComparable<T>, IEquatable<T>
    {
        private readonly IEnumerable<T> _first;
        private readonly IEnumerable<T> _second;
        private IEnumerator<T> _firstEnum;
        private IEnumerator<T> _secondEnum;
        private bool _firstNext;
        private bool _secondNext;
        private bool _firstRun;


        public UnionList(IEnumerable<T> first, IEnumerable<T> second)
        {
            _first = first;
            _second = second;
        }

        public IEnumerator<T> GetEnumerator()
        {
            _firstEnum = _first.GetEnumerator();
            _secondEnum = _second.GetEnumerator();

            _firstRun = true;
            _firstNext = true;
            _secondNext = true;

            return this;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        public bool MoveNext()
        {
            if (_firstRun)
            {
                _firstRun = false;
                _firstNext = _firstEnum.MoveNext();
                _secondNext = _secondEnum.MoveNext();

                Current = _firstEnum.Current.CompareTo(_secondEnum.Current) < 0
                    ? _secondEnum.Current
                    : _firstEnum.Current;

                return true;
            }

            //both have values
            if (_firstNext && _secondNext)
            {
                var firstCompare = _firstEnum.Current.CompareTo(Current);
                if (firstCompare == 0)
                {
                    _firstNext = _firstEnum.MoveNext();
                }

                var secondCompare = _secondEnum.Current.CompareTo(Current);
                if (secondCompare == 0)
                {
                    _secondNext = _secondEnum.MoveNext();
                }

                Current = _firstEnum.Current.CompareTo(_secondEnum.Current) < 0
                    ? _secondEnum.Current
                    : _firstEnum.Current;

                return true;
            }

            if (_firstNext)
            {
                if (_firstNext = _firstEnum.MoveNext())
                {
                    Current = _firstEnum.Current;
                    return true;
                }

                return false;
            }

            if (_secondNext)
            {
                if (_secondNext = _secondEnum.MoveNext())
                {
                    Current = _secondEnum.Current;
                    return true;
                }
            }

            return false;
        }

        public void Reset()
        {
            Current = default;
        }

        public T Current { get; private set; }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }
}

