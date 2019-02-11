using System;
using System.Collections;
using System.Collections.Generic;

namespace aspnetapp.Collections
{
    public class Enumerator5 : IEnumerator<uint>
    {
        private readonly HList<HList<uint>> _list;

        private HList<uint>.Enumerator _enum0;
        private HList<uint>.Enumerator _enum1;
        private HList<uint>.Enumerator _enum2;
        private HList<uint>.Enumerator _enum3;
        private HList<uint>.Enumerator _enum4;


        private bool _move0;
        private bool _move1;
        private bool _move2;
        private bool _move3;
        private bool _move4;

        private uint _current;

        public Enumerator5(HList<HList<uint>> list)
        {
            _list = list;
        }

        public bool MoveNext()
        {
            uint current = default;
            if (_move0)
            {
                current = _enum0.Current;
            }

            if (_move1 && current < _enum1.Current)
            {
                current = _enum1.Current;
            }

            if (_move2 && current < _enum2.Current)
            {
                current = _enum2.Current;
            }

            if (_move3 && current < _enum3.Current)
            {
                current = _enum3.Current;

            }

            if (_move4 && current < _enum4.Current)
            {
                current = _enum4.Current;
            }

            if (current == default)
            {
                return false;
            }

            if (current == _enum0.Current)
            {
                _move0 = _enum0.MoveNext();
            }

            if (current == _enum1.Current)
            {
                _move1 = _enum1.MoveNext();
            }

            if (current == _enum2.Current)
            {
                _move2 = _enum2.MoveNext();
            }

            if (current == _enum3.Current)
            {
                _move3 = _enum3.MoveNext();
            }

            if (current == _enum4.Current)
            {
                _move4 = _enum4.MoveNext();
            }

            _current = current;
            return true;

        }

    public void Reset()
        {
            _enum0 = _list[0].GetEnumerator();
            _enum1 = _list[1].GetEnumerator();
            _enum2 = _list[2].GetEnumerator();
            _enum3 = _list[3].GetEnumerator();
            _enum4 = _list[4].GetEnumerator();

            _move0 = _enum0.MoveNext();
            _move1 = _enum1.MoveNext();
            _move2 = _enum2.MoveNext();
            _move3 = _enum3.MoveNext();
            _move4 = _enum4.MoveNext();
        }

        public uint Current { get; }

        object IEnumerator.Current => Current;

        public void Dispose() => Reset();
    }
}