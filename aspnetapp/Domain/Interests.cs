using System;
using System.Collections;
using System.Collections.Generic;
using aspnetapp.Collections;

namespace aspnetapp
{
    public unsafe struct Interests : IEquatable<Interests>, IReadOnlyCollection<ushort>
    {
        private fixed ushort Buffer[11];
        private readonly byte _count;

        public ushort First => Buffer[0];
        public ushort Last => Buffer[Count - 1];

        public Interests(ushort[] interests)
        {
            Array.Sort(interests);

            for (var i = 0; i < interests.Length; i++)
            {
                Buffer[i] = interests[i];
            }

            _count = (byte)interests.Length;
        }

        public Interests(HList<ushort> interests)
        {
            interests.Sort();

            for (var i = 0; i < interests.Count; i++)
            {
                Buffer[i] = interests[(uint) i];
            }

            _count = (byte)interests.Count;
        }

        public bool Equals(Interests other)
        {
            if (_count != other._count)
            {
                return false;
            }

            fixed (ushort* thisP = this)
            fixed (ushort* otherP = other)
            {
                var thisSpan = new Span<ushort>(thisP, _count);
                var otherSpan = new Span<ushort>(otherP, _count);
                return thisSpan.SequenceEqual(otherSpan);
            }
        }

        public void Add(ushort index)
        {
            //TODO: Finish it to remove string split and hlist from abstract query
            fixed (ushort* thisP = this)
            {
                //TODO: Optimize with usage of index of any
                var thisSpan = new Span<ushort>(thisP, 11);
                //thisSpan.in
            }
        }

        public bool ContainsAll(Interests other)
        {
            if (Count < other.Count)
            {
                return false;
            }

            if (Last < other.First || other.Last < First)
            {
                return false;
            }

            uint thisIndex = 0;
            uint otherIndex = 0;
            var intersectCount = 0;
            while (thisIndex < Count && otherIndex < other.Count)
            {
                ref ushort thisIn = ref Buffer[thisIndex];
                ref ushort otherIn = ref other.Buffer[otherIndex];

                if (thisIn < otherIn)
                {
                    thisIndex++;
                    continue;
                }

                if (otherIn < thisIn)
                {
                    otherIndex++;
                    continue;
                }

                intersectCount++;
                thisIndex++;
                otherIndex++;
            }

            return other.Count == intersectCount;
        }

        public bool ContainsAny(Interests other)
        {
            if (Count == 0 || other.Count == 0)
            {
                return false;
            }

            if (Last < other.First || other.Last < First)
            {
                return false;
            }

            fixed (ushort* thisP = this)
            {
                //TODO: Optimize with usage of index of any
                var thisSpan = new Span<ushort>(thisP, _count);
                foreach (var candidate in other)
                {
                    if (thisSpan.BinarySearch(candidate) >= 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool Contains(ushort interestIndex)
        {
            if (Count == 0 )
            {
                return false;
            }

            /*if (Last < interestIndex || interestIndex < First)
            {
                return false;
            }*/
            fixed (ushort* thisP = this)
            {
                var thisSpan = new Span<ushort>(thisP, _count);
                return thisSpan.BinarySearch(interestIndex) >= 0;
            }
        }

        private ref ushort GetPinnableReference() => ref Buffer[0];

        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<ushort> IEnumerable<ushort>.GetEnumerator() => GetEnumerator();

        public override bool Equals(object obj) => obj is Interests other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                fixed (ushort* p = this)
                {
                    return (((long*)p)[0].GetHashCode() * 397) ^ Count;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static bool operator ==(Interests left, Interests right) => left.Equals(right);

        public static bool operator !=(Interests left, Interests right) => !left.Equals(right);

        public byte Count => _count;

        int IReadOnlyCollection<ushort>.Count => _count;

        public uint IntersectCount(Interests other)
        {
            uint thisIndex = 0;
            uint otherIndex = 0;
            uint intersectCount = 0;
            while (thisIndex < Count && otherIndex < other.Count)
            {
                ref ushort thisIn = ref Buffer[thisIndex];
                ref ushort otherIn = ref other.Buffer[otherIndex];

                if (thisIn < otherIn)
                {
                    thisIndex++;
                    continue;
                }

                if (otherIn < thisIn)
                {
                    otherIndex++;
                    continue;
                }

                intersectCount++;
                thisIndex++;
                otherIndex++;
            }

            return intersectCount;
        }

        public struct Enumerator : IEnumerator<ushort>
        {
            private readonly Interests _interests;
            private sbyte _current;

            public Enumerator(Interests interests)
            {
                _interests = interests;
                _current = -1;
            }

            public bool MoveNext()
            {
                _current++;
                return _current < _interests.Count;
            }

            public ushort Current => _interests.Buffer[_current];

            public void Dispose()
            {
            }

            public void Reset() => throw new NotImplementedException();

            object IEnumerator.Current => Current;
        }

        public override string ToString() => string.Join(", ", this);
    }
}