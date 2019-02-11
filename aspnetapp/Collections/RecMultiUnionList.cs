using System.Collections;
using System.Collections.Generic;
using aspnetapp.Domain;

namespace aspnetapp.Collections
{
    public class RecMultiUnionList
    {
        private readonly HList<HList<RecommendIndexEntry>> _list;
        private readonly HList<HList<RecommendIndexEntry>.Enumerator> _enums;
        private RecommendIndexEntry _current;

        public RecMultiUnionList()
        {
            _list = new HList<HList<RecommendIndexEntry>>(5);
            _enums =  new HList<HList<RecommendIndexEntry>.Enumerator>();
        }

        public RecMultiUnionList(IEnumerable<HList<RecommendIndexEntry>> list) : this()
        {
            _list.AddRange(list);
        }

        public RecMultiUnionList Add(HList<RecommendIndexEntry> list)
        {
            if (list.Count > 0)
            {
                _list.Add(list);
            }
            return this;
        }

        public void Prepare()
        {
            _enums.Clear();

            foreach (var enumerable in _list)
            {
                HList<RecommendIndexEntry>.Enumerator enumerator = enumerable.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    _enums.Add(enumerator);
                }
            }
        }

        public bool MoveNext(out int count)
        {
            count = 0;
            if (_enums.Count != 0)
            {
                _current = default;
                for (int i = 0; i < _enums.Count; i++)
                {
                    ref HList<RecommendIndexEntry>.Enumerator enumerator = ref _enums.GetByRef(i);
                    if (enumerator.Current > _current)
                    {
                        count = 0;
                        _current = enumerator.Current;
                    }
                }

                for (int i = 0; i < _enums.Count; i++)
                {
                    ref HList<RecommendIndexEntry>.Enumerator enumerator = ref _enums.GetByRef(i);
                    if (enumerator.Current == _current )
                    {
                        count++;
                        if (!enumerator.MoveNext())
                        {
                            _enums.RemoveAt(i);
                            i--;//Dirty hack to make list work after removal
                        }
                    }
                }

                return true;
            }
            return false;
        }

        public ref RecommendIndexEntry Current => ref _current;


        public void Clear()
        {
            _list.Clear();
            _enums.Clear();
        }
    }
}