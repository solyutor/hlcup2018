using System.Collections;
using System.Collections.Generic;

namespace aspnetapp.Collections
{
    public class UintIntersectList : IEnumerable<uint>, IEnumerator<uint>
    {
        private readonly List<HList<uint>> _list;
        private readonly HList<uint> _positions;
        private uint _current;
        private bool _empty;

        public UintIntersectList()
        {
            _list = new List<HList<uint>>(5);
            _positions =  new HList<uint>();
        }

        public UintIntersectList(IEnumerable<HList<uint>> list) : this()
        {
            _list.AddRange(list);
        }

        public UintIntersectList Add(HList<uint> list)
        {
            if (list.Count == 0)
            {
                _empty = true;
            }
            _list.Add(list);

            return this;
        }

        public IEnumerator<uint> GetEnumerator()
        {
            Reset();

            foreach (var enumerable in _list)
            {
                _positions.Add(0);
            }

            return this;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        public bool MoveNext()
        {
            var firstPosition = _positions[0];
            if (_list[0].Count <= firstPosition)
            {
                return false;
            }
            _current = _list[0][firstPosition];
            for (int currentListIndex = 0; currentListIndex < _positions.Count; currentListIndex++)
            {
                var currentList = _list[currentListIndex];
                ref uint currentPosition = ref _positions.GetByRef( currentListIndex);

                while (_current < currentList[currentPosition])
                {
                    currentPosition++;
                    if (currentPosition == currentList.Count)
                    {
                        return false;
                    }
                }

                if (currentList[currentPosition] < _current)
                {
                    _current = currentList[currentPosition];
                    currentListIndex = -1;// startOver
                }
            }

            _positions[0]++;
            return true;
        }

        public void Reset()
        {
            _positions.Clear();
            _empty = false;
            _current = default;
            _list.Sort((x,y) => x.Count.CompareTo(y.Count));
        }

        public uint Current => _current;

        object IEnumerator.Current => _current;

        public void Dispose()
        {
        }

        public void Clear()
        {
            _list.Clear();
            _positions.Clear();
            _empty = false;
        }
    }
}