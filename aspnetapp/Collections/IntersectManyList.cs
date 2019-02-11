using System;
using System.Collections;
using System.Collections.Generic;

namespace aspnetapp.Collections
{
    public class IntersectManyList<T> : IEnumerable<T>, IEnumerator<T>
        where T : IComparable<T>, IEquatable<T>
    {
        private IEnumerable<HList<T>> _source;
        private HList<HList<T>.Enumerator> _enums;


        public IntersectManyList(IEnumerable<HList<T>> source)
        {
            _source = source;
            _enums = new HList<HList<T>.Enumerator>();
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var list in _source)
            {
                var enumerator = list.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    _enums.Add(enumerator);
                }
            }

            return this;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        public bool MoveNext()
        {
/*
            if (_enums.Count == 0)
            {
                return false;
            }





            int compared = 0;
            while((compared = _firstEnum.Current.CompareTo(_secondEnum.Current)) != 0)
            {
                if (compared > 0)
                {
                    if (_firstNext = _firstEnum.MoveNext())
                    {
                        continue;
                    }

                    return false;
                }
                else
                {
                    if (_secondNext = _secondEnum.MoveNext())
                    {
                        continue;
                    }

                    return false;
                }
            }

            Current = _firstEnum.Current;
            _firstNext = _firstEnum.MoveNext();
            _secondNext = _secondEnum.MoveNext();
            return true;
*/
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