using System;
using System.Collections;
using System.Collections.Generic;

namespace aspnetapp.Collections
{
    public class MultiUnionList<T> : IEnumerable<T>, IEnumerator<T>
        where T : IComparable<T>, IEquatable<T>
    {
        private readonly HList<IEnumerable<T>> _list;
        private readonly HList<IEnumerator<T>> _enums;
        private readonly HList<IEnumerator<T>> _toMove;

        public MultiUnionList()
        {
            _list = new HList<IEnumerable<T>>(4);

            _enums =  new HList<IEnumerator<T>>();
            _toMove = new HList<IEnumerator<T>>();
        }

        public MultiUnionList(IEnumerable<IEnumerable<T>> list) : this()
        {
            _list.AddRange(list);
        }

        public MultiUnionList<T> Add(IEnumerable<T> list)
        {
            _list.Add(list);
            return this;
        }

        public IEnumerator<T> GetEnumerator()
        {
            _enums.Clear();
            _toMove.Clear();

            foreach (var enumerable in _list)
            {
                IEnumerator<T> enumerator = enumerable.GetEnumerator();
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
            if (_enums.Count != 0)
            {
                Current = default;
                foreach (var enumerator in _enums)
                {
                    var compareResult = enumerator.Current.CompareTo(Current);
                    if (compareResult == 0)
                    {
                        _toMove.Add(enumerator);
                    }
                    else if (compareResult > 0)
                    {
                        _toMove.ClearFast();
                        Current = enumerator.Current;
                        _toMove.Add(enumerator);
                    }

                }

                foreach (var enumerator in _toMove)
                {
                    if (!enumerator.MoveNext())
                    {
                        _enums.Remove(enumerator);
                    }

                }
                _toMove.ClearFast();

                return true;
            }
            _toMove.Clear();
            return false;
        }

        public bool MoveNext(out int count)
        {
            if (_enums.Count != 0)
            {
                Current = default;
                foreach (var enumerator in _enums)
                {
                    var compareResult = enumerator.Current.CompareTo(Current);
                    if (compareResult == 0)
                    {
                        _toMove.Add(enumerator);
                    }
                    else if (compareResult > 0)
                    {
                        _toMove.Clear();
                        Current = enumerator.Current;
                        _toMove.Add(enumerator);
                    }

                }

                foreach (var enumerator in _toMove)
                {
                    if (!enumerator.MoveNext())
                    {
                        _enums.Remove(enumerator);
                    }

                }

                count = _toMove.Count;
                _toMove.Clear();
                return true;
            }

            count = 0;
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

        public void Clear()
        {
            _list.Clear();
            _enums.Clear();
            _toMove.Clear();
        }
    }
}