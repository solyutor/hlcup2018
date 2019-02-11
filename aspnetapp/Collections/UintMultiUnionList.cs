using System.Collections;
using System.Collections.Generic;

namespace aspnetapp.Collections
{
    public class UintMultiUnionList : IEnumerable<uint>, IEnumerator<uint>
    {
        private readonly HList<HList<uint>> _list;
        private readonly HList<HList<uint>.Enumerator> _enums;
        private uint _current;
        private Enumerator5 _5Enum;

        public UintMultiUnionList()
        {
            _list = new HList<HList<uint>>(5);
            _enums =  new HList<HList<uint>.Enumerator>();
            _5Enum = new Enumerator5(_list);
        }

        public UintMultiUnionList(IEnumerable<HList<uint>> list) : this()
        {
            _list.AddRange(list);
        }

        public UintMultiUnionList Add(HList<uint> list)
        {
            if (list.Count > 0)
            {
                _list.Add(list);
            }
            return this;
        }

        public IEnumerator<uint> GetEnumerator()
        {
            _enums.Clear();

            foreach (var enumerable in _list)
            {
                HList<uint>.Enumerator enumerator = enumerable.GetEnumerator();
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
                _current = default;
                for (int i = 0; i < _enums.Count; i++)
                {
                    ref HList<uint>.Enumerator enumerator = ref _enums.GetByRef(i);
                    if (enumerator.Current > _current)
                    {
                        _current = enumerator.Current;
                    }
                }

                for (int i = 0; i < _enums.Count; i++)
                {
                    ref HList<uint>.Enumerator enumerator = ref _enums.GetByRef(i);
                    if (enumerator.Current == _current && !enumerator.MoveNext())
                    {
                        _enums.Remove(enumerator);
                        i--;//Dirty hack to make list work after removal
                    }
                }
                return true;
            }
            return false;
        }

        public void Reset()
        {
            _current = default;
        }

        public uint Current => _current;

        object IEnumerator.Current => _current;

        public void Dispose()
        {
        }

        public void Clear()
        {
            _list.Clear();
            _enums.Clear();
        }
    }
}